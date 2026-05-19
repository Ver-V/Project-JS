using System.Collections.Generic;
using ProjectJS.PStats;
using ProjectJS.Utils;
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
    public LayerMask enemyLayer;

    private NetworkVariable<float> curHealth = new NetworkVariable<float>(); // player's current health
    private NetworkVariable<float> curGuardGauge = new NetworkVariable<float>(); // player's current guard gauge
    private NetworkVariable<int> currentWeaponIndex = new NetworkVariable<int>(0);

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

        //[jh] NetworkVariable의 OnValueChanged 이벤트에 구독
        curHealth.OnValueChanged += OnCurHealthChanged;
        curGuardGauge.OnValueChanged += OnCurGuardGaugeChanged;

        if (IsOwner) 
        {   
            int WeaponChoice = ProjectJS.PStats.PlayerWeaponSelection.SelectedWeaponIndex;
            if (WeaponChoice < 0) WeaponChoice = 0;

            SetWeaponRpc(WeaponChoice);

            UpdateWeaponVisual(-1, WeaponChoice);
            Debug.Log($"[Player] Owner Initialized with weapon index: {WeaponChoice}");
        }
        else if (currentWeaponIndex.Value != -1)
        {   
            UpdateWeaponVisual(-1, currentWeaponIndex.Value);
        }

        //[jh] 게임 씬 로드될 때 플레이어 스폰 시 UI 연결
        ProjectJS.UI.GameScene.GameSceneUI.Instance?.RegisterPlayer(this);
    }

    //[jh] 테스트용 플레이어 UI 연결
    [ContextMenu("TestUIConnect")]
    public void TestUIConnect()
    {
        ProjectJS.UI.GameScene.GameSceneUI.Instance?.RegisterPlayer(this);
    }

    public void OnAttackHit()
    {
        if (currentWeapon == null) return;

        Vector2 hitCenter;

        if (attackPoint != null)
        {
            Vector3 localPos = attackPoint.localPosition;
            float directionSign = FacingDirection.x >= 0 ? 1f : -1f;

            Vector3 mirroredLocalPos = new Vector3(Mathf.Abs(localPos.x) * directionSign, localPos.y, localPos.z);
            hitCenter = transform.TransformPoint(mirroredLocalPos);
        }

        else
        {
            float offsetDistance = currentWeapon.AttackRange;
            hitCenter = (Vector2)transform.position + (FacingDirection * offsetDistance);
        }

        Debug.DrawLine((Vector2)transform.position, hitCenter, Color.red, 0.5f);

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(hitCenter, attackSize, 0f, enemyLayer);

        if (hitEnemies.Length > 0)
        {
            // Trigger Effects on Hit
            if (currentWeapon.AttackVfxPrefab != null)
            {
                Instantiate(currentWeapon.AttackVfxPrefab, hitCenter, Quaternion.identity);
            }

            // Hit Stop & Screen Shake (only for owner)
            if (IsOwner)
            {
                StartCoroutine(TriggerHitStop(0.07f));
                if (CameraShake.Instance != null)
                {
                    CameraShake.Instance.Shake(0.1f, 0.2f);
                }
            }
        }

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<ProjectJS.Controller.BossController>(out var boss))
            {
                boss.RequestTakeDamageServerRpc(currentWeapon.Damage);
            }
        }
    }
    
 
    private System.Collections.IEnumerator TriggerHitStop(float duration)
    {
        if (isHitStopping) yield break;
        isHitStopping = true;
        
        // Pause Animator
        float prevAnimSpeed = anim.speed;
        anim.speed = 0.05f;

        yield return new WaitForSecondsRealtime(duration);

        anim.speed = prevAnimSpeed;
        
        isHitStopping = false;
    }

    public void SetGuarding(bool state)
    {
        if (state && !IsGuarding) guardStartTime = Time.time;
        IsGuarding = state;

        if (anim != null)
        {
            anim.SetBool("IsGuarding", state);
        }
    }

    public void TryAttack()
    {
        if (currentWeapon == null)
        {
            Debug.LogWarning("[Player] CurrentWeapon is null! Attack failed.");
            return;
        }

        if (Time.time >= nextAttackTime && !IsGuarding)
        {
            Attack();
            nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, currentWeapon.AttackSpeed);
        }
    }

    private void Attack()
    {
        Debug.Log("[Player] Attack Triggered!");
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // Play Attack SFX
        if (currentWeapon != null && currentWeapon.AttackSfxClip != null)
        {
            AudioSource.PlayClipAtPoint(currentWeapon.AttackSfxClip, transform.position);
        }
    }

    [Rpc(SendTo.Server)]
    private void SetWeaponRpc(int Index)
    {
        currentWeaponIndex.Value = Index;
        if (curHealth.Value <= 0) curHealth.Value = BaseStats.MaxHealth;
    }

    private void UpdateWeaponVisual(int OldIndex, int NewIndex)
    {
        if (AvailableWeapons == null || AvailableWeapons.Count == 0)
        {
            Debug.LogError("[Player] AvailableWeapons list is empty!");
            return;
        }

        if (NewIndex < 0 || NewIndex >= AvailableWeapons.Count)
        {
            Debug.LogWarning($"[Player] Invalid Weapon Index: {NewIndex}");
            return;
        }

        currentWeapon = AvailableWeapons[NewIndex];
        
        if (anim != null && currentWeapon.WeaponAnimatorController != null)
        {
            anim.runtimeAnimatorController = currentWeapon.WeaponAnimatorController;
        }
        
        // TODO: Weapon Sprite change logic
        // Example: weaponSpriteRenderer.sprite = currentWeapon.WeaponSprite;
    }
    
    public void TakeDamage(float EnemyDamage, Vector2 attackerPos)
    {
        if (!IsOwner) return;

        TakeDamageServerRpc(EnemyDamage, attackerPos);
    }

    [Rpc(SendTo.Server)]
    private void TakeDamageServerRpc(float EnemyDamage, Vector2 attackerPos)
    {
        Vector2 dirToAttacker = (attackerPos - (Vector2)transform.position).normalized;
        float dot = Vector2.Dot(FacingDirection, dirToAttacker);

        if (IsGuarding && curGuardGauge.Value > 0 && dot > 0)
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
        }

        if (curHealth.Value <= 0)
        {
            DieClientRpc();
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
        // TODO : SFX, VFX.
    }

    private void Die()
    {
        // TODO : gameover screen, sprite.play(died)
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Set Gizmo color to semi-transparent red
        Gizmos.color = new Color(1, 0, 0, 0.5f); 

        Vector2 hitCenter;
        
        // Default attack range to 1f if no weapon is selected in the editor
        float currentAttackRange = (currentWeapon != null) ? currentWeapon.AttackRange : 1f;

        if (attackPoint != null)
        {
            // Calculate direction sign based on facing direction (default to right in editor if not playing)
            float directionSign = Application.isPlaying ? (FacingDirection.x >= 0 ? 1f : -1f) : 1f;
            Vector3 localPos = attackPoint.localPosition;
            
            // Mirror the local position based on facing direction
            Vector3 mirroredLocalPos = new Vector3(Mathf.Abs(localPos.x) * directionSign, localPos.y, localPos.z);
            hitCenter = transform.TransformPoint(mirroredLocalPos);
        }
        else
        {
            // Calculate hit center based on attack range if no attack point is assigned
            Vector2 faceDir = Application.isPlaying ? FacingDirection : Vector2.right; 
            hitCenter = (Vector2)transform.position + (faceDir * currentAttackRange);
        }

        // Draw the wireframe cube representing the attack area
        Gizmos.DrawWireCube(hitCenter, attackSize);
    }
#endif

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