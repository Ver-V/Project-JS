using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{

    public float maxHealth = 100f; // Player's max health
    public float curHealth; // player's current health
    public float maxGuardGauge = 100f; // player's max guard gauge
    public float curGuardGauge; // player's current guard gauge
    public float moveSpeed = 5f; // player's movement speed
    public float damage = 10f; // player's attack damage
    public float attackRange = 1f; // player's attack range
    public float attackSpeed = 1f; // player's attack speed per second
    
    private float guardStartTime = 0f; 
    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isGuarding = false;
    private float nextAttackTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        curHealth = maxHealth;
        curGuardGauge = maxGuardGauge;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        Move();
    }

    private void HandleInput()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        if (Input.GetMouseButton(1)  && curGuardGauge > 0)
        {
            guardStartTime = Time.time;
            isGuarding = true;
        }
        else
        {
            isGuarding = false;
        }
        if (Input.GetMouseButton(0))
        {
            if (Time.time >= nextAttackTime && !isGuarding)
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackSpeed;
            }
        }
    }

    private void Move()
    {
        Vector2 nextPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);
    }

    private void Attack()
    {
        return;
    }

    public void TakeDamage(float EnemyDamage)
    {
        if (isGuarding && curGuardGauge > 0)
        {
            if (Time.time - guardStartTime <= 0.2f)
            {
                curGuardGauge -= (EnemyDamage * 0.5f);
                PlayJustGuardEffect();
            }
            else
            {
                float blockedDamage = EnemyDamage * 0.5f;
                curGuardGauge -= blockedDamage;
            }
        }
        else
        {
            curHealth -= EnemyDamage;
        }

        if (curHealth <= 0)
        {
            Die();
        }
    }

    private void PlayJustGuardEffect()
    {
        // TODO : SFX, VFX.
    }

    private void Die()
    {
        return;
        // TODO : gameover screen, sprite.play(died)
    }

}
