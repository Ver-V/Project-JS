using UnityEngine;

namespace ProjectJS.PStats
{
    // [중요] 이거 없으면 에디터에서 생성 못 함
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "ProjectJS/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [SerializeField] private string WeaponName; // weapon's name
        [SerializeField] private float Damage = 10.0f; // player's attack damage
        [SerializeField] private float AttackRange = 1f; // player's attack range
        [SerializeField] private float AttackSpeed = 1f; // player's attack speed per second
        [SerializeField] private GameObject WeaponPrefab; // kind of weapon

        public string GetWeaponName => WeaponName;
        public float GetDamage => Damage;
        public float GetAttackRange => AttackRange;
        public float GetAttackSpeed => AttackSpeed;
        public GameObject GetWeaponPrefab => WeaponPrefab;
    }
}