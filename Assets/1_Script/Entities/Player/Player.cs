using System.Collections;
using System.Collections.Generic;
using ProjectJS.PStats;
using ProjectJS.Utils;
using ProjectJS.Controller;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] private PlayerStats BaseStats;

    [Header("Weapon System")]
    [SerializeField] private List<WeaponData> AvailableWeapons;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public Vector2 attackSize;
    public float attackOffset = 1.0f;
    public LayerMask enemyLayer;

    [Header("Weapon Visuals")]
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;
    [SerializeField] private List<Sprite> weaponSprites;

    [Header("Guard Settings")]
    [SerializeField] private Vector2 guardSize = new Vector2(1.5f, 2.0f);
    [SerializeField] private float guardOffset = 0.5f;

    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 1.0f;
    private float lastHitTime = -1.0f;
    private Coroutine invincibilityCoroutine;

    private NetworkVariable<float> curHealth = new NetworkVariable<float>();
    private NetworkVariable<float> curGuardGauge = new NetworkVariable<float>();
    private NetworkVariable<int> currentWeaponIndex = new NetworkVariable<int>(0);

    public bool IsDead { get; private set; } = false;
    public bool IsGuarding { get; private set; } = false;
    private float guardStartTime = 0.0f;
    private float nextAttackTime = 0.0f;
    private WeaponData currentWeapon;
    private Animator anim;
    private Rigidbody2D rb;
    private bool isHitStopping = false;

    public bool IsHitStopping => isHitStopping;
    public WeaponData CurrentWeapon => currentWeapon;
    public PlayerStats Stats => BaseStats;
    public float CurGuardGauge => curGuardGauge.Value;
    public float CurHealthGauge => curHealth.Value; //[jh] 체력 바를 위한 게이지 프로퍼티
    public Vector2 FacingDirection { get; set; } = Vector2.right;

    //[jh] 게이지 UI에서 연결하기 위한 이벤트
    public event System.Action<float, float, float> OnHealthChangedEvent;
    public event System.Action<float, float, float> OnGuardGaugeChangedEvent;

    public override void OnNetworkSpawn()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentWeaponIndex.OnValueChanged += UpdateWeaponVisual;

        curHealth.OnValueChanged += OnCurHealthChanged;
        curGuardGauge.OnValueChanged += OnCurGuardGaugeChanged;

        if (IsOwner) 
        {   
            int WeaponChoice = ProjectJS.PStats.PlayerWeaponSelection.SelectedWeaponIndex;
            if (WeaponChoice < 0) WeaponChoice = 0;

            SetWeaponRpc(WeaponChoice);

            UpdateWeaponVisual(-1, WeaponChoice);
        }
        else if (currentWeaponIndex.Value != -1)
        {   
            UpdateWeaponVisual(-1, currentWeaponIndex.Value);
        }

        ProjectJS.UI.GameScene.GameSceneUI.Instance?.RegisterPlayer(this);

        if (IsServer)
        {
            StartCoroutine(GuardGaugeRegenRoutine());
        }
    }

    private IEnumerator GuardGaugeRegenRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            
            if (!IsDead && !IsGuarding)
            {
                if (curGuardGauge.Value < BaseStats.MaxGuardGauge)
                {
                    curGuardGauge.Value = Mathf.Min(curGuardGauge.Value + 1f, BaseStats.MaxGuardGauge);
                }
            }
        }
    }

    //[jh] 테스트용 플레이어 UI 연결
    [ContextMenu("TestUIConnect")]
    public void TestUIConnect()
    {
        ProjectJS.UI.GameScene.GameSceneUI.Instance?.RegisterPlayer(this);
    }

    public void OnAttackHit()
    {
        Debug.Log("[Player] OnAttackHit called!");
        if (currentWeapon == null || IsDead) return;

        Vector2 hitCenter = (Vector2)transform.position + (FacingDirection * attackOffset);
        if (attackPoint != null) hitCenter.y = attackPoint.position.y;

        Vector2 effectiveSize = new Vector2(attackSize.x * currentWeapon.AttackRange, attackSize.y);

        Debug.DrawLine((Vector2)transform.position, hitCenter, Color.red, 0.5f);

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(hitCenter, effectiveSize, 0f, Constants.LAYER_BOSS);
        Debug.Log($"[Player] OverlapBoxAll found {hitEnemies.Length} enemies in LAYER_BOSS");

        if (hitEnemies.Length > 0)
        {
            if (currentWeapon.AttackVfxPrefab != null)
            {
                Instantiate(currentWeapon.AttackVfxPrefab, hitCenter, Quaternion.identity);
            }

            if (IsOwner)
            {
                StartCoroutine(TriggerHitStop(0.07f));
                if (CameraShake.Instance != null)
                {
                    CameraShake.Instance.Shake(0.1f, 0.2f);
                }
            }
        }

        HashSet<ProjectJS.Controller.BossController> hitBosses = new HashSet<ProjectJS.Controller.BossController>();

        foreach (Collider2D enemy in hitEnemies)
        {
            var boss = enemy.GetComponentInParent<ProjectJS.Controller.BossController>();
            if (boss != null && hitBosses.Add(boss))
            {
                Debug.Log($"[Player] Requesting {currentWeapon.Damage} damage to Boss!");
                boss.RequestTakeDamageServerRpc(currentWeapon.Damage);
            }
        }
    }
    
 
    private System.Collections.IEnumerator TriggerHitStop(float duration)
    {
        if (isHitStopping) yield break;
        isHitStopping = true;
        
        float prevAnimSpeed = anim.speed;
        anim.speed = 0.05f;

        yield return new WaitForSecondsRealtime(duration);

        anim.speed = prevAnimSpeed;
        
        isHitStopping = false;
    }

    public void SetGuarding(bool state)
    {
        if (IsDead) return;
        if (state && !IsGuarding) guardStartTime = Time.time;
        IsGuarding = state;

        if (anim != null)
        {
            SetGuardingRpc(state);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetGuardingRpc(bool state)
    {
        if (anim != null)
        {
            anim.SetBool("IsGuarding", state);
        }
    }

    public void TryAttack()
    {
        if (currentWeapon == null || IsDead) return;

        if (Time.time >= nextAttackTime && !IsGuarding)
        {
            Attack();
            nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, currentWeapon.AttackSpeed);
        }
    }

    private void Attack()
    {
        if (anim != null)
        {
            AttackRpc();
        }

        if (currentWeapon != null && currentWeapon.AttackSfxClip != null)
        {
            AudioSource.PlayClipAtPoint(currentWeapon.AttackSfxClip, transform.position);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AttackRpc()
    {
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }
    }

    [Rpc(SendTo.Server)]
    private void SetWeaponRpc(int Index)
    {
        currentWeaponIndex.Value = Index;
        if (curHealth.Value <= 0) curHealth.Value = BaseStats.MaxHealth;
        if (curGuardGauge.Value <= 0) curGuardGauge.Value = BaseStats.MaxGuardGauge;
    }

    private void UpdateWeaponVisual(int OldIndex, int NewIndex)
    {
        if (AvailableWeapons == null || AvailableWeapons.Count == 0) return;

        if (NewIndex < 0 || NewIndex >= AvailableWeapons.Count) return;

        currentWeapon = AvailableWeapons[NewIndex];
        
        if (anim != null && currentWeapon.WeaponAnimatorController != null)
        {
            anim.runtimeAnimatorController = currentWeapon.WeaponAnimatorController;
        }
        
        if (weaponSpriteRenderer != null && weaponSprites != null && NewIndex < weaponSprites.Count)
        {
            weaponSpriteRenderer.sprite = weaponSprites[NewIndex];
        }
    }
    
    public void TakeDamage(float EnemyDamage, Vector2 attackerPos)
    {
        if (!IsOwner || IsDead) return;

        TakeDamageServerRpc(EnemyDamage, attackerPos);
    }

    [Rpc(SendTo.Server)]
    private void TakeDamageServerRpc(float EnemyDamage, Vector2 attackerPos)
    {
        if (Time.time - lastHitTime < invincibilityDuration) return;

        Vector2 guardCenter = (Vector2)transform.position + (FacingDirection * guardOffset);
        bool isAttackerInGuardArea = Mathf.Abs(attackerPos.x - guardCenter.x) <= guardSize.x / 2f &&
                                     Mathf.Abs(attackerPos.y - guardCenter.y) <= guardSize.y / 2f;

        if (IsGuarding && curGuardGauge.Value > 0 && isAttackerInGuardArea)
        {
            if (Time.time - guardStartTime <= 0.2f)
            {
                curGuardGauge.Value -= (EnemyDamage * 0.5f);
                PlayJustGuardEffectRpc();
            }
            else
            {
                float blockedDamage = EnemyDamage * 0.5f;
                curGuardGauge.Value -= blockedDamage;
            }
        }
        else
        {
            curHealth.Value -= EnemyDamage;
            lastHitTime = Time.time;
            StartInvincibilityClientRpc();
        }

        if (curHealth.Value <= 0)
        {
            DieClientRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartInvincibilityClientRpc()
    {
        if (invincibilityCoroutine != null) StopCoroutine(invincibilityCoroutine);
        invincibilityCoroutine = StartCoroutine(InvincibilityFlashRoutine());
    }

    private IEnumerator InvincibilityFlashRoutine()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            foreach (var r in renderers)
            {
                if (r != null)
                {
                    Color c = r.color;
                    c.a = (c.a == 1f) ? 0.5f : 1f;
                    r.color = c;
                }
            }
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        foreach (var r in renderers)
        {
            if (r != null)
            {
                Color c = r.color;
                c.a = 1f;
                r.color = c;
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DieClientRpc()
    {
        Die();
    }

    [Rpc(SendTo.NotMe)]
    private void PlayJustGuardEffectRpc()
    {
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;
        
        if (anim != null)
        {
            anim.SetTrigger("Dead");
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    [Rpc(SendTo.Server)]
    public void RequestRetryServerRpc()
    {
        RespawnClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RespawnClientRpc()
    {
        Respawn();
    }

    private void Respawn()
    {
        IsDead = false;
        
        if (IsServer)
        {
            curHealth.Value = BaseStats.MaxHealth;
            curGuardGauge.Value = BaseStats.MaxGuardGauge;
            lastHitTime = -1.0f;
            StartCoroutine(GuardGaugeRegenRoutine());
        }

        if (anim != null)
        {
            anim.Play("Idle"); // or whatever the default state is
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f); 

        Vector2 currentDir = Application.isPlaying ? FacingDirection : 
                            (transform.localScale.x >= 0 ? Vector2.right : Vector2.left);

        Vector2 hitCenter = (Vector2)transform.position + (currentDir * attackOffset);
        if (attackPoint != null) hitCenter.y = attackPoint.position.y;

        float rangeMultiplier = 1f;
        if (currentWeapon != null) rangeMultiplier = currentWeapon.AttackRange;
        else if (AvailableWeapons != null && AvailableWeapons.Count > 0) rangeMultiplier = AvailableWeapons[0].AttackRange;

        Vector2 effectiveSize = new Vector2(attackSize.x * rangeMultiplier, attackSize.y);
        Gizmos.DrawWireCube(hitCenter, effectiveSize);

        Gizmos.color = new Color(0, 0, 1, 0.5f);
        Vector2 guardCenter = (Vector2)transform.position + (currentDir * guardOffset);
        Gizmos.DrawWireCube(guardCenter, guardSize);
    }

    //[jh] netWorkVariable의 OnValueChanged 이벤트에 구독하기 위한 함수
    private void OnCurHealthChanged(float previousValue, float newValue)
    {
        OnHealthChangedEvent?.Invoke(previousValue, newValue, Stats.MaxHealth);
    }

    private void OnCurGuardGaugeChanged(float previousValue, float newValue)
    {
        OnGuardGaugeChangedEvent?.Invoke(previousValue, newValue, Stats.MaxGuardGauge);
    }

}