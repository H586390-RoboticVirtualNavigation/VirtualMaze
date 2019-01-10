using UnityEngine;
using UnityEngine.UI;


public class MySliderScript : MonoBehaviour {
    public Slider slider;
    public Text valueText;

    public float maxValue = 1f;
    public float minValue = 0f;

    public Slider.SliderEvent OnValueChanged;
    public float value {
        get { return slider.value; }
        set {
            slider.value = value;
            valueText.text = value.ToString(Format_ValueFormat);
        }
    }

    //Formats value to 2 significant numbers
    private const string Format_ValueFormat = "0.00";

    private void Awake() {
        slider.maxValue = maxValue;
        slider.minValue = minValue;
    }

    /// <summary>
    /// Convienence method to bubble up OnValueChanged() from Child Slider
    /// and set value Text
    /// </summary>
    /// <param name="value"></param>
    private void OnChildSliderChanged(float value) {
        //if(slider.value == value) {
        //    //prevents feedback loop
        //    return;
        //}
        valueText.text = value.ToString(Format_ValueFormat);
        OnValueChanged.Invoke(value);
    }
}
