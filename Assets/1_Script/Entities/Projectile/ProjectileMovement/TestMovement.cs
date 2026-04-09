using UnityEngine;

namespace ProjectJS.Entities {
    public class TestMovement : MonoBehaviour, IProjectileMovement
    {
        [SerializeField] private float speed;
        public Vector2 Evaluate(Vector2 startPos, Vector2 startDir, float time)
		{
			Vector2 right = new Vector2(-startDir.y, startDir.x);

			float amplitude = 1.5f;
			float frequency = 5f;

			float sin = Mathf.Sin(time * frequency);
			float cos = Mathf.Cos(time * frequency);

			return startPos
			   + startDir * speed * time
			   + right * sin * amplitude
			   + startDir * cos * 0.2f;
		}
    }
}