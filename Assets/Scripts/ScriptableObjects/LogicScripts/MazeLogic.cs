using UnityEngine;

public abstract class MazeLogic : ScriptableObject, IMazeLogicProvider {
    public abstract int GetNextTarget(int currentTarget, RewardArea[] rewards);
    public abstract bool IsTrialCompleteAfterReward();
    public abstract void Setup(RewardArea[] rewards);

    public void ProcessReward(RewardArea rewardArea) {
        Console.Write(rewardArea.target.name); // log reward name
    }
}
