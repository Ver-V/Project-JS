using UnityEngine;

namespace ProjectJS.Skills

{
    public enum ShardSpecies {None, DarkMatter}

    [CreateAssetMenu(fileName = "NewShardData", menuName = "ProjectJS/Skills/ShardData")]
    public class ShardData : ScriptableObject
    {
        [SerializeField] private string shardName;
        [SerializeField] private ShardSpecies species = ShardSpecies.None;

        [Header("Status Effect")]
        [SerializeField] private StatusEffect grantedEffect = StatusEffect.None;

        [Header("Modifiers")]
        [SerializeField] private float damageMultiplier = 1.0f; // Skill Damage Multiplier
        [SerializeField] private float rangeMultiplier = 1.0f; // Skill Range Multiplier
        [SerializeField] private float cooldownMultiplier = 1.0f; // Cooldown reduce or increase
        [SerializeField] private GameObject speciesEffectprefab; // species effect

        public string ShardName { get { return shardName; } set { shardName = value; } }
        public ShardSpecies Species { get => species; set => species = value; }
        public StatusEffect GrantedEffect => grantedEffect;
        public float DamageMultiplier { get => damageMultiplier; set => damageMultiplier = value; }
        public float RangeMultiplier { get => rangeMultiplier; set => rangeMultiplier = value; }
        public float CooldownMultiplier { get => cooldownMultiplier; set => cooldownMultiplier = value; }
        public GameObject SpeciesEffectprefab { get => speciesEffectprefab; set => speciesEffectprefab = value; }
    }
}