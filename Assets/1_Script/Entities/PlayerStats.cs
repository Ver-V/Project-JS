using UnityEngine;


namespace ProjectJS.PStats
{
    [System.Serializable] 
    public class PlayerStats
    {
        [SerializeField] private float maxHealth = 100.0f; // Player's max health
        [SerializeField] private float maxGuardGauge = 100.0f; // player's max guard gauge
        [SerializeField] private float moveSpeed = 5.0f; // player's movement speed

        public float MaxHealth => maxHealth;
        public float MaxGuardGauge => maxGuardGauge;
        public float MoveSpeed => moveSpeed;

    }
}