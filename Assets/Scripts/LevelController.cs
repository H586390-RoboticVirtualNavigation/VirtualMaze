using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour {
    /// <summary>
    /// Triggers when the player enters the reward area
    /// </summary>
    /// <param name="rewardArea">RewardArea of the trigger zone entered</param>
    /// /// <param name="isTarget">If the area the current target</param>
    public delegate void OnEnterTriggerZone(RewardArea rewardArea, bool isTarget);
    public static event OnEnterTriggerZone OnEnteredTriggerZone;

    /// <summary>
    /// Triggers when the player leaves the reward area
    /// </summary>
    /// <param name="rewardArea">RewardArea of the trigger zone entered</param>
    /// <param name="isTarget">If the area the current target</param>
    public delegate void OnExitTriggerZone(RewardArea rewardArea, bool isTarget);
    public static event OnExitTriggerZone OnExitedTriggerZone;

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
    protected int numTrials { get; private set; } = 0;

    /// <summary>
    /// Gameobjects tagged as "RewardArea" in the scene will be populated in here.
    /// </summary>
    public RewardArea[] rewards { get; private set; }

    protected IMazeLogicProvider logicProvider;

    [SerializeField]
    private RobotMovement robotMovement = null;

    [SerializeField]
    private CueController cueController = null;

    [SerializeField]
    private ParallelPort parallelPort = null;

    [SerializeField]
    private RewardsController rewardsCtrl = null;

    // cache waitForUnpause for efficiency
    private WaitUntil waitIfPaused;

    private int targetIndex;
    private bool success = false;

    //Strings
    private const string Format_NoRewardAreaComponentFound = "{0} does not have a RewardAreaComponent";

    private void Awake() {
        waitIfPaused = new WaitUntil(() => !isPaused);

        RewardArea.OnEnteredTriggerZone += OnZoneEnter;
        RewardArea.OnExitedTriggerZone += OnZoneExit;

        //Prepare Eyelink
        EyeLink.Initialize();
        onSessionTrigger.AddListener(EyeLink.OnSessionTrigger);
        onSessionTrigger.AddListener(parallelPort.TryWriteTrigger);
    }

    private void OnDestroy() {
        RewardArea.OnEnteredTriggerZone -= OnZoneEnter;
        RewardArea.OnExitedTriggerZone -= OnZoneExit;
    }

    private void OnZoneExit(RewardArea rewardArea) {
        OnExitedTriggerZone?.Invoke(rewardArea, rewardArea.Equals(rewards[targetIndex]));
    }

    private void OnZoneEnter(RewardArea rewardArea) {
        OnEnteredTriggerZone?.Invoke(rewardArea, rewardArea.Equals(rewards[targetIndex]));
    }

    //stop and reset levelcontroller
    public void StopLevel() {
        logicProvider?.Cleanup(rewards);
        cueController.HideAll();
        RewardArea.OnRewardTriggered -= OnRewardTriggered;
        StopAllCoroutines();
        FadeCanvas.fadeCanvas.AutoFadeOut();
    }

    public IEnumerator StartSession(Session session) {
        //prepare the scene
        AsyncOperation task = SceneManager.LoadSceneAsync(session.MazeScene, LoadSceneMode.Single);
        task.allowSceneActivation = true;
        while (!task.isDone) {
            yield return null;
        }

        rewards = RewardArea.GetAllRewardsFromScene();
        startWaypoint = FindObjectOfType<StartWaypoint>().transform;

        logicProvider = session.MazeLogic;
        numTrials = session.numTrials;

        logicProvider.Setup(rewards);

        //disable robot movement
        robotMovement.SetMovementActive(false);
        StartCoroutine(MainLoop());
    }

    IEnumerator MainLoop() {
        int trialCounter = 0;
        targetIndex = MazeLogic.NullRewardIndex;//reset targetindex for MazeLogic

        yield return waitIfPaused;
        yield return FadeInAndStartSession();

        PrepareNextTask(true);// first task always true

        while (trialCounter < numTrials) {
            // +1 since trailCounter is starts from 0
            SessionStatusDisplay.DisplayTrialNumber(trialCounter + 1);
            if (logicProvider.ShowCue(targetIndex)) {
                yield return ShowCues();
            }
            else {
                cueController.ShowHint();
            }

            robotMovement.SetMovementActive(true); // enable robot
            yield return TrialTimer();

            // disable reward
            rewards[targetIndex].StopBlinking();
            rewards[targetIndex].IsActivated = false;

            if (!success) {
                targetIndex = MazeLogic.NullRewardIndex;//reset targetindex for MazeLogic

                if (resetRobotPositionDuringInterTrial) {
                    yield return FadeCanvas.fadeCanvas.AutoFadeOut();
                }

                cueController.HideHint();

                float timeoutDuration = Session.timeoutDuration / 1000f;
                yield return SessionStatusDisplay.Countdown("Timeout", timeoutDuration);

                if (trialCounter < numTrials) {
                    yield return InterTrial();
                }

                if (!restartOnTaskFail) {
                    trialCounter++;
                }

                yield return PauseIfRequired();
            }
            else if (logicProvider.IsTrialCompleteAfterCurrentTask(success)) {
                cueController.HideHint();
                rewardsCtrl.Reward();
                trialCounter++;
                targetIndex = MazeLogic.NullRewardIndex;//reset targetindex for MazeLogic

                yield return PauseIfRequired();

                if (trialCounter < numTrials) {
                    yield return InterTrial();
                }
            }

            PrepareNextTask(success); // continue with next task or reward.

            success = false; //reset the success
        }


        yield return FadeCanvas.fadeCanvas.AutoFadeOut();

        //double check
        while (FadeCanvas.fadeCanvas.isTransiting) {
            yield return null;
        }
        StopLevel();
        onSessionFinishEvent.Invoke();
    }

    private IEnumerator FadeInAndStartSession() {
        onSessionTrigger.Invoke(SessionTrigger.ExperimentVersionTrigger, GameController.versionNum);

        robotMovement.MoveToWaypoint(startWaypoint);

        //fade in and wait for fadein to complete
        yield return FadeCanvas.fadeCanvas.FadeToScreen();
    }

    /// <summary>
    /// Method to decide the next target.
    /// </summary>
    /// <param name="currentTaskSuccess">flag if the curent task is successful</param>
    private void PrepareNextTask(bool currentTaskSuccess) {
        // prepare next 
        targetIndex = logicProvider.GetNextTarget(targetIndex, rewards);
        cueController.SetTargetImage(logicProvider.GetTargetImage(rewards, targetIndex));

        rewards[targetIndex].IsActivated = true; // enable reward
        rewards[targetIndex].StartBlinking(); // start blinking if target has light
        RewardArea.OnRewardTriggered += OnRewardTriggered;

    }

    private IEnumerator ShowCues() {
        Debug.Log("showCues");
        PlayerAudio.instance.PlayStartClip(); // play start sound

        cueController.ShowCue();
        onSessionTrigger.Invoke(SessionTrigger.TrialStartedTrigger, targetIndex);

        yield return new WaitForSecondsRealtime(1f);

        cueController.HideCue();
        cueController.ShowHint();

        onSessionTrigger.Invoke(SessionTrigger.CueOffsetTrigger, targetIndex);
    }

    private void OnRewardTriggered(RewardArea rewardArea) {
        //check if triggered reward is the reward we are looking for
        if (!rewardArea.Equals(rewards[targetIndex])) {
            return;
        }
        //temporarily disable listener as occasionally this will be triggered twice.
        RewardArea.OnRewardTriggered -= OnRewardTriggered;
        logicProvider.ProcessReward(rewardArea);

        success = true;
    }

    protected virtual IEnumerator InterTrial() {
        if (resetRobotPositionDuringInterTrial) {
            //fadeout and wait for fade out to finish.
            yield return FadeCanvas.fadeCanvas.AutoFadeOut();
            robotMovement.MoveToWaypoint(startWaypoint);
        }

        yield return PauseIfRequired();

        //delay for inter trial window
        float countDownTime = Session.getTrailIntermissionDuration() / 1000.0f;

        yield return SessionStatusDisplay.Countdown("InterTrial Countdown", countDownTime);


        //fade in and wait for fade in to finish
        yield return FadeCanvas.fadeCanvas.FadeToScreen();

    }

    private IEnumerator PauseIfRequired() {
        if (isPaused) {
            Console.Write("ExperimentPaused");
        }
        yield return waitIfPaused;
    }

    private IEnumerator TrialTimer() {
        // convert to seconds
        float trialTimeLimit = Session.trialTimeLimit / 1000f;
        SessionStatusDisplay.DisplaySessionStatus("Trial Running");

        while (trialTimeLimit > 0 && !success) {
            yield return SessionStatusDisplay.Tick(trialTimeLimit, out trialTimeLimit);
        }
        RewardArea.OnRewardTriggered -= OnRewardTriggered;

        //disable robot movement
        robotMovement.SetMovementActive(false);

        if (success) {
            onSessionTrigger.Invoke(SessionTrigger.TrialEndedTrigger, targetIndex);
        }
        else {
            onSessionTrigger.Invoke(SessionTrigger.TimeoutTrigger, targetIndex);
            PlayerAudio.instance.PlayErrorClip(); //play audio
        }
    }

    //inner classes
    /// <summary>
    /// Broadcasts the SessionTrigger and the current reward index
    /// </summary>
    public class SessionTriggerEvent : UnityEvent<SessionTrigger, int> { }
}
