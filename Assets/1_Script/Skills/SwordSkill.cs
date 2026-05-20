using UnityEngine;

namespace ProjectJS.Skills
{
    public class SwordSkill : SkillBase
    {
        protected override void Execute()
        {
            Vector2 skillDir = SkillDirection; 
            Vector2 boxCenter = (Vector2)transform.position + (skillDir * (FinalRange * 1.0f));
            Vector2 boxSize = new Vector2(FinalRange, 1.5f);
            float angle = Mathf.Atan2(skillDir.y, skillDir.x) * Mathf.Rad2Deg;

            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(boxCenter, boxSize, angle, ProjectJS.Utils.Constants.LAYER_BOSS);

            foreach (var enemy in hitEnemies)
            {
                ApplyDamageAndEffect(enemy);
            }
            Destroy(gameObject, 0.1f);
        }

        private void OnDrawGizmos()
        {
            // 디버그용 범위 표시
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + (Vector3)(SkillDirection * (FinalRange * 0.5f)), new Vector3(FinalRange, 1.5f, 0));
        }
    }
}