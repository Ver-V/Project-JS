using ProjectJS.Controller;
using TMPro;
using UnityEngine;


namespace ProjectJS.UI.GameScene
{
    public class BossHPUI : GaugeUI
    {
        [SerializeField] private TMP_Text bossNameText;
        private BossController bossController;

        public void Bind(BossController target)
        {
            if (bossController != null)
                bossController.OnHealthChangedEvent -= UpdateGaugeBar;
        
            bossController = target;
        
            if (bossController == null)
            {
                bossNameText.text = string.Empty;
                UpdateGaugeBar(0, 0, 1);
                return;
            }
        
            bossController.OnHealthChangedEvent += UpdateGaugeBar;

            //임시 보스 이름 설정
            bossNameText.text = bossController.name;
        
            UpdateGaugeBar(1,1,1);
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
