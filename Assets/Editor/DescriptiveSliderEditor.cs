using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

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
