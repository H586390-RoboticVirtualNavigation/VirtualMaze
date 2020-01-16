using UnityEngine;

[CreateAssetMenu(menuName = "MazeLogic/FixedRewardMazeLogic")]
public class FixedMazeLogic : MazeLogic {
    private int rewardIndex;
    private int numRewards;

    public override int GetNextTarget(int currentTarget, RewardArea[] rewards) {
        int nextTarget;
        if (currentTarget == NullRewardIndex) {
            nextTarget = 0;
        }
        else {
            nextTarget = currentTarget + 1;
        }

        rewardIndex = nextTarget;

        return Mathf.Min(nextTarget, numRewards - 1);
    }

    public override Sprite GetTargetImage(RewardArea[] rewards, int targetIndex) {
        return rewards[rewards.Length - 1].cueImage;
    }

    public override bool IsTrialCompleteAfterCurrentTask(bool currentTaskSuccess) {
        return currentTaskSuccess && rewardIndex >= numRewards - 1;
    }

    public override void Setup(RewardArea[] rewards) {
        numRewards = rewards.Length;
    }

    public override bool ShowCue(int targetIndex) {
        return targetIndex == 0;
    }
}
