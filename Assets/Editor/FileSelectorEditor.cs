using UnityEditor;
using UnityEngine;
using UnityEditor.UI;
using UnityEngine.UI;


[CustomEditor(typeof(FileSelector), false)]
[CanEditMultipleObjects]
public class FileSelectorEditor : InputFieldEditor {
    FileSelector mytarget;

    SerializedProperty OnPathSelected;

    private void Awake() {
        mytarget = (FileSelector)target;

    }

    protected override void OnEnable() {
        OnPathSelected = serializedObject.FindProperty("OnPathSelected");
        base.OnEnable();
    }

    public override void OnInspectorGUI() {
        mytarget.fb = (FileBrowser)EditorGUILayout.ObjectField("File Browser", mytarget.fb, typeof(FileBrowser), true);
        mytarget.browseBtn = (Button)EditorGUILayout.ObjectField("BrowseButton", mytarget.browseBtn, typeof(Button), true);
        Debug.Log(mytarget.fb);

        EditorGUILayout.PropertyField(OnPathSelected);
        base.OnInspectorGUI();
    }
}
