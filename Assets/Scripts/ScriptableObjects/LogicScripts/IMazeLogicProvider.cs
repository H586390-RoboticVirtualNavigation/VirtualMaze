using UnityEngine;

public interface IMazeLogicProvider {
    /// <summary>
    /// Allow preprocessing of the rewards in the maze before the trial starts
    /// </summary>
    /// <param name="rewards">Array of all the rewards in the trials</param>
    void Setup(RewardArea[] rewards);

    /// <summary>
    /// Allows cleaning up of any data or rewards after the trial has ended
    /// </summary>
    /// <param name="rewards">Array of all the rewards in the trials</param>
    void Cleanup(RewardArea[] rewards);

    /// <summary>
    /// Allows the code to process the reward that the subject has successfully reached.
    /// </summary>
    /// <param name="rewardArea">reward area of the current target</param>
    /// <param name="success">True if the current task is successful</param>
    void ProcessReward(RewardArea rewardArea, bool success);

    /// <summary>
    /// Allows developers to define when a trial is complete. This is used when
    /// the subject has to reach more than 1 target/posters in a trial.
    /// </summary>
    /// <param name="currentTaskSuccess">True if subject is successful when looking for the current target</param>
    /// <returns>Return False if there is another target before the trial ends. True if there is no more additional targets</returns>
    bool IsTrialCompleteAfterCurrentTask(bool currentTaskSuccess);

    /// <summary>
    /// Decides the next target based on the current target. The array of all 
    /// rewards are also provided for selection or further processing.
    /// </summary>
    /// <param name="currentTarget">current target given to the subject.</param>
    /// <param name="rewards">Array of all rewards found in the Maze</param>
    /// <returns>the index of the reward with respect to the rewards array</returns>
    int GetNextTarget(int currentTarget, RewardArea[] rewards);

    /// <summary>
    /// Decides if cue should be shown to the subject when the target is changed.
    /// 
    /// ** Used when RewardAreas are used as Checkpoints with no posters
    /// </summary>
    /// <param name="targetIndex">current target given to the subject</param>
    /// <returns>True if the cue should be shown. False if not</returns>
    bool ShowCue(int targetIndex);

    /// <summary>
    /// Selects the image to be shown to the subject. Image sprites can be given from
    /// the code or images of any of the other rewards.
    /// </summary>
    /// <param name="rewards">Array of all available rewards</param>
    /// <param name="targetIndex">Current target given to the subject</param>
    /// <returns></returns>
    Sprite GetTargetImage(RewardArea[] rewards, int targetIndex);
}
