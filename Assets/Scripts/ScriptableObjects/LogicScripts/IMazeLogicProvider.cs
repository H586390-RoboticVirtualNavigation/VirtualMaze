public interface IMazeLogicProvider {
    void Setup(RewardArea[] rewards);
    void Cleanup(RewardArea[] rewards);
    void ProcessReward(RewardArea rewardArea);
    bool IsTrialCompleteAfterReward(bool currentTaskSuccess);
    int GetNextTarget(int currentTarget, RewardArea[] rewards);
}
