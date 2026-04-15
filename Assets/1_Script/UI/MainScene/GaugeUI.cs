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

    protected virtual void UpdateGaugeBar(float current, float max)
    {
        gaugeSlider.value = current / max;
    }

    protected virtual void OnDestroy() { }

}
