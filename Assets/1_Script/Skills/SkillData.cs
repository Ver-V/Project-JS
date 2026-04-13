using UnityEngine;

namespace ProjectJS.Skills

{
    [CreateAssetMenu(fileName = "NewSkillData", menuName = "ProjectJS/Skills/SkillData")]
    public class SkillData : ScriptableObject
    {
        [SerializeField] private string skillName;
        [SerializeField] private float baseDamage = 10.0f;
        [SerializeField] private float baseCooldown = 15.0f;
        [SerializeField] private float baseRange = 2.0f;

        [Header("Visual & Audio")]
        [SerializeField] private GameObject vfxPrefab; // Skill Effects
        [SerializeField] private AudioClip sfxClip; // Skill Sounds
        [SerializeField] private SkillBase skillLogicPrefab; // Skill logic prefab (sword, axe, spear... etc.)
        
        public string SkillName => skillName;
        public float BaseDamage => baseDamage;
        public float BaseCooldown => baseCooldown;
        public float BaseRange => baseRange;
        public GameObject VfxPrefab => vfxPrefab;
        public AudioClip SfxClip => sfxClip;
        public SkillBase SkillLogicPrefab => skillLogicPrefab;
    }

}