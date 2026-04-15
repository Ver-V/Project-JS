using TMPro;
using UnityEngine;

public class GuardGaugeUI : GaugeUI
{
    //[SerializeField] private GuardComponent currentTarget;
    [SerializeField] private TMP_Text _guardGaugeText;

    //public void Bind(GuardComponent target)
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
        _guardGaugeText.text = $"{current} / {max}";
    }

    protected override void OnDestroy()
    {
        //Bind(null);
    }
}