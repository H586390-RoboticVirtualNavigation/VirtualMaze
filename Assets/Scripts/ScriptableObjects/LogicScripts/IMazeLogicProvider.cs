using UnityEngine;

public interface IMazeLogicProvider {
    void Setup(RewardArea[] rewards);
    void Cleanup(RewardArea[] rewards);
    void ProcessReward(RewardArea rewardArea);
    bool IsTrialCompleteAfterCurrentTask(bool currentTaskSuccess);
    int GetNextTarget(int currentTarget, RewardArea[] rewards);
    bool ShowCue(int targetIndex);
    Sprite GetTargetImage(RewardArea[] rewards, int targetIndex);
}
