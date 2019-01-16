using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// The poster, which is a mesh, requires a material to show a picture.
/// Therefore make sure that the cueImage (Sprite) in this script matches the
/// material unless the experiment requires it to be different.
/// </summary>
public class RewardArea : MonoBehaviour {
    //Drag in Unity Editor
    /// <summary>
    /// // image to display as cue
    /// </summary>
    public Sprite cueImage;

    /// <summary>
    /// Target of the Reward Area
    /// </summary> 
    public Transform target;

    /// <summary>
    /// viewing angle required to register if the target is in sight
    /// </summary>
    public float fieldOfViewAngle = 110f;

    /// <summary>
    /// Minimum valid distance from the target.
    /// </summary>
    [Range(0.1f, 2f)]
    public float distance = 1;

    /// <summary>
    /// All rewards use the same trigger event. RewardArea script will be returned for extra processing
    /// </summary>
    /// <param name="rewardArea">Script of RewardArea that is triggered</param>
    public delegate void OnRewardTriggeredEvent(RewardArea rewardArea);
    public static event OnRewardTriggeredEvent OnRewardTriggered;

    private EventManager eventManager;
    private RobotMovement robotMovement;


    private void Awake() {
        eventManager = EventManager.eventManager;
        GameObject robot = GameObject.FindWithTag(Tags.Player);
        robotMovement = robot.GetComponent<RobotMovement>();
    }
    private void Update() {
        Debug.DrawRay(transform.position, transform.forward * distance);
    }
    //start listening to robot it enters trigger area
    private void OnTriggerEnter(Collider other) {
        robotMovement.OnRobotMoved += CheckFieldOfView;
    }

    /// <summary>
    /// Checks if the poster is within the robot's field of view.
    /// </summary>
    /// <param name="robot">Current transform of the robot</param>
    private void CheckFieldOfView(Transform robot) {
        Vector3 direction = target.position - robot.position;
        direction.y = 0; // ignore y axis

        float angle = Vector3.Angle(direction, robot.forward);

        Debug.DrawRay(robot.position, robot.forward * distance, Color.black);
        Debug.DrawRay(robot.position, direction.normalized * distance, Color.cyan);

        //robot fires raycast to check if the robot is close enough to the target
        if (angle < fieldOfViewAngle * 0.5f) {//only fires raycast if target is in field of view
            if (robotMovement.FireRaycastFromViewCenter(distance, out RaycastHit hit)) {
                OnRewardTriggered?.Invoke(this);
            }
        }
    }

    //stop listening to robot it enters trigger area
    private void OnTriggerExit(Collider other) {
        robotMovement.OnRobotMoved -= CheckFieldOfView;
    }

    private void OnDisable() {
        //remove listener once disabled
        robotMovement.OnRobotMoved -= CheckFieldOfView;
    }

    /// <summary>
    /// Helper method to access GameObject.SetActive
    /// </summary>
    /// <param name="value"></param>
    public void SetActive(bool value) {
        gameObject.SetActive(value);
    }
}
