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
        [SerializeField] private Sprite weaponSprite; // weapon's sprite for visual
        [SerializeField] private SkillData weaponSkill; // weapon's specific skill
        [SerializeField] private RuntimeAnimatorController weaponAnimatorController;
        
        [Header("Visual & Audio")]
        [SerializeField] private GameObject attackVfxPrefab; // Attack Hit Effects
        [SerializeField] private AudioClip attackSfxClip; // Attack Sounds

        public string WeaponName => weaponName;
        public float Damage => damage;
        public float AttackRange => attackRange;
        public float AttackSpeed => attackSpeed;
        public GameObject WeaponPrefab => weaponPrefab;
        public Sprite WeaponSprite => weaponSprite;
        public SkillData WeaponSkill => weaponSkill;
        public GameObject AttackVfxPrefab => attackVfxPrefab;
        public AudioClip AttackSfxClip => attackSfxClip;
        public RuntimeAnimatorController WeaponAnimatorController => weaponAnimatorController;
    }
}