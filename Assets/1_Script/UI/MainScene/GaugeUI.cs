using UnityEngine;
using UnityEngine.UI;

public class GaugeUI : MonoBehaviour
{
    [SerializeField] protected Slider gaugeSlider;
    protected virtual void Start()
    {
        if(gaugeSlider == null) gaugeSlider = GetComponent<Slider>();
        //targetScript.onGaugeChangedAction += UpdateHPBar;
        
    }

    protected virtual void UpdateGaugeBar(int currentHP)
    {
        gaugeSlider.value = currentHP;
    }

    protected virtual void OnDestroy()
    {
        //targetScript?.onGaugeChangedAction -= UpdateGuageBar;
    }

}
