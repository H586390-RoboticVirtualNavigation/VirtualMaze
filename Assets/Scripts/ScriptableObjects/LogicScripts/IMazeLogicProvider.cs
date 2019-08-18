public interface IMazeLogicProvider {
    void Setup(RewardArea[] rewards);
    void ProcessReward(RewardArea rewardArea);
    bool IsTrialCompleteAfterReward();
    int GetNextTarget(int currentTarget, RewardArea[] rewards);
}
