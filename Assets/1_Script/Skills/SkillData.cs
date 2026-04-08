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
        
        public string SkillName { get => skillName; set => skillName = value; }
        public float BaseDamage { get => baseDamage; set => baseDamage = value; }
        public float BaseCooldown { get => baseCooldown; set => baseCooldown = value; }
        public float BaseRange { get => baseRange; set => baseRange = value; }
        public GameObject VfxPrefab { get => vfxPrefab; set => vfxPrefab = value; }
        public AudioClip SfxClip { get => sfxClip; set => sfxClip = value; }
        public SkillBase SkillLogicPrefab { get => skillLogicPrefab; set => skillLogicPrefab = value; }
    }

}