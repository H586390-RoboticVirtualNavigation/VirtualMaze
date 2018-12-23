using UnityEngine;
using UnityEngine.Events;

public class RobotMovement : MonoBehaviour, IConfigurableComponent {

    /// <summary>
    /// Wrapper class for RobotMovement settings
    /// </summary>
    [System.Serializable]
    public class Settings : SerializableSettings {
        public float rotationSpeed;
        public float movementSpeed;

        public bool isJoystickEnabled;
        public bool isReverseEnabled;
        public bool isForwardEnabled;
        public bool isRightEnabled;
        public bool isLeftEnabled;
    }

    public UnityEvent OnConfigChanged  = new UnityEvent();

    private Rigidbody rigidBody;

    //default values
    private float rotationSpeed;
    private float movementSpeed;

    private bool isJoystickEnabled;
    private bool isReverseEnabled;
    private bool isForwardEnabled;
    private bool isRightEnabled;
    private bool isLeftEnabled;

    void Awake() {
        //Apply default settings first.
        ApplySavableSettings(GetDefaultSettings());

        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        float vertical;
        float horizontal;

        // using joy stick
        if (isJoystickEnabled) {
            vertical = SerialController.vertical;
            horizontal = SerialController.horizontal;
        }
        else {
            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
        }

        if (ShouldRotate(horizontal)) {
            Quaternion rotateBy = Quaternion.Euler(0, SerialController.horizontal * rotationSpeed * Time.deltaTime, 0);
            rigidBody.MoveRotation(transform.rotation * rotateBy);
        }

        if (ShouldMove(vertical)) {
            Vector3 moveBy = transform.forward * SerialController.vertical * movementSpeed * Time.deltaTime;
            rigidBody.MovePosition(transform.position + moveBy);
        }
    }

    /// <summary>
    /// Checks if object should rotate either left, right or both.
    /// </summary>
    /// <returns>True if should rotate, false if not</returns>
    private bool ShouldRotate(float horizontal) {
        return (horizontal < 0 && isLeftEnabled) ||
            (horizontal > 0 && !isRightEnabled);
    }

    private bool ShouldMove(float vertical) {
        return (vertical < 0 && isReverseEnabled) ||
            (vertical > 0 && isForwardEnabled);
    }

    public SerializableSettings GetSavableSettings() {
        Settings settings = new Settings();

        settings.isJoystickEnabled = isJoystickEnabled;
        settings.isForwardEnabled = isForwardEnabled;
        settings.isReverseEnabled = isReverseEnabled;
        settings.isLeftEnabled = isLeftEnabled;
        settings.isRightEnabled = isRightEnabled;

        settings.movementSpeed = movementSpeed;
        settings.rotationSpeed = rotationSpeed;

        return settings;
    }

    public void ApplySavableSettings(SerializableSettings settings) {
        Settings applySettings = (Settings)settings;

        isJoystickEnabled = applySettings.isJoystickEnabled;
        isForwardEnabled = applySettings.isForwardEnabled;
        isReverseEnabled = applySettings.isReverseEnabled;
        isLeftEnabled = applySettings.isLeftEnabled;
        isRightEnabled = applySettings.isRightEnabled;

        movementSpeed = applySettings.movementSpeed;
        rotationSpeed = applySettings.rotationSpeed;
    }

    public SerializableSettings GetDefaultSettings() {
        return new Settings() {
            isJoystickEnabled = true,
            isForwardEnabled = true,
            isReverseEnabled = true,
            isLeftEnabled = true,
            isRightEnabled = true,

            movementSpeed = 5,
            rotationSpeed = 5
        };
    }


    public string GetConfigID() {   
        return typeof(Settings).FullName;
    }

    public void OnRotationSpeedChanged(float value) {
        rotationSpeed = value;
    }

    public void OnMovementSpeedChanged(float value) {
        movementSpeed = value;
    }

    public void OnJoystickEnableToggled(bool value) {
        isJoystickEnabled = value;
    }

    public void OnForwardEnableToggled(bool value) {
        isForwardEnabled = value;
    }

    public void OnReverseEnableToggled(bool value) {
        isReverseEnabled = value;
    }

    public void OnLeftEnableToggled(bool value) {
        isLeftEnabled = value;
    }

    public void OnRightEnableToggled(bool value) {
        isRightEnabled = value;
    }
}
