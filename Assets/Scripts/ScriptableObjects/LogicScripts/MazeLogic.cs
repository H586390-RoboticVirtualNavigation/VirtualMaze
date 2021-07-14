using UnityEngine;

public abstract class MazeLogic : ScriptableObject, IMazeLogicProvider {
    public static int NullRewardIndex = -1;
    private bool endTrial = false;

    private bool deathSceneComplete = false;

    private bool hasDeathScene = false;

    public abstract int GetNextTarget(int currentTarget, RewardArea[] rewards);
    public abstract bool IsTrialCompleteAfterCurrentTask(bool currentTaskSuccess);
    public abstract void Setup(RewardArea[] rewards);
    public abstract Sprite GetTargetImage(RewardArea[] rewards, int targetIndex); 


    /// <summary>
    /// Triggers when the player presses spacebar in target rewardArea
    /// </summary>
    /// <param name="rewardArea">RewardArea of the trigger zone entered</param>
    public event OnRewardTriggered RewardTriggered;
    
    /// <summary>
    /// Triggers when the player presses spacebar in wrong rewardArea
    /// </summary>
    /// <param name="rewardArea">RewardArea of the trigger zone entered</param>
    public event OnWrongRewardTriggered WrongRewardTriggered;

    public abstract bool ShowCue(int targetIndex);

    public virtual void Cleanup(RewardArea[] rewards) {
        endTrial = false;
        deathSceneComplete = false;
        hasDeathScene = false;
        Destroy(this);
    }
    public virtual void ProcessReward(RewardArea rewardArea, bool success) {
        if (rewardArea.target != null) {
            Console.Write($"{rewardArea.target.name} : {success}"); // log reward name
        }
    }
    protected virtual void OnRewardTriggered(RewardArea rewardArea) {
        RewardTriggered?.Invoke(rewardArea);
    }

    protected virtual void OnWrongRewardTriggered() {
        Debug.Log("Wrong Reward");
        PlayerAudio.instance.PlayErrorClip(); //play audio
        WrongRewardTriggered?.Invoke();
    }

    public virtual void CheckFieldOfView(Transform robot, RewardArea reward, float s_proximityDistance, float RequiredDistance, float s_requiredViewAngle) {
        Transform target = reward.target;
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
            reward.OnProximityEntered();
            Debug.Log("RewardProx");
        }

        //check if in view angle
        if (angle < s_requiredViewAngle * 0.5f) {
            //checks if close enough
            reward.Triggered();
        }
    }

    public virtual void TrialListener(RewardArea target) {
    }

    public virtual void TrialSetup(RewardArea[] rewards, int target) {
    }

    public virtual bool EndTrial() {
        return endTrial;
    }

    public virtual void IsTrialOver(bool status) {
        endTrial = status;
    }
    public virtual bool ExecuteDeathScene() {
        return hasDeathScene;
    }
    public virtual bool DeathSceneComplete() {
        return deathSceneComplete;
    }
    
    public virtual void StartDeathScene(bool status) {
        hasDeathScene = status;
    }
    public virtual void SetDeathSceneStatus(bool status) {
        deathSceneComplete = status;
    }

}
