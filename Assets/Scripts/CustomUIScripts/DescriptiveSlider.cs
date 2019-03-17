using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Extension of Slider UI Component
/// </summary>
//[RequireComponent(typeof(Slider))]
public class DescriptiveSlider : Slider {
    //private Slider slider;
    public Text valueText;

    protected override void Set(float input, bool sendCallback) {
        float oldValue = value;
        base.Set(input, sendCallback);

        if(value != oldValue) {
            valueText.text = value.ToString(Format_ValueFormat);
        }
    }



    //public float value {
    //    get { return slider.value; }
    //    set {
    //        slider.value = value;
    //        valueText.text = value.ToString(Format_ValueFormat);
    //    }
    //}

    //[HideInInspector]
    //public Slider.SliderEvent onValueChanged;

    //Formats value to 2 significant numbers
    private const string Format_ValueFormat = "0.00";

    //private void Awake() {
    //    slider = GetComponent<Slider>();
    //    onValueChanged = slider.onValueChanged;
    //    onValueChanged.AddListener(OnChildSliderChanged);
    //}

    //private void OnChildSliderChanged(float value) {
    //    valueText.text = value.ToString(Format_ValueFormat);
    //    onValueChanged.Invoke(value);
    //}
}
