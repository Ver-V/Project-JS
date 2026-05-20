using TMPro;
using UnityEngine;

namespace ProjectJS.UI.GameScene
{
    public class GuardGaugeUI : GaugeUI
    {
        [SerializeField] private Player currentTarget;
        [SerializeField] private TMP_Text _guardGaugeText;

        public void Bind(Player target)
        {
            // 기존 구독 해제
            if (currentTarget != null)
                currentTarget.OnGuardGaugeChangedEvent -= UpdateGaugeBar;

            currentTarget = target;

            if (currentTarget != null)
            {
                currentTarget.OnGuardGaugeChangedEvent += UpdateGaugeBar;
                UpdateGaugeBar(currentTarget.CurGuardGauge, currentTarget.CurGuardGauge, currentTarget.Stats.MaxGuardGauge);
            }
            

            
        }
        protected override void UpdateGaugeBar(float prev, float current, float max)
        {
            base.UpdateGaugeBar(prev, current, max);
            _guardGaugeText.text = $"{current} / {max}";
        }

        protected override void OnDestroy()
        {
            Bind(null);
        }
    }
}
