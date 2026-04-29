using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.Controller
{
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : NetworkBehaviour
    {
        private Player player;
        private Rigidbody2D rb;
        private Vector2 movement;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }

            player = GetComponent<Player>();
            rb = GetComponent<Rigidbody2D>();
        }

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

            if (Input.GetMouseButton(1) && player.CurGuardGauge > 0)
            {
                player.SetGuarding(true);
            }
            else
            {
                player.SetGuarding(false);
            }

            if (Input.GetMouseButton(0))
            {
                player.TryAttack();
            }
        }

        private void Move()
        {
            if (player.Stats == null) return;
            
            Vector2 nextPosition = rb.position + movement * player.Stats.MoveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(nextPosition);
        }
    }
}