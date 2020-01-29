using UnityEngine;

public abstract class MazeLogic : ScriptableObject, IMazeLogicProvider {
    public static int NullRewardIndex = -1;

    public abstract int GetNextTarget(int currentTarget, RewardArea[] rewards);
    public abstract bool IsTrialCompleteAfterCurrentTask(bool currentTaskSuccess);
    public abstract void Setup(RewardArea[] rewards);
    public abstract Sprite GetTargetImage(RewardArea[] rewards, int targetIndex);

    public abstract bool ShowCue(int targetIndex);

    public virtual void Cleanup(RewardArea[] rewards) {
        Destroy(this);
    }

    public virtual void ProcessReward(RewardArea rewardArea, bool success) {
        if (rewardArea.target != null) {
            Console.Write($"{rewardArea.target.name} : {success}"); // log reward name
        }
    }
}
