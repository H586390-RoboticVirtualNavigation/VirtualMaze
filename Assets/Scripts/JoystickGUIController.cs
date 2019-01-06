using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoystickGUIController : SettingsGUIController {
    //Drag in from Unity Editor
    public MySliderScript deadzoneSlider;
    public Button serialStateToggle;
    public Text serialStateText;
    public InputField joystickPortField;
    public JoystickController joystickController;

    private void Awake() {
        joystickPortField.onEndEdit.AddListener(onPortFieldEditEnd);
        deadzoneSlider.OnValueChanged.AddListener(onSliderValueChanged);
        serialStateToggle.onClick.AddListener(onSerialStateButtonClicked);
    }

    public override void UpdateSettingsGUI() {
        deadzoneSlider.value = joystickController.deadzoneAmount;
        joystickPortField.text = joystickController.portNum;

        joystickPortField.image.color = Color.white;
        joystickController.JoystickClose();
        serialStateText.text = "Open";
    }

    private void onSliderValueChanged(float value) {
        joystickController.deadzoneAmount = value;
    }
    private void onPortFieldEditEnd(string port) {
        joystickController.portNum = port;
    }

    public void onSerialStateButtonClicked() {
        if (!joystickController.isOpen) {
            if (joystickController.JoystickOpen()) {
                serialStateText.text = "Close";
                joystickPortField.image.color = Color.green;
            }
            else {
                //TODO report to console
                joystickPortField.image.color = Color.red;
            }
        }
        else {
            joystickController.JoystickClose();
            serialStateText.text = "Open";
            //joystickPortField.image.color = Color.red;
        }
    }
}
