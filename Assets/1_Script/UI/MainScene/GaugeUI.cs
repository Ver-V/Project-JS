using UnityEngine;
using UnityEngine.UI;

public class GaugeUI : MonoBehaviour
{
    [SerializeField] protected Slider gaugeSlider;
    [SerializeField] protected Image fillImage; // 색상 변화용

    protected virtual void Start()
    {
        if (gaugeSlider == null) gaugeSlider = GetComponent<Slider>();
    }

    //아래 두 함수 중에 어떤거 쓸 지 Player.cs 보고 결정
    protected virtual void UpdateGaugeBar(float current, float max)
    {
        gaugeSlider.value = current / max;
    }
    protected virtual void UpdateGaugeBar(float percent)
    {
        gaugeSlider.value = percent;
    }

    protected virtual void OnDestroy() { }

}
