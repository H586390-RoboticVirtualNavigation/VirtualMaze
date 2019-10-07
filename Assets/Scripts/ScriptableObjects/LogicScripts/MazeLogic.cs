using UnityEngine;

public abstract class MazeLogic : ScriptableObject, IMazeLogicProvider {
    public static int NullRewardIndex = -1;

    public abstract int GetNextTarget(int currentTarget, RewardArea[] rewards);
    public abstract bool IsTrialCompleteAfterReward(bool currentTaskSuccess);
    public abstract void Setup(RewardArea[] rewards);

    public virtual void Cleanup(RewardArea[] rewards) {
        Destroy(this);
    }

    public virtual void ProcessReward(RewardArea rewardArea) {
        Console.Write(rewardArea.target.name); // log reward name
    }
}
