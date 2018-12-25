using UnityEngine;
using UnityEngine.Events;

public class RobotMovement : ConfigurableComponent {

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

    private Rigidbody rigidBody;

    //default values
    public float rotationSpeed;
    public float movementSpeed;

    public bool isJoystickEnabled;
    public bool isReverseEnabled;
    public bool isForwardEnabled;
    public bool isRightEnabled;
    public bool isLeftEnabled;

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
            Quaternion rotateBy = Quaternion.Euler(0, horizontal * rotationSpeed * Time.deltaTime, 0);
            Debug.Log(rotateBy);
            rigidBody.MoveRotation(transform.rotation * rotateBy);
        }

        if (ShouldMove(vertical)) {
            Vector3 moveBy = transform.forward * vertical * movementSpeed * Time.deltaTime;
            Debug.Log(moveBy);
            rigidBody.MovePosition(transform.position + moveBy);
        }
    }

    /// <summary>
    /// Checks if object should rotate either left, right or both.
    /// </summary>
    /// <returns>True if should rotate, false if not</returns>
    private bool ShouldRotate(float horizontal) {
        return (horizontal < 0 && isLeftEnabled) ||
            (horizontal > 0 && isRightEnabled);
    }

    private bool ShouldMove(float vertical) {
        return (vertical < 0 && isReverseEnabled) ||
            (vertical > 0 && isForwardEnabled);
    }

    public override SerializableSettings GetSavableSettings() {
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

    protected override void ApplySavableSettings(SerializableSettings settings) {
        Settings applySettings = (Settings)settings;

        isJoystickEnabled = applySettings.isJoystickEnabled;
        isForwardEnabled = applySettings.isForwardEnabled;
        isReverseEnabled = applySettings.isReverseEnabled;
        isLeftEnabled = applySettings.isLeftEnabled;
        isRightEnabled = applySettings.isRightEnabled;

        movementSpeed = applySettings.movementSpeed;
        rotationSpeed = applySettings.rotationSpeed;
    }

    public override SerializableSettings GetDefaultSettings() {
        return new Settings() {
            isJoystickEnabled = true,
            isForwardEnabled = true,
            isReverseEnabled = true,
            isLeftEnabled = true,
            isRightEnabled = true,

            movementSpeed = 5f,
            rotationSpeed = 5f
        };
    }

    public override string GetConfigID() {
        return typeof(Settings).FullName;
    }
}
