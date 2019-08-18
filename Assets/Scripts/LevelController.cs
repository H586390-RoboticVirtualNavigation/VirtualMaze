using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour {
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

    protected WaitForSecondsRealtime CueDisplayDuration { get; private set; } = new WaitForSecondsRealtime(1f);

    // cache waitForUnpause for efficiency
    private WaitUntil waitIfPaused;

    private int targetIndex;
    private bool success = false;

    //Strings
    private const string Format_NoRewardAreaComponentFound = "{0} does not have a RewardAreaComponent";

    private void Awake() {
        waitIfPaused = new WaitUntil(() => !isPaused);

        //Prepare Eyelink
        EyeLink.Initialize();
        onSessionTrigger.AddListener(EyeLink.OnSessionTrigger);
        DontDestroyOnLoad(this);
    }

    //stop and reset levelcontroller
    public void StopLevel() {
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
        bool firstTrial = true;

        yield return FadeInAndStartSession();

        while (trialCounter < numTrials) {
            // +1 since trailCounter is starts from 0
            SessionStatusDisplay.DisplayTrialNumber(trialCounter + 1);

            // start the first trial.
            if (firstTrial) {
                firstTrial = false;
                PrepareTask(true);// first task always true
            }
            else {
                yield return InterTrial();
                PrepareTask(success);
            }
            yield return ShowCues();
            yield return TrialTimer();

            if (!success) {
                float timeoutDuration = Session.timeoutDuration / 1000f;

                if (resetRobotPositionDuringInterTrial && restartOnTaskFail) {
                    yield return FadeCanvas.fadeCanvas.AutoFadeOut();
                    robotMovement.MoveToWaypoint(startWaypoint);
                }

                yield return SessionStatusDisplay.Countdown("Timeout", timeoutDuration);
            }

            if (logicProvider.IsTrialCompleteAfterReward(success)) {
                success = false; //reset the success
                trialCounter++;
                // checks if should pause else continue.
                if (isPaused) {
                    Console.Write("ExperimentPaused");
                }
                yield return waitIfPaused;
            }
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
        yield return FadeCanvas.fadeCanvas.FadeIn();
    }

    /// <summary>
    /// Method to decide the next target.
    /// </summary>
    /// <param name="currentTaskSuccess">flag if the curent task is successful</param>
    private void PrepareTask(bool currentTaskSuccess) {
        //restart trial unless indicated
        if (!currentTaskSuccess && restartOnTaskFail) {
            Console.Write("Restart Trial");
        }
        else {
            // prepare next 
            targetIndex = logicProvider.GetNextTarget(targetIndex, rewards);
            cueController.SetTargetImage(rewards[targetIndex].cueImage);
        }
    }

    private IEnumerator ShowCues() {
        Debug.Log("showCues");
        PlayerAudio.instance.PlayStartClip(); // play start sound

        cueController.ShowCue();
        onSessionTrigger.Invoke(SessionTrigger.TrialStartedTrigger, targetIndex);

        yield return CueDisplayDuration;

        cueController.HideCue();
        cueController.ShowHint();

        rewards[targetIndex].IsActivated = true; // enable reward
        rewards[targetIndex].StartBlinking(); // start blinking if target has light
        robotMovement.SetMovementActive(true); // enable robot

        onSessionTrigger.Invoke(SessionTrigger.CueOffsetTrigger, targetIndex);

        RewardArea.OnRewardTriggered += OnRewardTriggered;
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

        //delay for inter trial window
        float countDownTime = Session.getTrailIntermissionDuration() / 1000.0f;

        yield return SessionStatusDisplay.Countdown("InterTrial Countdown", countDownTime);

        if (resetRobotPositionDuringInterTrial) {
            //fade in and wait for fade in to finish
            yield return FadeCanvas.fadeCanvas.FadeIn();
        }
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
        cueController.HideHint();

        // disable reward
        rewards[targetIndex].StopBlinking();
        rewards[targetIndex].IsActivated = false;

        if (success) {
            onSessionTrigger.Invoke(SessionTrigger.TrialEndedTrigger, targetIndex);
        }
        else {
            onSessionTrigger.Invoke(SessionTrigger.TimeoutTrigger, targetIndex);
            //play audio
            PlayerAudio.instance.PlayErrorClip();
        }
    }

    //inner classes
    /// <summary>
    /// Broadcasts the SessionTrigger and the current reward index
    /// </summary>
    public class SessionTriggerEvent : UnityEvent<SessionTrigger, int> { }
}
