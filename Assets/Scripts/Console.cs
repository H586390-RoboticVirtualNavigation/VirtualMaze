using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Writes information to the Console GUI.
/// </summary>
public class Console {
    private const string Format_NoGameObject
        = "A Gameobject with a Text Component and tagged as {0} should exist in the scene.";

    private static Text _console;

    private static Text console {
        get {
            if (_console == null) {
                _console = GameObject.FindGameObjectWithTag(Tags.Console)?.GetComponent<Text>();
                if (_console == null) {
                    Debug.LogError(string.Format(Format_NoGameObject, Tags.Console));
                }
            }
            return _console;
        }
    }

    public static void WriteError(string text) {
        console.color = Color.red;
        console.text = text;
    }

    public static void Write(string text) {
        console.color = Color.black;
        console.text = text;
    }
}
