using TMPro;
using UnityEngine;

public class HPGaugeUI : GaugeUI
{
    //[SerializeField] private HealthComponent currentTarget;
    [SerializeField] private TMP_Text _hpGaugeText;

    //public void Bind(HealthComponent target)
    //{
    //    // 기존 구독 해제
    //    if (currentTarget != null)
    //        currentTarget.onGaugeChanged -= UpdateGaugeBar;
    //
    //    currentTarget = target;
    //
    //    if (currentTarget != null)
    //        currentTarget.onGaugeChanged += UpdateGaugeBar;
    //}
    protected override void UpdateGaugeBar(float current, float max)
    {
        base.UpdateGaugeBar(current, max);
        _hpGaugeText.text = $"{current} / {max}";

        //[임시] 체력 비율에 따른 색 변화
        fillImage.color = Color.Lerp(Color.red, Color.yellow, current / max);
        
    }

    protected override void OnDestroy()
    {
        //Bind(null);
    }
}