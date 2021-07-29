using UnityEngine;
using System;
using System.Collections;

public class DirectionError : MonoBehaviour
{
    public AudioClip errorClip;
    public RobotMovement robotMovement;
    public CueController cueController;
    public LevelController lvlController;

    public RewardArea[] rewards { get; private set; }
    protected IMazeLogicProvider logicProvider;
    private int currentTargetIndex = MazeLogic.NullRewardIndex;
    private int previousTargetIndex = MazeLogic.NullRewardIndex;

    public bool enableDirectionError = false;
    private bool isSoundTriggered = false;
    private bool isRewardsGet = false;
    private bool hasBeenExecutedDuringThisTrial = false;
    private bool allowInternalTrialCounterUpdate = true;
    private float timer = 1000f;
    private float distanceDiff;
    private int previousTrial = 0;
    private int tempPreviousTrial = 0;
    public int internalTrialCounter = 0;
    private RewardArea previousReward;

    private int[,] correctTurnSign;

    [Range(0, 25)]
    private static float s_distanceRange = 3f;

    public static float distanceRange
    {
        get => s_distanceRange;
        set
        {
            float v = Mathf.Clamp(value, 0, 25);
            s_distanceRange = v;
            if (v != value)
            {
                Console.Write($"Value Clamped to {v}");
            }
        }
    }

    void Awake()
    {
        // Left - Positive, Right - Negative (Flip sign for Camel, Donkey, Pig)
        // correctTurn: [., L, R, R, R, L], // Cat, Camel, Rabbit,  Donkey, Crocodile, Pig
        //              [R, ., R, L, R, R],
        //              [L, R, ., R, R, R],
        //              [R, R, L, ., L, R],
        //              [L, L, R, L, ., L],
        //              [L, R, L, L, L, .],

        correctTurnSign = new int[6, 6] {
            { 0, 1, -1, -1, -1, 1 },
            { 1, 0, 1, -1, 1, 1 },
            { 1, -1, 0, -1, -1, -1 },
            { 1, 1, -1, 0, -1, 1 },
            { 1, 1, -1, 1, 0, 1 },
            { -1, 1, -1, -1, -1, 0 }
        };
    }

    void Update()
    {
        // Checks if a session is currently running
        if (LevelController.sessionStarted && (enableDirectionError) && !(lvlController.resetRobotPositionDuringInterTrial)) {
            if (!isRewardsGet) {
                rewards = lvlController.rewards;
                isRewardsGet = true;
            }
            CheckDirection();
            HintBlink();
        }
        else {
            // Reset();
            isRewardsGet = false;
        }

        // Check if robot is going in the correct direction
        void CheckDirection()
        {
            timer += Time.deltaTime;

            if (internalTrialCounter < lvlController.trialCounter) { // Set internalTrialCounter to follow lvlController.trialCounter for first session
                internalTrialCounter += 1;
            }
            else if ((internalTrialCounter > lvlController.trialCounter) && allowInternalTrialCounterUpdate) { // Set internalTrialCounter to increase incrementally every time lvlController.trialCounter increases
                if (lvlController.trialCounter == 0) {
                    internalTrialCounter -= 1;
                }
                internalTrialCounter += 1;
                tempPreviousTrial = lvlController.trialCounter;
                allowInternalTrialCounterUpdate = false;
            }
            else if (tempPreviousTrial != lvlController.trialCounter) {
                allowInternalTrialCounterUpdate = true;
            }

            // Debug.Log("internalTrialCounter: " + internalTrialCounter);
            // Debug.Log("lvlController.trialCounter: " + lvlController.trialCounter);
            // Debug.Log("tempPreviousTrial: " + tempPreviousTrial);

            if (!hasBeenExecutedDuringThisTrial && !lvlController.success) { // Execute at start of each trial
                currentTargetIndex = lvlController.targetIndex;
                Debug.Log("Current Index:" + currentTargetIndex);
                previousTrial = internalTrialCounter;
                hasBeenExecutedDuringThisTrial = true;
                Debug.Log("hasBeenExecutedDuringThisTrial");
            }

            if (internalTrialCounter > 0) { // Ignore first trial

                // Debug.Log("success? " + lvlController.success);
                if ((previousTrial != internalTrialCounter) && !lvlController.success) { // Change in trial number
                    Debug.Log("Current Trial: " + internalTrialCounter);
                    Debug.Log("Previous Trial: " + previousTrial);
                    previousTargetIndex = currentTargetIndex; // Store current target index as previous index
                    Debug.Log("previousTargetIndex: " + previousTargetIndex);
                    previousReward = rewards[previousTargetIndex];
                    hasBeenExecutedDuringThisTrial = false;
                    isSoundTriggered = false;
                }

                var currentPos = robotMovement.getRobotTransform().position;
                // Debug.Log("Current Position: " + currentPos);
                // Debug.Log("Reward Area Position: " + previousReward.transform.position);
                if (previousTargetIndex == 1 || previousTargetIndex == 2) {
                    distanceDiff = previousReward.transform.position.z - currentPos.z;
                }
                else {
                    distanceDiff = previousReward.transform.position.x - currentPos.x;
                }

                // Debug.Log("distanceDiff: " + distanceDiff);

                if (Math.Abs(distanceDiff) > distanceRange && !isSoundTriggered) { // Chechk if distance is larger than set distance range
                    if (Math.Sign(distanceDiff) != correctTurnSign[previousTargetIndex, currentTargetIndex]) { // Wrong direction
                        WrongDirectionSound();
                    }
                    isSoundTriggered = true;
                }
            }
        }
    }

    private void WrongDirectionSound()
    {
        PlayerAudio.instance.PlayErrorClip();
        timer = 0f; // For resetting the blinking timer
    }

    // Number and duration of blinks
    int numBlinks = 4;
    float overallBlinkDuration = 0.5f;

    private void HintBlink()
    {
        for (int i = 0; i < numBlinks; i++)
        {
            if (timer >= (i * overallBlinkDuration) && timer < (((2 * i) + 1) * overallBlinkDuration / 2))
            {
                cueController.HideHint();
            }
            if (timer >= (((2 * i) + 1) * overallBlinkDuration / 2) && timer < ((i + 1) * overallBlinkDuration))
            {
                cueController.ShowHint();
            }
        }
    }

    public void Reset()
    {
        timer = 1000f;
        isSoundTriggered = false;
        isRewardsGet = false;
        hasBeenExecutedDuringThisTrial = false;
        allowInternalTrialCounterUpdate = true;
        internalTrialCounter = 0;
        currentTargetIndex = MazeLogic.NullRewardIndex;
        previousTargetIndex = MazeLogic.NullRewardIndex;
        previousTrial = 0;
        tempPreviousTrial = 0;
        logicProvider?.Cleanup(rewards);
    }
}