using ProjectJS.Manager;
using ProjectJS.Utils;
using UnityEngine;

namespace ProjectJS.Entities
{
	public class Projectile : MonoBehaviour
    {
		[SerializeField] private int id;
        private IProjectileMovement movement;
        private Vector2 startPos;
        private Vector2 startDir;
        private float startTime;

		public int Id => id;

		private void Awake()
		{
			movement = GetComponent<IProjectileMovement>();
		}

		public void Init(int id, Vector2 pos, Vector2 dir)
		{
			Managers.Pool.RegisterProjectileSync(id, this.gameObject);
			
			this.id = id;
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
				float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
				Vector3 scale = transform.localScale;

				// rotation/scale 보정
				if (angle >= -90f && angle <= 90f)
				{
					scale.x = 1f;
					transform.rotation = Quaternion.Euler(0, 0, angle);
				}
				else
				{
					scale.x = -1f;
					float adjustedAngle = angle + (angle > 0 ? -180f : 180f);
					transform.rotation = Quaternion.Euler(0, 0, adjustedAngle);
				}

				transform.localScale = scale;
			}
		}
	}
}