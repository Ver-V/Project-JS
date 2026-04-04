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

    private NetworkVariable<float> CurHealth = new NetworkVariable<float>(); // player's current health
    private NetworkVariable<float> CurGuardGauge = new NetworkVariable<float>(); // player's current guard gauge
    private NetworkVariable<int> CurrentWeaponIndex = new NetworkVariable<int>(0);

    private Rigidbody2D Rb;
    private Vector2 Movement;
    private bool IsGuarding = false;
    private float GuardStartTime = 0.0f;
    private float NextAttackTime = 0f;
    private WeaponData CurrentWeapon;

    public override void OnNetworkSpawn()
    {

        Rb = GetComponent<Rigidbody2D>();
        CurrentWeaponIndex.OnValueChanged += UpdateWeaponVisual;

        if (IsOwner) 
        {   // Throw Selection class
            int WeaponChoice = PlayerWeaponSelection.SelectedWeaponIndex;
            // report my choice
            RequestSetWeaponServerRpc(WeaponChoice);
        }
        else if (CurrentWeaponIndex.Value != -1)
        {   // Synchronize with other players
            UpdateWeaponVisual(-1, CurrentWeaponIndex.Value);
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
        Movement.x = Input.GetAxisRaw("Horizontal");
        Movement.y = Input.GetAxisRaw("Vertical");
        Movement = Movement.normalized;

        if (Input.GetMouseButton(1)  && CurGuardGauge.Value > 0)
        {
            if (!IsGuarding) GuardStartTime = Time.time;
            IsGuarding = true;
        }
        else
        {
            IsGuarding = false;
        }
        if (Input.GetMouseButton(0))
        {
            if (Time.time >= NextAttackTime && !IsGuarding)
            {
                Attack();
                NextAttackTime = Time.time + 1f / CurrentWeapon.GetAttackSpeed;
            }
        }
    }

    private void Move()
    {
        Vector2 NextPosition = Rb.position + Movement * BaseStats.GetMoveSpeed * Time.fixedDeltaTime;
        Rb.MovePosition(NextPosition);
    }

    private void Attack()
    {
        return;
    }

    [ServerRpc]
    private void RequestSetWeaponServerRpc(int Index)
    {
        CurrentWeaponIndex.Value = Index;
        if (CurHealth.Value <= 0) CurHealth.Value = BaseStats.GetMaxHealth;
    }

    private void UpdateWeaponVisual(int OldIndex, int NewIndex)
    {
        if (NewIndex < 0 || NewIndex >= AvailableWeapons.Count) return;
        CurrentWeapon = AvailableWeapons[NewIndex];
        // TODO: Weapon Modeling change logic (Thank you UI)
    }
    public void TakeDamage(float EnemyDamage)
    {
        if (!IsServer) return;

        if (IsGuarding && CurGuardGauge.Value > 0)
        {
            if (Time.time - GuardStartTime <= 0.2f)
            {
                CurGuardGauge.Value -= (EnemyDamage * 0.5f);
                PlayJustGuardEffectClientRpc();
            }
            else
            {
                float blockedDamage = EnemyDamage * 0.5f;
                CurGuardGauge.Value -= blockedDamage;
            }
        }
        else
        {
            CurHealth.Value -= EnemyDamage;
        }

        if (CurHealth.Value <= 0)
        {
            Die();
        }
    }

    private void PlayJustGuardEffectClientRpc()
    {
        // TODO : SFX, VFX.
    }

    private void Die()
    {
        // TODO : gameover screen, sprite.play(died)
    }

}
