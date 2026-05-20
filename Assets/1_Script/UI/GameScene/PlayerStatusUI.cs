using UnityEngine;
using UnityEngine.UI;

namespace ProjectJS.UI.GameScene
{
    public class PlayerStatusUI : MonoBehaviour
    {
        [SerializeField] private HPGaugeUI playerHPGauge;
        [SerializeField] private GuardGaugeUI playerGuardGauge;
        [SerializeField] private Image playerWeaponImage;


        public void Bind(Player player)
        {
            playerHPGauge.Bind(player);
            playerGuardGauge.Bind(player);
            playerWeaponImage.sprite = player.CurrentWeapon.WeaponSprite;
        }

        public void UnBind(Player player)
        {
            playerHPGauge.Bind(null);
            playerGuardGauge.Bind(null);
        }

    }
}

