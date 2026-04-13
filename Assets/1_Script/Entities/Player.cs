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

    private NetworkVariable<float> curHealth = new NetworkVariable<float>(); // player's current health
    private NetworkVariable<float> curGuardGauge = new NetworkVariable<float>(); // player's current guard gauge
    private NetworkVariable<int> currentWeaponIndex = new NetworkVariable<int>(0);

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isGuarding = false;
    private float guardStartTime = 0.0f;
    private float nextAttackTime = 0.0f;
    private WeaponData currentWeapon;
    public WeaponData CurrentWeapon => currentWeapon;

    public override void OnNetworkSpawn()
    {

        rb = GetComponent<Rigidbody2D>();
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

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        HandleInput();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        Move();
    }

    private void HandleInput()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        if (Input.GetMouseButton(1)  && curGuardGauge.Value > 0)
        {
            if (!isGuarding) guardStartTime = Time.time;
            isGuarding = true;
        }
        else
        {
            isGuarding = false;
        }
        if (Input.GetMouseButton(0))
        {
            if (Time.time >= nextAttackTime && !isGuarding && currentWeapon != null)
            {
                Attack();
                nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, currentWeapon.AttackSpeed);
            }
        }
    }

    private void Move()
    {
        Vector2 NextPosition = rb.position + movement * BaseStats.MoveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(NextPosition);
    }

    private void Attack()
    {
        return;
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
        if (isGuarding && curGuardGauge.Value > 0)
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
