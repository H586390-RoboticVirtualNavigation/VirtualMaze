using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoystickGUIController : DataGUIController {
    //Drag in from Unity Editor
    public DescriptiveSlider deadzoneSlider;
    public Button serialStateToggle;
    public Text serialStateText;
    public InputField joystickPortField;
    public JoystickController joystickController;

    private void Awake() {
        joystickPortField.onEndEdit.AddListener(onPortFieldEditEnd);
        deadzoneSlider.onValueChanged.AddListener(onSliderValueChanged);
        serialStateToggle.onClick.AddListener(onSerialStateButtonClicked);
    }

    public override void UpdateSettingsGUI() {
        deadzoneSlider.value = joystickController.DeadzoneAmount;
        joystickPortField.text = joystickController.PortNum;

        joystickPortField.image.color = Color.white;
        joystickController.JoystickClose();
        serialStateText.text = "Open";
    }

    private void onSliderValueChanged(float value) {
        joystickController.DeadzoneAmount = value;
    }
    private void onPortFieldEditEnd(string port) {
        joystickController.PortNum = port;
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
