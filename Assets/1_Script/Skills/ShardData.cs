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

        public string ShardName => shardName;
        public ShardSpecies Species => species;
        public StatusEffect GrantedEffect => grantedEffect;
        public float DamageMultiplier => damageMultiplier;
        public float RangeMultiplier => rangeMultiplier;
        public float CooldownMultiplier => cooldownMultiplier;
        public GameObject SpeciesEffectprefab => speciesEffectprefab;
    }
}