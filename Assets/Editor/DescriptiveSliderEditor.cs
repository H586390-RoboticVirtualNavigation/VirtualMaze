using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

[CustomEditor(typeof(DescriptiveSlider), false)]
[CanEditMultipleObjects]
public class DescriptiveSliderEditor : SliderEditor {
    DescriptiveSlider mytarget;

    private void Awake() {
        mytarget = (DescriptiveSlider)target;
    }

    public override void OnInspectorGUI() {
        mytarget.valueText = (Text) EditorGUILayout.ObjectField("Value Text",mytarget.valueText, typeof(Text), true);
        base.OnInspectorGUI();
    }
}
