using UnityEngine;


namespace ProjectJS.PStats
{
    [System.Serializable] 
    public class PlayerStats
    {
        [SerializeField] private float maxHealth = 100.0f; // Player's max health
        [SerializeField] private float maxGuardGauge = 100.0f; // player's max guard gauge
        [SerializeField] private float moveSpeed = 5.0f; // player's movement speed

        public float MaxHealth { get => maxHealth; set => maxHealth = value; }
        public float MaxGuardGauge { get => maxGuardGauge; set => maxGuardGauge = value; }
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

    }
}