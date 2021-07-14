using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// The poster, which is a mesh, requires a material to show a picture.
/// Therefore make sure that the cueImage (Sprite) in this script matches the
/// material unless the experiment requires it to be different.
/// 
/// Make sure the object attached with this script is tagged as a 
/// <see cref="Tags.RewardArea"/>. 
/// </summary>
/// <seealso cref="GameObject.FindGameObjectsWithTag(string)"/>
public class RewardArea : MonoBehaviour {
    //Drag in Unity Editor
    /// <summary>
    /// image to display as cue
    /// </summary>
    public Sprite cueImage;

    public MeshRenderer imageRenderer;

    /// <summary>
    /// Optional target of the Reward Area
    /// if left null, this reward area will send an RewardTriggered event once
    /// the subject enters the collider.
    /// </summary> 
    public Transform target;

    /// <summary>
    /// optional blinkLight
    /// </summary>
    public Renderer blinkLight;

    /// <summary>
    /// viewing angle required to register if the target is in sight
    /// 
    /// Arbitrary max angle decided based on a reasonable field of view
    /// [Range(0, 110)]
    /// </summary>
    public static float RequiredViewAngle {
        get => s_requiredViewAngle;
        set {
            float v = Mathf.Clamp(value, 0, 110);
            s_requiredViewAngle = v;
            if (v != value) {
                Console.Write($"Value Clamped to {v}");
            }
        }
    }

    /// <summary>
    /// Minimum valid distance from the target.
    /// 
    /// maximum length is decided by the radius of the sphere collider in the 
    /// Cube Reward Prefab.
    /// [Range(0, 7)]
    /// </summary>

    public static float RequiredDistance {
        get => s_requiredDistance;
        set {
            float v = Mathf.Clamp(value, 0, 7);
            s_requiredDistance = v;
            if (v != value) {
                Console.Write($"Value Clamped to {v}");
            }
        }
    }

    private static float s_requiredViewAngle = 110f;
    private static float s_requiredDistance = 2f;
    /// <summary>
    /// Sends an event when subject is in range.
    /// 
    /// maximum length is decided by the radius of the sphere collider in the 
    /// Cube Reward Prefab.
    /// </summary>
    [Range(0, 7)]
    private static float s_proximityDistance = 3.5f;

    /// <summary>
    /// The order of the reward. RewardAreas will be sorted in ascending order.
    /// <see cref="GetAllRewardsFromScene"/>
    /// 
    /// DO NOT change the value here change it in the Unity Editor
    /// </summary>
    [SerializeField]
    private int rewardOrder = -1;

    /// <summary>
    /// use this instead of <see cref="GameObject.SetActive(bool)"/> so that
    /// code in this script will still run when inactive.
    /// </summary>
    public bool IsActivated { get; set; } = true;

    public static float ProximityDistance {
        get => s_proximityDistance;
        set {
            float v = Mathf.Clamp(value, 0, 7);
            s_proximityDistance = v;
            if (v != value) {
                Console.Write($"Value Clamped to {v}");
            }
        }
    }

    

    /// <summary>
    /// All proximity events use the same trigger event. RewardArea object will be returned for extra processing
    /// </summary>
    /// <param name="rewardArea">RewardArea that is triggered</param>
    public delegate void OnProximityEnteredEvent(RewardArea rewardArea);
    public static event OnProximityEnteredEvent OnProximityTriggered;

    /// <summary>
    /// All rewards use the same trigger event. RewardArea object will be returned for extra processing
    /// </summary>
    /// <param name="rewardArea">RewardArea that is triggered</param>
    public delegate void OnRewardTriggeredEvent(RewardArea rewardArea);
    public static event OnRewardTriggeredEvent OnRewardTriggered;

    /// <summary>
    /// Triggers when the player enter the RewardArea collider
    /// </summary>
    /// <param name="rewardArea">RewardArea of the trigger zone entered</param>
    public delegate void OnEnterTriggerZone(RewardArea rewardArea);
    public static event OnEnterTriggerZone OnEnteredTriggerZone;

    /// <summary>
    /// Triggers when the player leaves the RewardArea Collider
    /// </summary>
    /// <param name="rewardArea">RewardArea of the trigger zone entered</param>
    public delegate void OnExitTriggerZone(RewardArea rewardArea);
    public static event OnExitTriggerZone OnExitedTriggerZone;

    /// <summary>
    /// Triggers when the player is within the RewardArea Collider
    /// </summary>
    /// <param name="rewardArea">RewardArea of the trigger zone entered</param>
    public delegate void InTriggerZone(RewardArea rewardArea);
    public static event InTriggerZone InTriggerZoneListener;

    /// <summary>
    /// Checks the field of view of the robot in respect to target when in proximity
    /// </summary>
    public delegate void CheckFieldOfViewInProximity (Transform robot, RewardArea target, float reqProxDist, float reqDist, float reqAngle);
    public static event CheckFieldOfViewInProximity CheckViewInProximity;
    
    //for blinking logic
    private bool blinkState;
    private readonly WaitForSeconds half_period = new WaitForSeconds(0.5f);
    private Coroutine blinkCoroutine; // reference to properly stop the coroutine


    //constants
    private const string Format_NoRewardAreaComponentFound = "{0} does not have a RewardAreaComponent but is tagged as a reward";
    private const string emissionKeyword = "_EMISSION";


    protected virtual void Start() {
        if (blinkLight != null) {
            blinkLight.material.DisableKeyword(emissionKeyword);
            blinkState = false;
        }
    }

    void Update() {
        StartCoroutine(BlinkReward(this));
    }
    /* only checks for proximity when the subject enters the collider */
    protected virtual void OnTriggerStay(Collider other) {
        if (target == null && IsActivated) { //RewardAreas used as checkpoints (without posters)
            OnRewardTriggered?.Invoke(this);
        }       
        else if (IsActivated) { //Any other activated RewardAreas
            // CheckFieldOfView(other.transform);
            OnRewardWithinProximity(other.transform);
        }


        InTriggerZoneListener?.Invoke(this);
    }

    private void OnTriggerEnter(Collider other) {
        OnEnteredTriggerZone?.Invoke(this);
    }

    private void OnTriggerExit(Collider other) {
        OnExitedTriggerZone?.Invoke(this);
    }

    public void OnProximityEntered() {
        OnProximityTriggered?.Invoke(this);
    }

    public void Triggered() {
        OnRewardTriggered?.Invoke(this);
    }
    public void OnRewardWithinProximity(Transform robot) {
        CheckViewInProximity?.Invoke(robot, this, s_proximityDistance, s_requiredDistance, s_requiredViewAngle);
    }

    /// <summary>
    /// Checks if the poster is within the robot's field of view.
    /// </summary>
    /// <param name="robot">Current transform of the robot</param>
    private void CheckFieldOfView(Transform robot) {
        Vector3 direction = target.position - robot.position;
        direction.y = 0; // ignore y axis

        float angle = Vector3.Angle(direction, robot.forward);

        //uncomment to see the required view in the scene tab
        if (Debug.isDebugBuild) {
            Vector3 left = Quaternion.AngleAxis(-s_requiredViewAngle / 2f, Vector3.up) * robot.forward * RequiredDistance;
            Vector3 right = Quaternion.AngleAxis(s_requiredViewAngle / 2f, Vector3.up) * robot.forward * RequiredDistance;
            Debug.DrawRay(robot.position, left, Color.black);
            Debug.DrawRay(robot.position, right, Color.black);
            Debug.DrawRay(robot.position, direction.normalized * RequiredDistance, Color.cyan);
        }

        float distance = Vector3.Magnitude(direction);
        Debug.Log($"dist:{distance} / {s_proximityDistance}");
        Debug.Log($"angle:{angle} / {s_requiredViewAngle}");
        if (distance <= s_proximityDistance) {
            OnProximityTriggered?.Invoke(this);
            Debug.Log("RewardProx");
        }

        //check if in view angle
        if (angle < s_requiredViewAngle * 0.5f) {
            //checks if close enough
            if (distance <= RequiredDistance) {
                Debug.Log("Reward!!!");
                OnRewardTriggered?.Invoke(this);
            }
        }
    }

    public void StartBlinking() {
        Debug.Log("Failed to blink");
		float timer = 0f; 
		int numBlinks = 4;
		float overallBlinkDuration = 0.5f; 
        if (blinkLight != null) {
            Debug.Log("Blinking");
            blinkCoroutine = StartCoroutine(Blink());
        }
    }


    
    
    // Used by TrainingHiddenLogicMD2 to make reward blink to indicate within in zone
    public void StartBlinkingReward(RewardArea reward) {
        blinkState = true;
    }

    // Blinks reward every half second while blinkState is true 
    public IEnumerator BlinkReward(RewardArea reward) {
        float overallBlinkDuration = 0.5f;
        while (blinkState) {
            Debug.Log("Blinking");
            reward.target.gameObject.SetActive(true);
            yield return new WaitForSeconds(overallBlinkDuration / 2);
            reward.target.gameObject.SetActive(false);
            yield return new WaitForSeconds(overallBlinkDuration / 2);
        }
    }    

    // Used by TrainingHiddenLogicMD2 to stop reward from blinking
    public void StopBlinkingReward(RewardArea reward) {
        blinkState = false;
        reward.target.gameObject.SetActive(true);
    }

    public void StopBlinking() {
        if (blinkCoroutine != null) {
            StopCoroutine(blinkCoroutine);
        }

        if (blinkLight != null) {
            blinkLight.material.DisableKeyword(emissionKeyword);
        }
    }

    private IEnumerator Blink() {
        while (true) {
            if (blinkState) {
                blinkLight.material.DisableKeyword(emissionKeyword);
            }
            else {
                blinkLight.material.EnableKeyword(emissionKeyword);
            }
            blinkState = !blinkState;
            yield return half_period;
        }
    }

    /// <summary>
    /// Looks into the scene for all of the RewardArea and their children classes
    /// 
    /// Cache the results of this method and use it infrequently as possible.
    /// Searching through the scene for these objects is expensive;
    /// </summary>
    /// <returns></returns>
    public static RewardArea[] GetAllRewardsFromScene() {
        //Find all rewardAreas in scene and populate rewards[].
        GameObject[] objs = GameObject.FindGameObjectsWithTag(Tags.RewardArea);
        RewardArea[] tempArr = new RewardArea[objs.Length];

        for (int i = 0; i < objs.Length; i++) {
            RewardArea area = objs[i].GetComponent<RewardArea>();
            if (area != null) {
                tempArr[i] = area;

                // Deactivate all rewards at the start.
                area.IsActivated = false;
            }
            else {
                Debug.LogWarning(string.Format(Format_NoRewardAreaComponentFound, objs[0].name));
            }
        }

        Array.Sort(tempArr, (a1, a2) => a1.rewardOrder.CompareTo(a2.rewardOrder));

        return tempArr;
    }
}
