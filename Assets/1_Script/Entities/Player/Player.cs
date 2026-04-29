using System.Collections.Generic;
using ProjectJS.PStats;
using Unity.Netcode;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

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
    public Vector2 FacingDirection { get; set; } = Vector2.down;

    public override void OnNetworkSpawn()
    {
        anim= GetComponentInChildren<Animator>();

        currentWeaponIndex.OnValueChanged += UpdateWeaponVisual;

        if (IsOwner) 
        {   // Throw Selection class
            int WeaponChoice = PlayerWeaponSelection.SelectedWeaponIndex;
            // report my choice
            SetWeaponRpc(WeaponChoice);
        }
        else if (currentWeaponIndex.Value != -1)
        {   // Synchronize with other players
            UpdateWeaponVisual(-1, currentWeaponIndex.Value);
        }

  

    }


    public void OnAttackHit()
    {
        Vector2 hitCenter = (Vector2)transform.position + (FacingDirection * 1.0f);
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(hitCenter, attackSize, 0f, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            return;
           //  enemy.GetComponent<Enemy>().TakeDamage(currentWeapon.Damage);
        
        }

    }
    
    public void SetGuarding(bool state)
    {
        if (state && !IsGuarding) guardStartTime = Time.time;
        IsGuarding = state;
    }

    public void TryAttack()
    {
        if (Time.time >= nextAttackTime && !IsGuarding && currentWeapon != null)
        {
            Attack();
            nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, currentWeapon.AttackSpeed);
        }
    }

    private void Attack()
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
    }

    private void UpdateWeaponVisual(int OldIndex, int NewIndex)
    {
        if (NewIndex < 0 || NewIndex >= AvailableWeapons.Count) return;
        currentWeapon = AvailableWeapons[NewIndex];
        // TODO: Weapon Modeling change logic (Thank you UI)
    }
    
    public void TakeDamage(float EnemyDamage)
    {
        if (!IsOwner) return;

        TakeDamageServerRpc(EnemyDamage);
    }

    [Rpc(SendTo.Server)]
    private void TakeDamageServerRpc(float EnemyDamage)
    {
        if (IsGuarding && curGuardGauge.Value > 0)
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