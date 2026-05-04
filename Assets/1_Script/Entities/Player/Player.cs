using System.Collections.Generic;
using ProjectJS.PStats;
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

    public WeaponData CurrentWeapon => currentWeapon;
    public PlayerStats Stats => BaseStats;
    public float CurGuardGauge => curGuardGauge.Value;
    public Vector2 FacingDirection { get; set; } = Vector2.right;
public override void OnNetworkSpawn()
{
    anim = GetComponentInChildren<Animator>();
    currentWeaponIndex.OnValueChanged += UpdateWeaponVisual;

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

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<ProjectJS.Controller.BossController>(out var boss))
            {
                boss.RequestTakeDamageServerRpc(currentWeapon.Damage);
            }
        }
    }
    
    public void SetGuarding(bool state)
    {
        if (state && !IsGuarding) guardStartTime = Time.time;
        IsGuarding = state;
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
        Debug.Log($"[Player] Weapon Updated: {currentWeapon.WeaponName} (Index: {NewIndex})");
        
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

}