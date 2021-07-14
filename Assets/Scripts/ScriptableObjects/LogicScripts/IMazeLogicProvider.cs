using UnityEngine;
public delegate void OnRewardTriggered (RewardArea rewardArea);
public delegate void OnWrongRewardTriggered ();
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

    /// <summary>
    /// Triggers when reward is processed
    /// </summary>
    /// <param name="rewardArea">RewardArea that is processed</param>
    event OnRewardTriggered RewardTriggered;

    /// <summary>
    /// Triggers when wrong reward is chosen
    /// </summary>
    event OnWrongRewardTriggered WrongRewardTriggered;

    /// <summary>
    /// Checks the field of view of the robot in respect to target when in proximity
    /// </summary>
    void CheckFieldOfView(Transform robot, RewardArea reward, float s_proximityDistance, float RequiredDistance, float s_requiredViewAngle);


    /// <summary>
    /// Decides if trial should end based on maze logic.
    /// 
    /// ** Used for ending trials in LevelController
    /// </summary>
    /// <param name="reward">Target reward in the trials</param>
    void TrialListener(RewardArea target);

    /// <summary>
    /// Allow preprocessing of the rewards in the maze before each trial starts
    /// </summary>
    /// <param name="rewards">Array of all the rewards in the trials</param>
    /// <param name="target">index of target reward in array</param>
    void TrialSetup(RewardArea[] rewards, int target);

    /// <summary>
    /// Sets endTrial boolean to status; determines if trial should end
    /// </summary>
    /// <param name="status">status of trial</param>
    void IsTrialOver(bool status);

    /// <summary>
    /// Returns endTrial boolean
    /// </summary>
    bool EndTrial();

    bool DeathSceneComplete();
    bool ExecuteDeathScene();
    void StartDeathScene(bool status);

    void SetDeathSceneStatus(bool complete);
}
