using UnityEngine;

[CreateAssetMenu(menuName = "MazeLogic/TrainingHiddenLogic")]
public class TrainingHiddenLogic : HiddenRewardMazeLogic {
    public override void Setup(RewardArea[] rewards) {
        base.Setup(rewards);
        TrackEnterTriggerZone(false);
        TrackExitTriggerZone(false);
    }

    public override int GetNextTarget(int currentTarget, RewardArea[] rewards) {
        int target = base.GetNextTarget(currentTarget, rewards);

        SetRewardTargetVisible(rewards[target], true);

        return target;
    }

    public override void ProcessReward(RewardArea rewardArea, bool success) {
        //base.ProcessReward(rewardArea, success); ignore parent ProcessReward
        if (success) {
            SetRewardTargetVisible(rewardArea, false);
        }
    }
}
