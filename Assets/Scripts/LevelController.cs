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

    /// <summary>
    /// Triggers when the player stays in the reward area
    /// </summary>
    /// <param name="rewardArea">RewardArea of the trigger zone entered</param>
    /// <param name="isTarget">If the area the current target</param>
    public delegate void InTriggerZone(RewardArea rewardArea, bool isTarget);
    public static event InTriggerZone InTriggerZoneListener;

    /// <summary>
    /// Triggers when the player stays in the reward area
    /// </summary>
    /// <param name="rewardArea">RewardArea of the trigger zone entered</param>
    /// <param name="isTarget">If the area the current target</param>
    public delegate void InRewardProximity(RewardArea rewardArea, bool isTarget);
    public static event InRewardProximity InRewardProximityEvent;

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
    public bool resetRobotPositionDuringInterTrial = true;
    public bool faceRandomDirectionOnStart = false;
    public bool multipleWaypoints = false;
    public bool disableInterSessionBlackout = false;
    public bool resetPositionOnSession = false;
    protected int numTrials { get; private set; } = 0;
    public int trialCounter = 0;

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

    [SerializeField]
    private SessionController sessionController = null;
    
    // cache waitForUnpause for efficiency
    private WaitUntil waitIfPaused;

    public int targetIndex;
    public bool success = false;

    //Strings
    private const string Format_NoRewardAreaComponentFound = "{0} does not have a RewardAreaComponent";

    public static bool sessionStarted { get; private set; } = false;

    private void Awake() {
        waitIfPaused = new WaitUntil(() => !isPaused);

        RewardArea.OnEnteredTriggerZone += OnZoneEnter;
        RewardArea.OnExitedTriggerZone += OnZoneExit;
        RewardArea.InTriggerZoneListener += WhileInTriggerZone;
        RewardArea.OnProximityTriggered += InProximity;

        //Prepare Eyelink
        EyeLink.Initialize();
        onSessionTrigger.AddListener(EyeLink.OnSessionTrigger);
        //kw edit (commented out)
        //onSessionTrigger.AddListener(parallelPort.TryWriteTrigger);
    }

    private void InProximity(RewardArea rewardArea) {
        if (targetIndex != MazeLogic.NullRewardIndex) {
            InRewardProximityEvent?.Invoke(rewardArea, rewardArea.Equals(rewards[targetIndex]));
        }
    }

    private void WhileInTriggerZone(RewardArea rewardArea) {
        if (targetIndex != MazeLogic.NullRewardIndex) {
            InTriggerZoneListener?.Invoke(rewardArea, rewardArea.Equals(rewards[targetIndex]));
        }
    }

    private void OnDestroy() {
        RewardArea.OnEnteredTriggerZone -= OnZoneEnter;
        RewardArea.OnExitedTriggerZone -= OnZoneExit;
        RewardArea.InTriggerZoneListener -= WhileInTriggerZone;
        RewardArea.OnProximityTriggered -= InProximity;
    }

    private void OnZoneExit(RewardArea rewardArea) {
        if (targetIndex != MazeLogic.NullRewardIndex) {
            OnExitedTriggerZone?.Invoke(rewardArea, rewardArea.Equals(rewards[targetIndex]));
        }
    }

    private void OnZoneEnter(RewardArea rewardArea) {
        if (targetIndex != MazeLogic.NullRewardIndex) {
            OnEnteredTriggerZone?.Invoke(rewardArea, rewardArea.Equals(rewards[targetIndex]));
        }
    }

    //stop and reset levelcontroller
    public void StopLevel() {
        numTrials = 0;
        success = false;
        sessionStarted = false;
        trialCounter = 0;
        targetIndex = MazeLogic.NullRewardIndex;
        logicProvider?.Cleanup(rewards);
        cueController.HideAll();
        RewardArea.OnRewardTriggered -= OnRewardTriggered;
        if (!disableInterSessionBlackout || sessionController.index == sessionController.sessions.Count)
        {
            FadeCanvas.fadeCanvas.AutoFadeOut();
        }
        StopAllCoroutines();
    }

    public IEnumerator StartSession(Session session) {
        //prepare the scene
        AsyncOperation task = SceneManager.LoadSceneAsync(session.MazeScene, LoadSceneMode.Single);
        task.allowSceneActivation = true;
        while (!task.isDone) {
            yield return null;
        }

        rewards = RewardArea.GetAllRewardsFromScene();
        //startWaypoint = FindObjectOfType<StartWaypoint>().transform;
        startWaypoint = StartWaypoint.GetWaypoint(multipleWaypoints);

        logicProvider = session.MazeLogic;
        numTrials = session.numTrials;

        logicProvider.Setup(rewards);

        //disable robot movement
        robotMovement.SetMovementActive(false);
        yield return MainLoop();
    }

    IEnumerator MainLoop() {
        targetIndex = MazeLogic.NullRewardIndex;//reset targetindex for MazeLogic

        /* If this is true, it means that this session has multiple tasks and should restart fully */
        bool shouldFullyRestart = false;

        yield return waitIfPaused;
        yield return FadeInAndStartSession();
        sessionStarted = true;

        PrepareNextTask(true); //first task is always true

        while (trialCounter < numTrials) {
            // +1 since trailCounter is starts from 0
            SessionStatusDisplay.DisplayTrialNumber(trialCounter + 1);
            if (logicProvider.ShowCue(targetIndex)) {
                yield return ShowCues();
            }
            else {
                cueController.ShowHint();
            }

            yield return new WaitForSecondsRealtime(0f); // Wait time after hint is shown
            robotMovement.SetMovementActive(true); // enable robot
            yield return TrialTimer();

            // disable reward
            try {
                rewards[targetIndex].StopBlinking();
                rewards[targetIndex].IsActivated = false;
            }
            catch {
                Debug.LogError(targetIndex);
                throw;
            }

            logicProvider.ProcessReward(rewards[targetIndex], success);

            if (!success) {
                if (shouldFullyRestart) {
                    targetIndex = MazeLogic.NullRewardIndex;//reset targetindex for MazeLogic
                }

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
            else {
                if (logicProvider.IsTrialCompleteAfterCurrentTask(success)) {
                    cueController.HideHint();
                    rewardsCtrl.Reward();
                    trialCounter++;

                    if (shouldFullyRestart) {
                        targetIndex = MazeLogic.NullRewardIndex;//reset targetindex for MazeLogic
                    }

                    yield return PauseIfRequired();

                    if (trialCounter < numTrials) {
                        yield return InterTrial();
                    }
                }
                else {
                    /* Success detected but trial has not ended */
                    shouldFullyRestart = true;
                }
            }

            PrepareNextTask((success || !restartOnTaskFail || targetIndex == MazeLogic.NullRewardIndex) && (trialCounter < numTrials)); // continue with next task or reward.

            success = false; //reset the success
        }

        yield return new WaitForSecondsRealtime(2f); // Wait time at the end of trial

        if (!disableInterSessionBlackout)
        {
            yield return FadeCanvas.fadeCanvas.AutoFadeOut();
        }

        //double check
        while (FadeCanvas.fadeCanvas.isTransiting) {
            yield return null;
        }
        StopLevel();
    }

    private IEnumerator FadeInAndStartSession() {
        onSessionTrigger.Invoke(SessionTrigger.ExperimentVersionTrigger, GameController.versionNum);
        int sessionIndex = sessionController.index;
        Debug.Log("Session Index: " + sessionIndex);

        if (resetPositionOnSession || sessionIndex == 1) {
            robotMovement.MoveToWaypoint(startWaypoint);
        }
        if (faceRandomDirectionOnStart) {
            robotMovement.RandomiseDirection(startWaypoint);
        }

        //fade in and wait for fadein to complete
        yield return FadeCanvas.fadeCanvas.AutoFadeIn();

    }

    /// <summary>
    /// Method to decide the next target.
    /// </summary>
    private void PrepareNextTask(bool success) {
        // prepare next 
        if (success) {
            targetIndex = logicProvider.GetNextTarget(targetIndex, rewards);
            cueController.SetTargetImage(logicProvider.GetTargetImage(rewards, targetIndex));
            startWaypoint = StartWaypoint.GetWaypoint(multipleWaypoints);
        }

        rewards[targetIndex].IsActivated = true; // enable reward
        rewards[targetIndex].StartBlinking(); // start blinking if target has light
        RewardArea.OnRewardTriggered += OnRewardTriggered;

    }

    private IEnumerator ShowCues() {
        Debug.Log("showCues");
        PlayerAudio.instance.PlayStartClip(); // play start sound

        // kw edit direct call to parallelport, extracted from listener
        Debug.Log("direct blank cue onset call");
        ParallelPort.TryOut32(parallelPort.portHexAddress, (int)SessionTrigger.TrialStartedTrigger + (int)targetIndex + 1);
        Debug.Log($"PPA \"{parallelPort.portHexAddress}\" in use");
        Debug.Log($"TST \"{(int)SessionTrigger.TrialStartedTrigger}\" in use");
        Debug.Log($"TI \"{(int)targetIndex + 1}\" in use");

        cueController.ShowCue();
        onSessionTrigger.Invoke(SessionTrigger.TrialStartedTrigger, targetIndex);

        yield return new WaitForSecondsRealtime(2f); // Wait time for showing cue before minimising

        cueController.HideCue();
        cueController.ShowHint();

        // kw edit direct call to parallelport, extracted from listener
        Debug.Log("direct blank cue offset call");
        ParallelPort.TryOut32(parallelPort.portHexAddress, (int)SessionTrigger.CueOffsetTrigger + (int)targetIndex + 1);
        Debug.Log($"PPA \"{parallelPort.portHexAddress}\" in use");
        Debug.Log($"TST \"{(int)SessionTrigger.CueOffsetTrigger}\" in use");
        Debug.Log($"TI \"{(int)targetIndex + 1}\" in use");

        onSessionTrigger.Invoke(SessionTrigger.CueOffsetTrigger, targetIndex);
    }

    private void OnRewardTriggered(RewardArea rewardArea) {
        //check if triggered reward is the reward we are looking for
        if (!rewardArea.Equals(rewards[targetIndex])) {
            return;
        }
        //temporarily disable listener as occasionally this will be triggered twice.
        RewardArea.OnRewardTriggered -= OnRewardTriggered;

        success = true;
    }

    protected virtual IEnumerator InterTrial() {
        yield return new WaitForSecondsRealtime(2f); // Wait time in-between trials
        cueController.HideHint();
        if (resetRobotPositionDuringInterTrial) {
            //fadeout and wait for fade out to finish.
            yield return FadeCanvas.fadeCanvas.AutoFadeOut();
            robotMovement.MoveToWaypoint(startWaypoint);
            if (faceRandomDirectionOnStart)
            {
                robotMovement.RandomiseDirection(startWaypoint);
            }
        }

        yield return PauseIfRequired();

        //delay for inter trial window
        float countDownTime = Session.getTrailIntermissionDuration() / 1000.0f;

        yield return SessionStatusDisplay.Countdown("InterTrial Countdown", countDownTime);


        //fade in and wait for fade in to finish
        yield return FadeCanvas.fadeCanvas.AutoFadeIn();

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

            // kw edit direct call to parallelport, extracted from listener
            Debug.Log("direct blank trial end call");
            ParallelPort.TryOut32(parallelPort.portHexAddress, (int)SessionTrigger.TrialEndedTrigger + (int)targetIndex + 1);
            Debug.Log($"PPA \"{parallelPort.portHexAddress}\" in use");
            Debug.Log($"TST \"{(int)SessionTrigger.TrialEndedTrigger}\" in use");
            Debug.Log($"TI \"{(int)targetIndex + 1}\" in use");
        }
        else {
            onSessionTrigger.Invoke(SessionTrigger.TimeoutTrigger, targetIndex);

            // kw edit direct call to parallelport, extracted from listener
            Debug.Log("direct blank trial expire call");
            ParallelPort.TryOut32(parallelPort.portHexAddress, (int)SessionTrigger.TimeoutTrigger + (int)targetIndex + 1);
            Debug.Log($"PPA \"{parallelPort.portHexAddress}\" in use");
            Debug.Log($"TST \"{(int)SessionTrigger.TimeoutTrigger}\" in use");
            Debug.Log($"TI \"{(int)targetIndex + 1}\" in use");

            PlayerAudio.instance.PlayErrorClip(); //play audio
        }
    }

    //inner classes
    /// <summary>
    /// Broadcasts the SessionTrigger and the current reward index
    /// </summary>
    public class SessionTriggerEvent : UnityEvent<SessionTrigger, int> { }
}
