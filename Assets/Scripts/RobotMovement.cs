using System;
using UnityEngine;

/// <summary>
/// Script to control the movement of the robot
/// 
/// If robotmovement.enable = false, the script will be unable to broadcast the robot's location
/// therefore, use SetMovmentActive(bool active)
/// </summary>
public class RobotMovement : ConfigurableComponent {
    //delegate to broadcast current movement position of Robot
    public delegate void RobotMovementEvent(Transform transform);

    /// <summary>
    /// Wrapper class for RobotMovement settings
    /// </summary>
    [System.Serializable]
    public class Settings : ComponentSettings {
        public float rotationSpeed;
        public float movementSpeed;

        public bool isJoystickEnabled;
        public bool isReverseEnabled;
        public bool isForwardEnabled;
        public bool isRightEnabled;
        public bool isLeftEnabled;

        public Settings(
            float rotationSpeed,
            float movementSpeed,
            bool isJoystickEnabled,
            bool isReverseEnabled,
            bool isForwardEnabled,
            bool isRightEnabled,
            bool isLeftEnabled
            ) {

            this.rotationSpeed = rotationSpeed;
            this.movementSpeed = movementSpeed;

            this.isForwardEnabled = isForwardEnabled;
            this.isJoystickEnabled = isJoystickEnabled;
            this.isLeftEnabled = isLeftEnabled;
            this.isRightEnabled = isRightEnabled;
            this.isReverseEnabled = isReverseEnabled;
        }
    }

    [SerializeField]
    private CharacterController charController = null; //drag and drop
    private bool enableMovement = true;

    private Settings settings;

    public float RotationSpeed { get => settings.rotationSpeed; set => settings.rotationSpeed = value; }
    public float MovementSpeed { get => settings.movementSpeed; set => settings.movementSpeed = value; }

    public bool IsForwardEnabled { get => settings.isForwardEnabled; set => settings.isForwardEnabled = value; }
    public bool IsJoystickEnabled { get => settings.isJoystickEnabled; set => settings.isJoystickEnabled = value; }
    public bool IsLeftEnabled { get => settings.isLeftEnabled; set => settings.isLeftEnabled = value; }
    public bool IsRightEnabled { get => settings.isRightEnabled; set => settings.isRightEnabled = value; }
    public bool IsReverseEnabled { get => settings.isReverseEnabled; set => settings.isReverseEnabled = value; }

    //movement broadcaster
    public event RobotMovementEvent OnRobotMoved;

    protected override void Awake() {
        base.Awake();
    }

    // Update is called once per frame
    void Update() {
        //do not do anything if movement disabled
        if (!enableMovement) return;

        float vertical;
        float horizontal;

        // using joy stick
        if (IsJoystickEnabled) {
            vertical = JoystickController.vertical;
            horizontal = JoystickController.horizontal;
        }
        else {
            ///Input.GetAxis has smoothing of input values which is unwanted
            vertical = Input.GetAxisRaw("Vertical");
            horizontal = Input.GetAxisRaw("Horizontal");
        }

        if (ShouldRotate(horizontal)) {
            Quaternion rotateBy = Quaternion.Euler(0, horizontal * RotationSpeed * Time.deltaTime, 0);
            transform.rotation = (transform.rotation * rotateBy);
        }

        if (ShouldMove(vertical)) {
            Vector3 moveBy = transform.forward * vertical * MovementSpeed * Time.deltaTime;
            charController.Move(moveBy);
        }
    }

    //LateUpdate runs after physics(FixedUpdate()) and gamelogic (Update()) therefore should
    //reflect the latest position of the robot
    private void LateUpdate() {
        //broadcast movement if there are listeners
        OnRobotMoved?.Invoke(transform);
    }

    /// <summary>
    /// Checks if object should rotate either left, right, or both.
    /// </summary>
    /// <param name="horizontal">Direction to rotate. Negative values means rotate left</param>
    /// <returns>True if should rotate, false if not</returns>
    private bool ShouldRotate(float horizontal) {
        return (horizontal < 0 && IsLeftEnabled) ||
            (horizontal > 0 && IsRightEnabled);
    }

    /// <summary>
    /// Checks if object should move either forward, reverse, or both.
    /// </summary>
    /// <param name="vertical">Direction to move. Positive values means move forward</param>
    /// <returns>True if should move, false if not</returns>
    private bool ShouldMove(float vertical) {
        return (vertical < 0 && IsReverseEnabled) ||
            (vertical > 0 && IsForwardEnabled);
    }

    /// <summary>
    /// Move robot to the specified waypoint. 
    /// 
    /// The rotation of the robot follows the Y rotation of the waypoint.
    /// </summary>
    /// <param name="waypoint"></param>
    public void MoveToWaypoint(Transform waypoint) {
        Vector3 startpos = waypoint.position;
        //do not want to change y axis of robot
        startpos.y = transform.position.y;

        transform.position = startpos;

        Quaternion startrot = transform.rotation;
        startrot.y = waypoint.rotation.y;
        transform.rotation = startrot;

        OnRobotMoved?.Invoke(transform);
    }

    /// <summary>
    /// Enables or disables the movement of the robot
    /// </summary>
    /// <param name="enable">true to enable</param>
    public void SetMovementActive(bool enable) {
        enableMovement = enable;
    }

    public override ComponentSettings GetCurrentSettings() {
        return settings;
    }

    protected override void ApplySettings(ComponentSettings settings) {
        this.settings = (Settings)settings;
    }

    public override ComponentSettings GetDefaultSettings() {
        return new Settings(5f, 5f, true, true, true, true, true);
    }

    public override Type GetSettingsType() {
        return typeof(Settings);
    }

    public static void MoveRobotTo(Transform robot, RobotConfiguration config) {
        if (config == null) {
            return;
        }

        Vector3 pos = robot.position;
        // Y is unchanged
        pos.x = Convert.ToSingle(config.x);
        pos.z = Convert.ToSingle(config.z);

        // Rotate around Y axis
        Vector3 orientation = robot.rotation.eulerAngles;
        orientation.y = Convert.ToSingle(config.degreeY);

        //convert back to quaterion
        Quaternion newrot = Quaternion.Euler(orientation);

        robot.SetPositionAndRotation(pos, newrot);
    }
}


public class RobotConfiguration {
    public readonly double x, z, degreeY;

    public RobotConfiguration(double x, double z, double degreeY) {
        this.x = x;
        this.z = z;
        this.degreeY = degreeY;
    }

    public RobotConfiguration(Vector3 transformPosition, double degreeY) : this(transformPosition.x, transformPosition.y, degreeY) {
    }
}