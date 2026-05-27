using NUnit.Framework.Constraints;
using ProjectJS.Animation;
using ProjectJS.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.Controller
{
	public class Angel_RushPattern : BossPatternBase
	{

		private Collider2D[] colliders = new Collider2D[Constants.MAX_PLAYERS + 1];
		private int playerCount = 0;
		private ContactFilter2D filter;

		[Header("Swing Pattern")]
		[SerializeField] private Vector2 swingSize;
		[SerializeField] private float rushDuration = .3f;
		[SerializeField] private float damageMult = 1f;
		
		private Vector3 cachedSocketPosition = Vector3.zero;
		private Vector3 cachedBossPosition = Vector3.zero;
        private float cachedRotation = 0f;

        private void Awake()
		{
			filter = new ContactFilter2D();
			filter.SetLayerMask(Constants.LAYER_PLAYER);
			filter.useTriggers = true;
		}

		public override IEnumerator DoPattern(BossAttack boss, float attack)
		{
			Transform target = GetComponent<BossAttack>().GetTarget();
			Vector2 dirToTarget = (target.position - transform.position).normalized;
			cachedRotation = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
            transform.localScale = new Vector3(dirToTarget.x > 0 ? 1 : -1, 1, 1);

			boss.GetComponent<BossController>().RequestAnimParam("isAttack6");
			yield return null;
			yield return new WaitUntil(() => !boss.IsAttackAnimPlaying);
		}
		
		public override bool Predict() => true;

        private IEnumerator RushCoroutine()
        {
            Vector2 dir = new Vector2(
                Mathf.Cos(cachedRotation * Mathf.Deg2Rad),
                Mathf.Sin(cachedRotation * Mathf.Deg2Rad)
            ).normalized;

            float rushSpeed = swingSize.y / rushDuration;

            GetComponent<Rigidbody2D>().linearVelocity = dir * rushSpeed;

            float elapsed = 0f;

            while (elapsed < rushDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }

        private void OnStartRush()
        {
            if (!NetworkManager.Singleton.IsHost) return;

			Transform target = GetComponent<BossAttack>().GetTarget();
			Vector2 dirToTarget = (target.position - transform.position).normalized;
			cachedRotation = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
			transform.localScale = new Vector3(dirToTarget.x > 0 ? 1 : -1, 1, 1);

			float offset = Mathf.Max(swingSize.x, swingSize.y) * 0.5f;

            cachedSocketPosition = (Vector2)transform.position + dirToTarget * offset;
            cachedBossPosition = (Vector2)transform.position;

            StartCoroutine(RushCoroutine());
        }

        private void OnRushSickle()
        {
            if (!NetworkManager.Singleton.IsHost) return;

			playerCount = Physics2D.OverlapBox(
                cachedSocketPosition,
                swingSize,
                cachedRotation,
                filter,
                colliders
            );

            for (int i = 0; i < playerCount; i++)
            {
                if (!colliders[i].TryGetComponent(out NetworkObject networkObject))
                    continue;

                if (!networkObject.IsOwner)
                    continue;

                if (colliders[i].TryGetComponent(out Player player))
                {
                    player.TakeDamage(
                        GetComponent<BossController>().GetAttackPower() * damageMult,
						cachedBossPosition
					);
                }
            }
        }

	}
}