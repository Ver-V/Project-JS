using UnityEngine;

namespace ProjectJS.Skills
{
    public class SpearSkill : SkillBase
    {
        protected override void Execute()
        {
            float spearRange = FinalRange * 1.75f;

            RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, new Vector2(0.5f, 0.5f), 0f, SkillDirection, spearRange, ProjectJS.Utils.Constants.LAYER_BOSS);

            foreach (var hit in hits)
            {
                ApplyDamageAndEffect(hit.collider);
            }

            Destroy(gameObject, 0.1f);
        }
    }
}