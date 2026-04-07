using UnityEngine;

namespace ProjectJS.Entities {
    public class TestMovement : MonoBehaviour, IProjectileMovement
    {
        [SerializeField] private float speed;
        public Vector2 Evaluate(Vector2 startPos, Vector2 startDir, float time)
        {
            return startPos + startDir * speed * time;
        }
    }
}