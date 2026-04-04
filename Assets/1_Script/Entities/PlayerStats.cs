using UnityEngine;


namespace ProjectJS.PStats
{
    [System.Serializable] 
    public class PlayerStats
    {
        [SerializeField] private float MaxHealth = 100.0f; // Player's max health
        [SerializeField] private float MaxGuardGauge = 100.0f; // player's max guard gauge
        [SerializeField] private float MoveSpeed = 5.0f; // player's movement speed

        public float GetMaxHealth => MaxHealth;
        public float GetMaxGuardGauge => MaxGuardGauge;
        public float GetMoveSpeed => MoveSpeed;

    }
}