using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

/// <summary>
/// Level Controllers manage an Experiment Session.
/// Each session will have either a single or multiple trials.
/// Each trial may contain more than 1 task.
/// 
/// BasicLevelController is programmed to have only 1 task per trial.
/// Override desired methods to implement more than 1 task per trial or any custom code.
/// 
/// </summary>
[Obsolete("LevelController have taken over the task of this class", error: false)]
public class BasicLevelController : MonoBehaviour {
    // Broadcasts when the session is finshed.
    public UnityEvent onSessionFinishEvent = new UnityEvent();

    // Broadcasts when any sessionTriggers happens.
    public SessionTriggerEvent onSessionTrigger = new SessionTriggerEvent();

    public bool isPaused;

    //drag and drop from Unity Editor
    public Transform startWaypoint;

    /// <summary>
    /// Flag to decide if the trail should be restarted if the subject failed
    /// the trial.
    /// </summary>
    public bool restartOnTaskFail = true;
    public bool resetRobotPositionDuringInterTrial = false;
    protected int trialCounter { get; private set; } = 0;

    /// <summary>
    /// Gameobjects tagged as "RewardArea" in the scene will be populated in here.
    /// </summary>
    public RewardArea[] rewards { get; private set; }

    protected Session session { get; private set; }
    protected RobotMovement robotMovement { get; private set; }
    protected CueController cueController { get; private set; }
    protected WaitForSecondsRealtime cueDisplayDuration { get; private set; } = new WaitForSecondsRealtime(1f);

    //reference to coroutine to properly stop it.
    private Coroutine timeLimitTimer;

    // cache waitForUnpause for efficiency
    private WaitUntil waitIfPaused;

    private int targetIndex;
    private bool firstTask = true;

    //Strings
    private const string Format_NoRewardAreaComponentFound = "{0} does not have a RewardAreaComponent";

    private void Awake() {
        waitIfPaused = new WaitUntil(() => !isPaused);

        //Prepare Eyelink
        EyeLink.Initialize();
        onSessionTrigger.AddListener(EyeLink.OnSessionTrigger);

        GameObject robot = GameObject.FindGameObjectWithTag(Tags.Player);
        robotMovement = robot.GetComponent<RobotMovement>();
        cueController = robot.GetComponentInChildren<CueController>();
    }

    /// <summary>
    /// Override this to place additional commands during Awake().
    /// </summary>
    protected virtual void Setup() { }

    private void OnEnable() {
        RewardArea.OnRewardTriggered += OnRewardTriggered;
    }

    private void OnDisable() {
        RewardArea.OnRewardTriggered -= OnRewardTriggered;
    }

    public void StopLevel() {
        cueController.HideAll();
        RewardArea.OnRewardTriggered -= OnRewardTriggered;
        StopTrialTimer();
        StopAllCoroutines();
        FadeCanvas.fadeCanvas.AutoFadeOut();
    }

    private void Start() {
        rewards = RewardArea.GetAllRewardsFromScene();

        //cache session
        SessionInfo.GetSessionInfo(out Session session);
        this.session = session;

        //disable robot movement
        robotMovement.SetMovementActive(false);

        // +1 since trailCounter is starts from 0
        SessionStatusDisplay.DisplayTrialNumber(trialCounter + 1);

        Setup();// run any custom code from inherited members.
    }

    public void StartSession() {
        StartCoroutine(FadeInAndStartSession());
    }

    private IEnumerator FadeInAndStartSession() {
        onSessionTrigger.Invoke(SessionTrigger.ExperimentVersionTrigger, GameController.versionNum);

        robotMovement.MoveToWaypoint(startWaypoint);

        //fade in and wait for fadein to complete
        yield return FadeCanvas.fadeCanvas.FadeToScreen();

        // start the first trial.
        StartCoroutine(GoNextTask(true));// first task always true
    }

    /// <summary>
    /// Returns the next Target for the subject.
    /// 
    /// Override this method to customize the next target.
    /// </summary>
    /// <param name="currentTarget">Index of the current target</param>
    /// <returns>Index of the next target</returns>
    protected virtual int GetNextTarget(int currentTarget, RewardArea[] rewards) {
        if (rewards.Length == 2) {
            return 1 - currentTarget; //returns 0 or 1
        }

        // minimally inclusive, maximally exclusive, therefore you will get random numbers between 0 to the number of rewards
        int nextTarget = Random.Range(0, rewards.Length);

        // retries if the random target number generated is the same as the current target number
        while (rewards.Length != 1 && nextTarget == currentTarget) {
            nextTarget = Random.Range(0, rewards.Length);
        }

        return nextTarget;
    }

    /// <summary>
    /// Method to decide the next target.
    /// </summary>
    /// <param name="currentTaskCleared">flag if the curent task is cleared</param>
    private IEnumerator GoNextTask(bool currentTaskCleared) {
        //restart trial unless indicated
        if (!currentTaskCleared && restartOnTaskFail) {
            Console.Write("Restart Trial");
            //teleport back to start

            StartCoroutine(StartTask()); // show cue for same target
            yield break;
        }

        //check if all trials completed
        if (trialCounter >= session.numTrials) {
            onSessionTrigger.Invoke(SessionTrigger.TrialEndedTrigger, targetIndex);
            StartCoroutine(FadeOutBeforeLevelEnd());
            yield break;
        }

        // check if a trial is considered cleared.
        if (IsTrialCompleteCondition(currentTaskCleared)) {
            trialCounter++; // increment if a trial is completed
            SessionStatusDisplay.DisplayTrialNumber(trialCounter);

            // checks if should pause else continue.
            if (isPaused) {
                Console.Write("ExperimentPausedforrr");
            }
            yield return waitIfPaused;

            // execute intertrial only it is not the first trial
            if (firstTask) {
                firstTask = false;
            }
            else {
                onSessionTrigger.Invoke(SessionTrigger.TrialEndedTrigger, targetIndex);
                yield return InterTrial(); //wait for interTrial to complete.
            }
        }

        // prepare next 
        targetIndex = GetNextTarget(targetIndex, rewards);

        cueController.SetTargetImage(rewards[targetIndex].cueImage);
        StartCoroutine(StartTask());
    }

    protected virtual bool IsTrialCompleteCondition(bool currentTaskCleared) {
        return true; // 1 task per trial
    }

    private IEnumerator StartTask() {
        Debug.Log("StartTask");
        PlayerAudio.instance.PlayStartClip(); // play start sound

        cueController.ShowCue();
        onSessionTrigger.Invoke(SessionTrigger.TrialStartedTrigger, targetIndex);

        yield return cueDisplayDuration;

        cueController.HideCue();
        cueController.ShowHint();

        rewards[targetIndex].IsActivated = true; // enable reward
        rewards[targetIndex].StartBlinking(); // start blinking if target has light
        robotMovement.SetMovementActive(true); // enable robot

        onSessionTrigger.Invoke(SessionTrigger.CueOffsetTrigger, targetIndex);

        StartTrialTimer();
    }

    protected virtual void ProcessReward(RewardArea rewardArea) {
        Console.Write(rewardArea.target.name); // log reward name
    }

    private void OnRewardTriggered(RewardArea rewardArea) {
        //check if triggered reward is the reward we are looking for
        if (!rewardArea.Equals(rewards[targetIndex])) {
            return;
        }

        StopTrialTimer();
        ProcessReward(rewardArea);
        cueController.HideHint(); // remove hint
        robotMovement.SetMovementActive(false); // disable robot movement

        rewardArea.StopBlinking(); // stop blinking if there is light
        rewardArea.IsActivated = false; // disable reward

        StartCoroutine(GoNextTask(true));
    }

    private IEnumerator FadeOutBeforeLevelEnd() {
        //fade out when end
        yield return FadeCanvas.fadeCanvas.AutoFadeOut();
        onSessionFinishEvent.Invoke();
    }

    protected virtual IEnumerator InterTrial() {
        if (resetRobotPositionDuringInterTrial) {
            //fadeout and wait for fade out to finish.
            yield return FadeCanvas.fadeCanvas.AutoFadeOut();
            robotMovement.MoveToWaypoint(startWaypoint);
        }

        //delay for inter trial window
        float countDownTime = Session.getTrailIntermissionDuration() / 1000.0f;

        yield return SessionStatusDisplay.Countdown("InterTrial Countdown", countDownTime);

        if (resetRobotPositionDuringInterTrial) {
            //fade in and wait for fade in to finish
            yield return FadeCanvas.fadeCanvas.FadeToScreen();
        }
    }

    private IEnumerator TrialTimer() {
        // convert to seconds
        float trialTimeLimit = Session.trialTimeLimit / 1000f;
        SessionStatusDisplay.DisplaySessionStatus("Trial Running");

        while (trialTimeLimit > 0) {
            yield return SessionStatusDisplay.Tick(trialTimeLimit, out trialTimeLimit);
        }

        //trigger - timeout
        onSessionTrigger.Invoke(SessionTrigger.TimeoutTrigger, targetIndex);

        //disable robot movement
        robotMovement.SetMovementActive(false);
        cueController.HideHint();
        rewards[targetIndex].StopBlinking(); // disable reward
        rewards[targetIndex].IsActivated = false; // disable reward

        //play audio
        PlayerAudio.instance.PlayErrorClip();

        float timeoutDuration = Session.timeoutDuration / 1000f;

        if (resetRobotPositionDuringInterTrial && restartOnTaskFail) {
            yield return FadeCanvas.fadeCanvas.AutoFadeOut();
            robotMovement.MoveToWaypoint(startWaypoint);
        }

        yield return SessionStatusDisplay.Countdown("Timeout", timeoutDuration);

        if (resetRobotPositionDuringInterTrial && restartOnTaskFail) {
            yield return FadeCanvas.fadeCanvas.FadeToScreen();
        }

        StartCoroutine(GoNextTask(false));
    }

    //helper methods to start Timer
    private void StartTrialTimer() {
        // Stop the existing Timer if any
        StopTrialTimer();

        timeLimitTimer = StartCoroutine(TrialTimer());
    }

    //helper methods to stop Timer
    private void StopTrialTimer() {
        if (timeLimitTimer != null) {
            StopCoroutine(timeLimitTimer);
        }
    }

    //inner classes

    /// <summary>
    /// Broadcasts the SessionTrigger and the current reward index
    /// </summary>
    public class SessionTriggerEvent : UnityEvent<SessionTrigger, int> { }
}
