using UnityEngine;

namespace ProjectJS.Animation
{
    public class PlayerAnimNotifier : MonoBehaviour
    {
        private Player player;

        private void Awake()
        {
            player = GetComponentInParent<Player>();
        }

        public void OnAttackHit()
        {
            if (player != null)
            {
                player.OnAttackHit();
            }
            else
            {
                Debug.LogWarning("[PlayerAnimNotifier] Player 컴포넌트를 부모에서 찾을 수 없습니다!");
            }
        }
    }
}