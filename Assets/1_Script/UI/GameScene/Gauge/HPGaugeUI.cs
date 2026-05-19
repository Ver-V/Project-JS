using TMPro;
using UnityEngine;

namespace ProjectJS.UI.GameScene 
{
    public class HPGaugeUI : GaugeUI
    {
        [SerializeField] private Player currentTarget;
        [SerializeField] private TMP_Text _hpGaugeText;
        [SerializeField] private float maxHealth;

        public void Bind(Player target)
        {
            // 기존 구독 해제
            if (currentTarget != null)
                currentTarget.OnHealthChangedEvent -= UpdateGaugeBar;

            currentTarget = target;

            if (currentTarget != null)
            {
                currentTarget.OnHealthChangedEvent += UpdateGaugeBar;

                UpdateGaugeBar(currentTarget.CurHealthGauge, currentTarget.CurHealthGauge, currentTarget.Stats.MaxHealth);
            }
        }
        protected override void UpdateGaugeBar(float prev, float current, float max)
        {
            //슬라이더 값 반영
            base.UpdateGaugeBar(prev, current, max);
            _hpGaugeText.text = $"{current} / {max}";

            //[임시] 체력 비율에 따른 색 변화
            fillImage.color = Color.Lerp(Color.red, Color.yellow, current / max);

        }

        protected override void OnDestroy()
        {
            Bind(null);
        }
    }
}

