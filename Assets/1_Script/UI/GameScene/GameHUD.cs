using ProjectJS.Controller;
using UnityEngine;

namespace ProjectJS.UI.GameScene
{
    public class GameHUD : MonoBehaviour
    {
        [SerializeField] private PlayerStatusUI playerStatusUI;
        [SerializeField] private BossHPUI bossHPUI;
        [SerializeField] private PartyListUI partyListUI;
        

        public void BindLocalPlayer(Player player)
        {
            playerStatusUI.Bind(player);
        }

        public void UnbindLocalPlayer(Player player)
        {
            playerStatusUI.UnBind(player);
        }

        public void RegisterPartyPlayer(Player player)
        {
            partyListUI.RegisterPlayer(player);
        }

        public void UnregisterPartyPlayer(Player player)
        {
            partyListUI.UnregisterPlayer(player);
        }

        public void BindBoss(BossController boss)
        {
            bossHPUI.Bind(boss);
        }

        public void UnbindBoss(BossController boss)
        {
            bossHPUI.Bind(null);
        }

        
    }
}