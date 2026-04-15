using ProjectJS.Utils;
using UnityEngine;

namespace ProjectJS.Entities
{
	public class Projectile : MonoBehaviour
    {
		private int id;
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
				Quaternion targetRot = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
				transform.rotation = targetRot;
			}
		}
	}
}