using UnityEngine;

[CreateAssetMenu(menuName = "MazeLogic/HiddenRewardMazeLogicMD2")]
public class HiddenRewardMazeLogicMD2 : StandardMazeLogic {

    public override void Setup(RewardArea[] rewards) {
        base.Setup(rewards);
        foreach (RewardArea area in rewards) {
            SetRewardTargetVisible(area, false);
        }

        TrackEnterProximity(true);
        TrackExitTriggerZone(true);
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
    public override int GetNextTarget(int currentTarget, RewardArea[] rewards) {
        LevelController.InTriggerZoneListener += WhileInTriggerZone;
        return base.GetNextTarget(currentTarget, rewards);
    }

    private void WhileInTriggerZone(RewardArea rewardArea, bool isTarget) {
        if (Input.GetKeyDown("space")) {
            if (isTarget) {
                SetRewardTargetVisible(rewardArea, true);
                ProcessReward(rewardArea, true);
            } else {
                Debug.Log("death scene");
                base.OnWrongRewardTriggered();
            }
        }
    }

    private void TriggerZoneExit(RewardArea rewardArea, bool isTarget) {
        SetRewardTargetVisible(rewardArea, false);

    }

    private void TriggerZoneEnter(RewardArea rewardArea, bool isTarget) {
        if (Input.GetKeyDown("space")) {
            SetRewardTargetVisible(rewardArea, true);
        }
    }

    protected void SetRewardTargetVisible(RewardArea area, bool visible) {
        area.target.gameObject.SetActive(visible);
    }

    public override void Cleanup(RewardArea[] rewards) {
        foreach (RewardArea area in rewards) {
            SetRewardTargetVisible(area, false);
        }
        base.Cleanup(rewards);
        TrackEnterProximity(false);
        TrackExitTriggerZone(false);
    }

    public override void ProcessReward(RewardArea rewardArea, bool success) {
        //Prints to console which reward is processed
        base.ProcessReward(rewardArea, success);
        base.OnRewardTriggered(rewardArea);
    }

}