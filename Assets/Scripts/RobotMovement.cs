using UnityEngine;
using System;
using UnityEngine.Events;

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

    private Rigidbody rigidBody;
    private bool enableMovement = true;

    public float rotationSpeed;
    public float movementSpeed;

    public bool isJoystickEnabled;
    public bool isReverseEnabled;
    public bool isForwardEnabled;
    public bool isRightEnabled;
    public bool isLeftEnabled;

    //drag in Unity Editior
    public Camera player_camera;// camera to shoot the ray from.

    //movement broadcaster
    public event RobotMovementEvent OnRobotMoved;

    protected override void Awake() {
        base.Awake();
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.constraints = RigidbodyConstraints.FreezePositionY;
    }

    // Update is called once per frame
    void Update() {
        //do not do anything if movement disabled
        if (!enableMovement) return;

        float vertical;
        float horizontal;

        // using joy stick
        if (isJoystickEnabled) {
            vertical = JoystickController.vertical;
            horizontal = JoystickController.horizontal;
        }
        else {
            ///Input.GetAxis has smoothing of input values which is unwanted
            vertical = Input.GetAxisRaw("Vertical");
            horizontal = Input.GetAxisRaw("Horizontal");
        }

        if (ShouldRotate(horizontal)) {
            Quaternion rotateBy = Quaternion.Euler(0, horizontal * rotationSpeed * Time.deltaTime, 0);
            rigidBody.rotation = (transform.rotation * rotateBy);
        }

        if (ShouldMove(vertical)) {
            Vector3 moveBy = transform.forward * vertical * movementSpeed * Time.deltaTime;
            rigidBody.position = transform.position + moveBy;
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
        return (horizontal < 0 && isLeftEnabled) ||
            (horizontal > 0 && isRightEnabled);
    }

    /// <summary>
    /// Checks if object should move either forward, reverse, or both.
    /// </summary>
    /// <param name="vertical">Direction to move. Positive values means move forward</param>
    /// <returns>True if should move, false if not</returns>
    private bool ShouldMove(float vertical) {
        return (vertical < 0 && isReverseEnabled) ||
            (vertical > 0 && isForwardEnabled);
    }

    /// <summary>
    /// Move robot to the specified waypoint. 
    /// 
    /// The rotation of the robot is maintained.
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
    }

    /// <summary>
    /// Enables or disables the movement of the robot
    /// </summary>
    /// <param name="enable">true to enable</param>
    public void SetMovementActive(bool enable) {
        enableMovement = enable;
    }

    /// <summary>
    /// Fire a raycast from the center of the screen of the subject.
    /// </summary>
    /// <param name="maxDistance">Max distance to check for a hit</param>
    /// <param name="hit">Resultant hit, null if raycast didnot hit anything</param>
    /// <returns>True if somehting was hit</returns>
    public bool FireRaycastFromViewCenter(float maxDistance, out RaycastHit hit) {
        Ray r = player_camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        //Debug.DrawRay(r.origin, r.direction, Color.blue);
        return Physics.Raycast(r, out hit, maxDistance);
    }

    public override ComponentSettings GetCurrentSettings() {
        Settings settings = new Settings(rotationSpeed, movementSpeed,
            isJoystickEnabled, isReverseEnabled, isForwardEnabled,
            isRightEnabled, isLeftEnabled);

        return settings;
    }

    protected override void ApplySettings(ComponentSettings settings) {
        Settings applySettings = (Settings)settings;

        isJoystickEnabled = applySettings.isJoystickEnabled;
        isForwardEnabled = applySettings.isForwardEnabled;
        isReverseEnabled = applySettings.isReverseEnabled;
        isLeftEnabled = applySettings.isLeftEnabled;
        isRightEnabled = applySettings.isRightEnabled;

        movementSpeed = applySettings.movementSpeed;
        rotationSpeed = applySettings.rotationSpeed;
    }

    public override ComponentSettings GetDefaultSettings() {
        return new Settings(5f, 5f, true, true, true, true, true);
    }

    public override Type GetSettingsType() {
        return typeof(Settings);
    }
}
