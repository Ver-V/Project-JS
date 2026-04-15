using UnityEngine;

namespace ProjectJS.Entities {
    public class TestMovement2 : MonoBehaviour, IProjectileMovement
    {
        [SerializeField] private float speed;
		public Vector2 Evaluate(Vector2 startPos, Vector2 startDir, float time)
		{
			float amplitude = 1.5f;
			float frequency = 5f;

			Vector2 dir = startDir.normalized;
			Vector2 forward = dir * speed * time;
			Vector2 perpendicular = new Vector2(-dir.y, dir.x);

			float wave = -Mathf.Sin(time * frequency) * amplitude;
			return startPos + forward + perpendicular * wave;
		}
	}
}