using UnityEngine;

public abstract class MazeLogic : ScriptableObject, IMazeLogicProvider {
    public abstract int GetNextTarget(int currentTarget, RewardArea[] rewards);
    public abstract bool IsTrialCompleteAfterReward(bool currentTaskSuccess);
    public abstract void Setup(RewardArea[] rewards);

    public void ProcessReward(RewardArea rewardArea) {
        Console.Write(rewardArea.target.name); // log reward name
    }
}
