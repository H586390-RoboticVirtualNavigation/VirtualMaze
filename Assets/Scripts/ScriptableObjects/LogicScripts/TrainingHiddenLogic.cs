using UnityEngine;

[CreateAssetMenu(menuName = "MazeLogic/TrainingHiddenLogic")]
public class TrainingHiddenLogic : HiddenRewardMazeLogic {
    int[] order;
    int index = 0;

    public override void Setup(RewardArea[] rewards) {
        base.Setup(rewards);
        TrackEnterProximity(false);
        TrackExitTriggerZone(false);

        order = new int[rewards.Length];
        for (int i = 0; i < order.Length; i++) {
            order[i] = i;
        }

        ShuffledMazeLogic.Shuffle(order);
    }

    public override int GetNextTarget(int currentTarget, RewardArea[] rewards) {
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
