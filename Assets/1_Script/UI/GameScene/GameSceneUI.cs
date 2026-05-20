using ProjectJS.Controller;
using UnityEngine;

namespace ProjectJS.UI.GameScene
{
    public class GameSceneUI : MonoBehaviour
    {
        public static GameSceneUI Instance { get; private set; }

        [SerializeField] private GameHUD gameHUD;

        private void Awake()
        {
            Instance = this;
        }

        public void RegisterPlayer(Player player)
        {
            if (player == null) return;

            if (player.IsOwner)
            {
                gameHUD.BindLocalPlayer(player);
                return;
            }

            gameHUD.RegisterPartyPlayer(player);
        }

        public void UnregisterPlayer(Player player)
        {
            if (player == null) return;

            if (player.IsOwner)
            {
                gameHUD.UnbindLocalPlayer(player);
                return;
            }

            gameHUD.UnregisterPartyPlayer(player);
        }

        public void RegisterBoss(BossController boss)
        {
            if (boss == null) return;
        
            gameHUD.BindBoss(boss);
        }
        
        public void UnregisterBoss(BossController boss)
        {
            if (boss == null) return;
        
            gameHUD.UnbindBoss(null);
        }
    }
}
