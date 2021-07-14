using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "MazeLogic/TrainingHiddenLogicMD2")]
public class TrainingHiddenLogicMD2 : HiddenRewardMazeLogicMD2 {

    private bool inView = false;
    int[] order;
    int index = 0;

    public override void Setup(RewardArea[] rewards) {
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
        
        order = new int[rewards.Length];
        for (int i = 0; i < order.Length; i++) {
            order[i] = i;
        }

        ShuffledMazeLogic.Shuffle(order);
    }

    public override int GetNextTarget(int currentTarget, RewardArea[] rewards) {
        Debug.Log("Target is changing");
        int target = -1;
        if (index == order.Length) { //reshuffle if all rewards are completed
            ShuffledMazeLogic.Shuffle(order);
            while (order[0] == currentTarget) { //keep shuffling till the next target is not the same as the current
                ShuffledMazeLogic.Shuffle(order);
            }
            index = 0;
        }

        target = order[index];

        index++;

        return target;
    }

    protected override void WhileInTriggerZone(RewardArea rewardArea, bool isTarget) {
        inZone = true;
        if (Input.GetKeyDown("space")) {
            TrackInTriggerZone(false);
            base.IsTrialOver(true);
            Debug.Log("InZone: " + inView + ", " + isTarget);
            if (inView & isTarget) {
                ProcessReward(rewardArea, true);
            } else {
                base.StartDeathScene(true);
                base.OnWrongRewardTriggered();
            }
        }
    }

    private void TriggerZoneExit(RewardArea rewardArea, bool isTarget) {
        inZone = false;
        rewardArea.StopBlinkingReward(rewardArea);
    }

    public override void Cleanup(RewardArea[] rewards) {
        base.Cleanup(rewards);

        foreach (RewardArea area in rewards) {
            SetRewardTargetVisible(area, false);
        }
        TrackEnterProximity(false);
        TrackExitTriggerZone(false);        

    }

    public override void ProcessReward(RewardArea rewardArea, bool success) {
        if (success) {
            base.StartDeathScene(false);
        }
        //Prints to console which reward is processed
        base.ProcessReward(rewardArea, success);
        base.OnRewardTriggered(rewardArea);
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
        // Debug.Log($"dist:{distance} / {s_proximityDistance}");
        // Debug.Log($"angle:{angle} / {s_requiredViewAngle}");
        if (distance <= s_proximityDistance) {
            reward.OnProximityEntered();

            //check if in view angle
            if (angle < s_requiredViewAngle * 0.5f || (distance <  1)) {
                //checks if close enough
                inView = true;
                reward.StartBlinkingReward(reward);
            } else {
                inView = false;
                reward.StopBlinkingReward(reward);
            }
        } else {
            inView = false;
        }

    }

    // Continously called in while loop in LevelController. Used to listen for Spacebar press
    public override void TrialListener(RewardArea target) {
        if (!inView) {
            target.StopBlinkingReward(target);
        }

        if (Input.GetKeyDown("space")) {
            base.IsTrialOver(true);
            if (!inView) {
                base.StartDeathScene(true);
                base.OnWrongRewardTriggered();
            } else {
                base.OnRewardTriggered(target);
            }
        }
    }
    

    // Setup right before trial begins
    public override void TrialSetup(RewardArea[] rewards, int target) {
        foreach (RewardArea area in rewards) {
            SetRewardTargetVisible(area, false);
        }
        SetRewardTargetVisible(rewards[target], true);

    }

}