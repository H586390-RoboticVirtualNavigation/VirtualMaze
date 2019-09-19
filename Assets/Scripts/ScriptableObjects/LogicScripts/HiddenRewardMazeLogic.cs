using UnityEngine;

[CreateAssetMenu(menuName = "MazeLogic/HiddenRewardMazeLogic")]
public class HiddenRewardMazeLogic : StandardMazeLogic {
    public override void Setup(RewardArea[] rewards) {
        base.Setup(rewards);
        foreach (RewardArea area in rewards) {
            SetRewardTargetVisible(area, false);
        }

        LevelController.OnEnteredTriggerZone += TriggerZoneEnter;
        LevelController.OnExitedTriggerZone += TriggerZoneExit;
    }

    private void TriggerZoneExit(RewardArea rewardArea, bool isTarget) {
        SetRewardTargetVisible(rewardArea, false);
    }

    private void TriggerZoneEnter(RewardArea rewardArea, bool isTarget) {
        if (isTarget) {
            SetRewardTargetVisible(rewardArea, true);
        }
    }

    void SetRewardTargetVisible(RewardArea area, bool visible) {
        area.target.gameObject.SetActive(visible);
    }

    public override void Cleanup(RewardArea[] rewards) {
        base.Cleanup(rewards);
        LevelController.OnEnteredTriggerZone -= TriggerZoneEnter;
        LevelController.OnExitedTriggerZone -= TriggerZoneExit;
    }

    public override void ProcessReward(RewardArea rewardArea) {
        base.ProcessReward(rewardArea);
        SetRewardTargetVisible(rewardArea, false);
    }
}
