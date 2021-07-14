using UnityEngine;

[CreateAssetMenu(menuName = "MazeLogic/HiddenRewardMazeLogicMD2")]
public class HiddenRewardMazeLogicMD2 : StandardMazeLogic {

    private bool inView = false;

    public bool inZone = false;

    public override void Setup(RewardArea[] rewards) {
        inView = false;
        base.Setup(rewards);
        foreach (RewardArea area in rewards) {
            SetRewardTargetVisible(area, false);
        }
        base.StartDeathScene(false);

        base.SetDeathSceneStatus(false);

        TrackEnterProximity(true);
        TrackExitTriggerZone(true);
        TrackFieldOfView(true);
        TrackInTriggerZone(true);
        RewardArea.RequiredViewAngle = 180f;
    }

    protected void TrackExitTriggerZone(bool enable) {
        if (enable) {
            LevelController.OnExitedTriggerZone += TriggerZoneExit;
        }
        else {
            LevelController.OnExitedTriggerZone -= TriggerZoneExit;
        }
    }

    protected void TrackEnterProximity(bool enable) {
        if (enable) {
            LevelController.InRewardProximityEvent += TriggerZoneEnter;
        }
        else {
            LevelController.InRewardProximityEvent -= TriggerZoneEnter;
        }
    }

    protected void TrackFieldOfView(bool enable) {
        if (enable) {
            LevelController.CheckViewInProximityEvent += CheckFieldOfView;
        }
        else {
            LevelController.CheckViewInProximityEvent -= CheckFieldOfView;
        }
    }

    protected void TrackInTriggerZone(bool enable) {
        if (enable) {
            LevelController.InTriggerZoneListener += WhileInTriggerZone;
        }
        else {
            LevelController.InTriggerZoneListener -= WhileInTriggerZone;
        }
    }
    public override int GetNextTarget(int currentTarget, RewardArea[] rewards) {
        LevelController.InTriggerZoneListener += WhileInTriggerZone;
        return base.GetNextTarget(currentTarget, rewards);
    }

    protected virtual void WhileInTriggerZone(RewardArea rewardArea, bool isTarget) {
        inZone = true;
        if (Input.GetKeyDown("space")) {
            TrackInTriggerZone(false);
            ProcessReward(rewardArea, inView & isTarget);
        }
    }

    private void TriggerZoneExit(RewardArea rewardArea, bool isTarget) {
        inZone = false;
        SetRewardTargetVisible(rewardArea, false);
    }

    private void TriggerZoneEnter(RewardArea rewardArea, bool isTarget) {
        inZone = true;
        if (Input.GetKeyDown("space")) {
            SetRewardTargetVisible(rewardArea, true);
        }
    }

    protected void SetRewardTargetVisible(RewardArea area, bool visible) {
        area.target.gameObject.SetActive(visible);
    }

    public override void Cleanup(RewardArea[] rewards) {
        inView = false;
        foreach (RewardArea area in rewards) {
            SetRewardTargetVisible(area, false);
        }
        base.Cleanup(rewards);
        TrackEnterProximity(false);
        TrackExitTriggerZone(false);
    }

    public override void ProcessReward(RewardArea rewardArea, bool success) {
        base.IsTrialOver(true);
        SetRewardTargetVisible(rewardArea, true);
        
        if (success) {
            base.StartDeathScene(false);
            base.OnRewardTriggered(rewardArea);
        } else {
            base.StartDeathScene(true);
            base.OnWrongRewardTriggered();
        }
        //Prints to console which reward is processed
        base.ProcessReward(rewardArea, success);
    }

    public override void CheckFieldOfView(Transform robot, RewardArea reward, float s_proximityDistance, float RequiredDistance, float s_requiredViewAngle) {
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
            Debug.Log("RewardProx");
            reward.OnProximityEntered();
        }

        //check if in view angle
        if (angle < s_requiredViewAngle * 0.5f || (distance < 1)) {
            //checks if close enough
            Debug.Log("In View!!!");
            inView = true;
            if (Input.GetKeyDown("space")) {
                    ProcessReward(reward, true);
            }
        } else {
            inView = false;
        }
    }

    // Continously called in while loop in LevelController. Used to listen for Spacebar press
    public override void TrialListener(RewardArea target) {
        if (Input.GetKeyDown("space")) { 
            ProcessReward(target, inView);
        }
    }

    // Setup right before trial begins
    public override void TrialSetup(RewardArea[] rewards, int target) {
        inView = false;
        foreach (RewardArea area in rewards) {
            SetRewardTargetVisible(area, false);
        }
    }
}