using UnityEngine;

namespace ProjectJS.Entities {
    public class TestMovement1 : MonoBehaviour, IProjectileMovement
    {
        [SerializeField] private float speed;
		public Vector2 Evaluate(Vector2 startPos, Vector2 startDir, float time)
		{
			Vector2 dir = startDir.normalized;
			Vector2 forward = dir * speed * time;

			return startPos + forward;
		}
	}
}