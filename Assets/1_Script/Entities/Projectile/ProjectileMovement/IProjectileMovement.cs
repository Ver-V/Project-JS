using UnityEngine;

namespace ProjectJS.Entities
{
	public interface IProjectileMovement
	{
		public Vector2 Evaluate(Vector2 startPos, Vector2 startDir, float time);
	}

}
