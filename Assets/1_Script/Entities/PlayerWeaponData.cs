using UnityEngine;
using ProjectJS.Skills;

namespace ProjectJS.PStats
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "ProjectJS/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [SerializeField] private string weaponName; // weapon's name
        [SerializeField] private float damage = 10.0f; // player's attack damage
        [SerializeField] private float attackRange = 1f; // player's attack range
        [SerializeField] private float attackSpeed = 1f; // player's attack speed per second
        [SerializeField] private GameObject weaponPrefab; // kind of weapon
        [SerializeField] private SkillData weaponSkill; // weapon's specific skill

        public string WeaponName { get => weaponName; set => weaponName = value; }
        public float Damage { get => damage; set => damage = value; }
        public float AttackRange { get => attackRange; set => attackRange = value; }
        public float AttackSpeed { get => attackSpeed; set => attackSpeed = value; }
        public GameObject WeaponPrefab { get => weaponPrefab; set => weaponPrefab = value; }
        public SkillData WeaponSkill { get => weaponSkill; set => weaponSkill = value; }
    }
}