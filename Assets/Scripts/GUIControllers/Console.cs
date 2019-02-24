using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Writes information to the Console GUI.
/// </summary>
public class Console : MonoBehaviour {
    private const string Format_NoGameObject
        = "A Gameobject with a Text Component and tagged as {0} should exist in the scene.";

    public Text console;

    public static Console Instance { get; private set; }

    //Singleton Implementation
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }
    }

    public static void WriteError(string text) {
        Instance.console.color = Color.red;
        Instance.console.text = text;
    }

    public static void Write(string text) {
        Instance.console.color = Color.black;
        Instance.console.text = text;
    }
}
