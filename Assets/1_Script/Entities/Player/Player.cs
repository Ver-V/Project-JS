using System.Collections;
using System.Collections.Generic;
using ProjectJS.PStats;
using ProjectJS.Utils;
using ProjectJS.Controller;
using ProjectJS.Manager;
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

    [Header("Player Effects")]
    [SerializeField] private GameObject damagedVfxPrefab;
    [SerializeField] private GameObject guardVfxPrefab;
    [SerializeField] private GameObject justGuardVfxPrefab;
    [SerializeField] private GameObject deathVfxPrefab;
    [SerializeField] private AudioClip damagedSfxClip;
    [SerializeField] private AudioClip guardSfxClip;
    [SerializeField] private AudioClip justGuardSfxClip;
    [SerializeField] private AudioClip deathSfxClip;

    [Header("Shader Effects")]
    [SerializeField] private Shader playerEffectShader;
    [SerializeField] private float damagedEffectDuration = 0.25f;
    [SerializeField] private float guardEffectDuration = 0.5f;
    [SerializeField] private float deathEffectDuration = 1.5f;
    private float lastHitTime = -1.0f;
    private Coroutine invincibilityCoroutine;
    private Coroutine shaderEffectCoroutine;
    private SpriteRenderer[] effectRenderers;
    private Material playerEffectMaterial;

    private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
    private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
    private static readonly int DirectionalId = Shader.PropertyToID("_Directional");
    private static readonly int HitDirectionId = Shader.PropertyToID("_HitDirection");
    private static readonly int NoiseAmountId = Shader.PropertyToID("_NoiseAmount");

    private NetworkVariable<float> curHealth = new NetworkVariable<float>();
    private NetworkVariable<float> curGuardGauge = new NetworkVariable<float>();
    private NetworkVariable<int> currentWeaponIndex = new NetworkVariable<int>(0);
    private NetworkVariable<bool> isFacingLeft = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

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

    public string GetPlayerNickname()
    {
        string nickname = GameManagerEx.Instance?.GetPlayerNickname(OwnerClientId);
        if (!string.IsNullOrEmpty(nickname))
        {
            return nickname;
        }

        if (IsOwner)
        {
            return SteamManager.Instance?.GetSteamNickname() ?? string.Empty;
        }

        return string.Empty;
    }

    //[jh] 게이지 UI에서 연결하기 위한 이벤트
    public event System.Action<float, float, float> OnHealthChangedEvent;
    public event System.Action<float, float, float> OnGuardGaugeChangedEvent;

    public override void OnNetworkSpawn()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        InitializeShaderEffects();
        currentWeaponIndex.OnValueChanged += UpdateWeaponVisual;
        isFacingLeft.OnValueChanged += OnFacingDirectionChanged;
        ApplyFacingVisual(isFacingLeft.Value);

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
        if (!IsOwner || currentWeapon == null || IsDead) return;

        Vector2 hitCenter = (Vector2)transform.position + (FacingDirection * attackOffset);
        if (attackPoint != null) hitCenter.y = attackPoint.position.y;

        Vector2 effectiveSize = new Vector2(attackSize.x * currentWeapon.AttackRange, attackSize.y);

        Debug.DrawLine((Vector2)transform.position, hitCenter, Color.red, 0.5f);

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(hitCenter, effectiveSize, 0f, Constants.LAYER_BOSS);
        if (hitEnemies.Length > 0)
        {
            PlayAttackHitEffectRpc(hitCenter, FacingDirection.x);

            StartCoroutine(TriggerHitStop(0.07f));
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.1f, 0.2f);
            }
        }

        HashSet<ProjectJS.Controller.BossController> hitBosses = new HashSet<ProjectJS.Controller.BossController>();

        foreach (Collider2D enemy in hitEnemies)
        {
            var boss = enemy.GetComponentInParent<ProjectJS.Controller.BossController>();
            if (boss != null && hitBosses.Add(boss))
            {
                boss.RequestTakeDamageServerRpc(currentWeapon.Damage);
            }
        }
    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (Mathf.Approximately(direction.x, 0f)) return;

        bool facingLeft = direction.x < 0f;
        FacingDirection = facingLeft ? Vector2.left : Vector2.right;
        ApplyFacingVisual(facingLeft);

        if (IsSpawned && IsOwner && isFacingLeft.Value != facingLeft)
        {
            isFacingLeft.Value = facingLeft;
        }
    }

    private void OnFacingDirectionChanged(bool previousValue, bool newValue)
    {
        FacingDirection = newValue ? Vector2.left : Vector2.right;
        ApplyFacingVisual(newValue);
    }

    private void ApplyFacingVisual(bool facingLeft)
    {
        if (anim == null) return;

        SpriteRenderer spriteRenderer = anim.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = facingLeft;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayAttackHitEffectRpc(Vector2 position, float facingX)
    {
        if (currentWeapon == null) return;

        if (currentWeapon.AttackVfxPrefab != null)
        {
            Quaternion rotation = facingX < 0f ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;
            Managers.Pool.SpawnVfx(currentWeapon.AttackVfxPrefab, position, rotation);
        }

        PlaySfx(currentWeapon.HitSfxClip);
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
        if (state && !IsGuarding)
        {
            guardStartTime = Time.time;
        }

        IsGuarding = state;

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

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AttackRpc()
    {
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        PlaySfx(currentWeapon?.AttackSfxClip);
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
                PlayDamageEffectRpc(PlayerEffectType.JustGuard, GetHitDirection(attackerPos));
            }
            else
            {
                float blockedDamage = EnemyDamage * 0.5f;
                curGuardGauge.Value -= blockedDamage;
                PlayDamageEffectRpc(PlayerEffectType.Guard, GetHitDirection(attackerPos));
            }
        }
        else
        {
            curHealth.Value -= EnemyDamage;
            lastHitTime = Time.time;
            PlayDamageEffectRpc(PlayerEffectType.Damaged, GetHitDirection(attackerPos));
            StartInvincibilityClientRpc();
        }

        if (curHealth.Value <= 0)
        {
            PlayDamageEffectRpc(PlayerEffectType.Death, 0f);
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

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayDamageEffectRpc(PlayerEffectType effectType, float hitDirection)
    {
        switch (effectType)
        {
            case PlayerEffectType.Damaged:
                PlayEffect(damagedVfxPrefab, damagedSfxClip);
                PlayShaderEffect(Color.red, damagedEffectDuration, hitDirection, false, true);
                break;
            case PlayerEffectType.Guard:
                PlayEffect(guardVfxPrefab, guardSfxClip);
                PlayShaderEffect(Color.white, guardEffectDuration, hitDirection, true, false);
                break;
            case PlayerEffectType.JustGuard:
                PlayEffect(justGuardVfxPrefab, justGuardSfxClip);
                PlayShaderEffect(new Color(1f, 0.75f, 0.05f), guardEffectDuration, hitDirection, true, false);
                break;
            case PlayerEffectType.Death:
                PlayEffect(deathVfxPrefab, deathSfxClip);
                PlayShaderEffect(new Color(1f, 0.02f, 0.02f), deathEffectDuration, 0f, false, false);
                break;
        }
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

    private void PlayEffect(GameObject vfxPrefab, AudioClip sfxClip)
    {
        if (vfxPrefab != null)
        {
            Managers.Pool.SpawnVfx(vfxPrefab, transform.position, Quaternion.identity);
        }

        PlaySfx(sfxClip);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null)
        {
            Managers.Pool.PlaySfx(clip, transform.position);
        }
    }

    private void InitializeShaderEffects()
    {
        if (playerEffectShader == null)
        {
            Debug.LogWarning("[Player] Player effect shader is not assigned.");
            return;
        }

        effectRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        playerEffectMaterial = new Material(playerEffectShader)
        {
            name = $"{name}_PlayerImpact_Runtime"
        };

        foreach (SpriteRenderer renderer in effectRenderers)
        {
            renderer.sharedMaterial = playerEffectMaterial;
        }

        ResetShaderEffect();
    }

    private float GetHitDirection(Vector2 attackerPosition)
    {
        float deltaX = attackerPosition.x - transform.position.x;
        if (Mathf.Abs(deltaX) < 0.01f)
        {
            return FacingDirection.x >= 0f ? 1f : -1f;
        }

        return Mathf.Sign(deltaX);
    }

    private void PlayShaderEffect(
        Color color,
        float duration,
        float hitDirection,
        bool directional,
        bool noisy)
    {
        if (playerEffectMaterial == null) return;

        if (shaderEffectCoroutine != null)
        {
            StopCoroutine(shaderEffectCoroutine);
        }

        shaderEffectCoroutine = StartCoroutine(ShaderEffectRoutine(
            color,
            duration,
            ConvertToShaderDirection(hitDirection),
            directional,
            noisy));
    }

    private float ConvertToShaderDirection(float worldDirection)
    {
        if (anim == null || Mathf.Approximately(worldDirection, 0f))
        {
            return worldDirection;
        }

        float visualRight = Mathf.Sign(Vector3.Dot(anim.transform.right, Vector3.right));
        return worldDirection * visualRight;
    }

    private IEnumerator ShaderEffectRoutine(
        Color color,
        float duration,
        float hitDirection,
        bool directional,
        bool noisy)
    {
        playerEffectMaterial.SetColor(FlashColorId, color);
        playerEffectMaterial.SetFloat(DirectionalId, directional ? 1f : 0f);
        playerEffectMaterial.SetFloat(HitDirectionId, hitDirection);
        playerEffectMaterial.SetFloat(NoiseAmountId, noisy ? 0.8f : 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
            float intensity = directional
                ? 1f - normalizedTime
                : Mathf.SmoothStep(1f, 0f, normalizedTime);

            playerEffectMaterial.SetFloat(FlashAmountId, intensity);
            yield return null;
        }

        ResetShaderEffect();
        shaderEffectCoroutine = null;
    }

    private void ResetShaderEffect()
    {
        if (playerEffectMaterial == null) return;

        playerEffectMaterial.SetFloat(FlashAmountId, 0f);
        playerEffectMaterial.SetFloat(DirectionalId, 0f);
        playerEffectMaterial.SetFloat(NoiseAmountId, 0f);
    }

    public override void OnDestroy()
    {
        if (playerEffectMaterial != null)
        {
            Destroy(playerEffectMaterial);
        }

        base.OnDestroy();
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
        ResetShaderEffect();
        
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

    private enum PlayerEffectType : byte
    {
        Damaged,
        Guard,
        JustGuard,
        Death
    }
}
