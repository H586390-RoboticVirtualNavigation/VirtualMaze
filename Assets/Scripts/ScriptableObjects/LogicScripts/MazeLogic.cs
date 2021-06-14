using UnityEngine;

public abstract class MazeLogic : ScriptableObject, IMazeLogicProvider {
    public static int NullRewardIndex = -1;

    public abstract int GetNextTarget(int currentTarget, RewardArea[] rewards);
    public abstract bool IsTrialCompleteAfterCurrentTask(bool currentTaskSuccess);
    public abstract void Setup(RewardArea[] rewards);
    public abstract Sprite GetTargetImage(RewardArea[] rewards, int targetIndex); 

    /// <summary>
    /// Triggers when the player presses spacebar in target rewardArea
    /// </summary>
    /// <param name="rewardArea">RewardArea of the trigger zone entered</param>
    public event OnRewardTriggered RewardTriggered;
    
    /// <summary>
    /// Triggers when the player presses spacebar in wrong rewardArea
    /// </summary>
    /// <param name="rewardArea">RewardArea of the trigger zone entered</param>
    public event OnWrongRewardTriggered WrongRewardTriggered;
    public abstract bool ShowCue(int targetIndex);

    public virtual void Cleanup(RewardArea[] rewards) {
        Destroy(this);
    }
    public virtual void ProcessReward(RewardArea rewardArea, bool success) {
        if (rewardArea.target != null) {
            Console.Write($"{rewardArea.target.name} : {success}"); // log reward name
        }
    }
    protected virtual void OnRewardTriggered(RewardArea rewardArea) {
        RewardTriggered?.Invoke(rewardArea);
    }

    protected virtual void OnWrongRewardTriggered() {
        WrongRewardTriggered?.Invoke();
    }
}
