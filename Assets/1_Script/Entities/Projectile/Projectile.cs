using ProjectJS.Manager;
using ProjectJS.Utils;
using UnityEngine;

namespace ProjectJS.Entities
{
	public class Projectile : MonoBehaviour
    {
		[SerializeField] private int id;
		[SerializeField] private float damage = 10f;
        private IProjectileMovement movement;
        private Vector2 startPos;
        private Vector2 startDir;
        private float startTime;

		public int Id => id;

		private void Awake()
		{
			movement = GetComponent<IProjectileMovement>();
		}

		public void Init(int id, Vector2 pos, Vector2 dir, float damage = 10f)
		{
			Managers.Pool.RegisterProjectileSync(id, this.gameObject);
			
			this.id = id;
			this.damage = damage;
			transform.position = pos.ToVector3();
			startPos = transform.position;
			startTime = Time.time;
			startDir = dir;

			transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
		}

		private void Update()
		{
			Vector3 nextPos = movement.Evaluate(startPos, startDir, Time.time - startTime).ToVector3();
			Vector2 dir = (nextPos - transform.position).ToVector2();
			transform.position = nextPos;

			if (dir.sqrMagnitude > Mathf.Epsilon)
			{
				Quaternion targetRot = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
				transform.rotation = targetRot;
			}
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.TryGetComponent<Player>(out var player))
			{
				player.TakeDamage(damage, (Vector2)transform.position);

				if (TryGetComponent<Poolable>(out var poolable))
				{
					Managers.Pool.Return(poolable.PoolingType, gameObject);
				}
			}
		}
	}
}