using UnityEngine;

namespace ProjectJS.Skills
{
    public class AxeSkill : SkillBase
    {
        protected override void Execute()
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, FinalRange, ProjectJS.Utils.Constants.LAYER_BOSS);

            foreach (var enemy in hitEnemies)
            {
                ApplyDamageAndEffect(enemy);
            }

            Destroy(gameObject, 0.1f);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, FinalRange);
        }
    }
}