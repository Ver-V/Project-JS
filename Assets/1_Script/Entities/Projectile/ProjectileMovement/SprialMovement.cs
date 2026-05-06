using UnityEngine;

namespace ProjectJS.Entities 
{
    public class SprialMovement : MonoBehaviour, IProjectileMovement
    {
		[SerializeField] private float speed;       
		[SerializeField] private float spiralFactor;

		public Vector2 Evaluate(Vector2 startPos, Vector2 startDir, float time)
		{
			float distance = speed * time;

			float safeR = Mathf.Max(distance, 0.1f);

			float angle = spiralFactor * (safeR / (1f + safeR));

			float baseAngle = Mathf.Atan2(startDir.y, startDir.x);
			float finalAngle = baseAngle + angle;

			Vector2 dir = new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle));

			return startPos + dir * distance;
		}
	}
}