using UnityEngine;
using UnityEngine.UI;

public class RobotMovementGUIController : DataGUIController {
    //drag and drop respective GameObjects with corresponding components in Unity Editor.
    public RobotMovement robotMovement;

    public DescriptiveSlider movementSpeedSlider;
    public DescriptiveSlider rotationSpeedSlider;

    public Toggle isJoystickEnabledToggle;
    public Toggle isForwardEnabledToggle;
    public Toggle isReverseEnabledToggle;
    public Toggle isLeftEnabledToggle;
    public Toggle isRightEnabledToggle;

    [SerializeField]
    private Toggle isXInverted;

    [SerializeField]
    private Toggle isYInverted;

    private void Awake() {
        rotationSpeedSlider.onValueChanged.AddListener(OnRotationSpeedChanged);
        movementSpeedSlider.onValueChanged.AddListener(OnMovementSpeedChanged);

        robotMovement.OnConfigChanged.AddListener(UpdateSettingsGUI);
    }

    public override void UpdateSettingsGUI() {
        movementSpeedSlider.value = robotMovement.MovementSpeed;
        rotationSpeedSlider.value = robotMovement.RotationSpeed;

        isJoystickEnabledToggle.isOn = robotMovement.IsJoystickEnabled;
        isForwardEnabledToggle.isOn = robotMovement.IsForwardEnabled;
        isReverseEnabledToggle.isOn = robotMovement.IsReverseEnabled;
        isLeftEnabledToggle.isOn = robotMovement.IsLeftEnabled;
        isRightEnabledToggle.isOn = robotMovement.IsRightEnabled;

        isXInverted.isOn = robotMovement.IsXInverted;
        isYInverted.isOn = robotMovement.IsYInverted;
    }

    //listeners for UI GameObjects, drag and drop in respective Child UI Components
    public void OnRotationSpeedChanged(float value) {
        robotMovement.RotationSpeed = value;
    }

    public void OnMovementSpeedChanged(float value) {
        robotMovement.MovementSpeed = value;
    }

    public void OnJoystickEnableToggled(bool value) {
        robotMovement.IsJoystickEnabled = value;
    }

    public void OnForwardEnableToggled(bool value) {
        robotMovement.IsForwardEnabled = value;
    }

    public void OnReverseEnableToggled(bool value) {
        robotMovement.IsReverseEnabled = value;
    }

    public void OnLeftEnableToggled(bool value) {
        robotMovement.IsLeftEnabled = value;
    }

    public void OnRightEnableToggled(bool value) {
        robotMovement.IsRightEnabled = value;
    }

    public void OnXInvertToggled(bool value) {
        robotMovement.IsXInverted = value;
    }

    public void OnYInvertToggled(bool value) {
        robotMovement.IsYInverted = value;
    }
    //End of Listeners for UI Objects
}
