using ProjectJS.Skills;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace ProjectJS.UI.GameScene
{
    public class PlayerStatusUI : MonoBehaviour
    {
        [SerializeField] private HPGaugeUI playerHPGauge;
        [SerializeField] private GuardGaugeUI playerGuardGauge;
        [SerializeField] private Image playerWeaponImage;
        [SerializeField] private Image playerSkillFillImage;
        private float _skillCooldown  =1f;

        private void Start()
        {
            playerSkillFillImage.fillAmount = 1;
        }

        private void Update()
        {
            if(playerSkillFillImage.fillAmount <= 1)
            {
                float temp = playerSkillFillImage.fillAmount + Time.deltaTime / _skillCooldown;
                playerSkillFillImage.fillAmount = temp >= 1 ? 1 : temp;

                if(playerSkillFillImage.fillAmount == 1)
                {
                    playerSkillFillImage.color = Color.white;
                }
            }
        }

        public void Bind(Player player)
        {
            playerHPGauge.Bind(player);
            playerGuardGauge.Bind(player);
            playerWeaponImage.sprite = player.CurrentWeapon.WeaponSprite;
            player.GetComponent<PlayerSkillManager>().OnSkillCastedAction += OnSkillCasted;
        }

        public void UnBind(Player player)
        {
            playerHPGauge.Bind(null);
            playerGuardGauge.Bind(null);
        }

        public void OnSkillCasted(float cooldown)
        {
            _skillCooldown = cooldown;
            playerSkillFillImage.fillAmount = 0f;
            playerSkillFillImage.color = Color.blue;
        }

    }
}

