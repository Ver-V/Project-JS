using ProjectJS.Manager;
using TMPro;
using UnityEngine;

namespace ProjectJS.UI.GameScene
{
    public class PartyHPUI : GaugeUI
    {
        [SerializeField] private TMP_Text partyClientNametext;

        private Player currentTarget;

        public void Bind(Player target)
        {
            if (currentTarget != null)
                currentTarget.OnHealthChangedEvent -= UpdateGaugeBar;

            currentTarget = target;

            if (currentTarget == null)
            {
                partyClientNametext.text = string.Empty;
                UpdateGaugeBar(0, 0, 1);
                return;
            }

            currentTarget.OnHealthChangedEvent += UpdateGaugeBar;

            partyClientNametext.text = GetPartyClientName(currentTarget);

            UpdateGaugeBar(currentTarget.CurHealthGauge, currentTarget.CurHealthGauge, currentTarget.Stats.MaxHealth);
        }

        private string GetPartyClientName(Player player)
        {
            return $"Player {player.OwnerClientId}";
        }

        protected override void UpdateGaugeBar(float prev, float current, float max)
        {
            base.UpdateGaugeBar(prev, current, max);
        }

        protected override void OnDestroy()
        {
            Bind(null);
        }
    }
}
