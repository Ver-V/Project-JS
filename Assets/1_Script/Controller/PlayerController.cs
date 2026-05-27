using ProjectJS.Manager;
using Unity.Netcode;
using UnityEngine;
using ProjectJS.Skills;

namespace ProjectJS.Controller
{
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : NetworkBehaviour
    {
        private Player player;
        private Rigidbody2D rb;
        private Vector2 movement;
        private PlayerSkillManager skillManager;
        private Animator anim;

     

        public override void OnNetworkSpawn()
        {
            player = GetComponent<Player>();
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponentInChildren<Animator>();
            skillManager = GetComponent<PlayerSkillManager>();

            if (!IsOwner)
            {
                rb.simulated = false;
                enabled = false;
                return;
            }
        }

        void Update()
        {
            if (!IsOwner) return;

            if (Input.GetKeyDown(KeyCode.R))
            {
                player.RequestRetryServerRpc();
                return;
            }

            if (player.IsDead) return;

            HandleInput();
        }

        void FixedUpdate()
        {
            if (!IsOwner || player.IsDead) return;
            if (player.IsHitStopping) return;
            Move();
        }

        private void HandleInput()
        {
            var input = Managers.PlayerInput.Player;

            movement = input.Move.ReadValue<Vector2>().normalized;

            bool isGuarding = input.Guard.IsPressed();

            if (isGuarding && player.CurGuardGauge > 0)
            {
                player.SetGuarding(true);
            }
            else
            {
                player.SetGuarding(false);
            }

            if (input.Attack.WasPressedThisFrame())
            {
                Debug.Log("[PlayerController] Attack Input Detected");
                player.TryAttack();
            }

            if (input.Skill.WasPressedThisFrame())
            {
                Debug.Log("[PlayerController] Skill Input Detected");
                if (skillManager != null)
                {
                    skillManager.TrySkill();
                }
            }
        }

        private void Move()
        {
            if (movement != Vector2.zero)
            {

                if (movement.x != 0 && !player.IsGuarding)
                {
                    player.FacingDirection = movement.x > 0 ? Vector2.right : Vector2.left;
                    transform.localScale = new Vector3(movement.x > 0 ? 1f : -1f, 1f, 1f);
                }
            }

            anim.SetFloat("Speed", movement.sqrMagnitude);
            
            if (player.Stats == null) return;
            Vector2 nextPosition = rb.position + movement * player.Stats.MoveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(nextPosition);
        }
    }
}