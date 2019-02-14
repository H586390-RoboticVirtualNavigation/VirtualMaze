using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
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
    protected bool restartOnTaskFail = true;
    protected bool resetRobotPositionDuringInterTrial = false;
    protected bool fadeoutDuringInterTrial = false;
    protected int trialCounter { get; private set; } = 0;

    /// <summary>
    /// Gameobjects tagged as "RewardArea" in the scene will be populated in here.
    /// </summary>
    protected RewardArea[] rewards { get; private set; }

    protected Session session { get; private set; }
    protected RobotMovement robotMovement { get; private set; }
    protected CueController cueController { get; private set; }
    protected Fading fade { get; private set; }
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
        fade = FindObjectOfType<Fading>();

        waitIfPaused = new WaitUntil(() => !isPaused);

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
        RewardArea.OnRewardTriggered -= OnRewardTriggered;
        StopTrialTimer();
        StopAllCoroutines();
        fade.FadeOut();
    }

    /// <summary>
    /// Helper method to populate rewards[]
    /// </summary>
    private void GetAllRewardsFromScene() {
        //Find all rewardAreas in scene and populate rewards[].
        GameObject[] objs = GameObject.FindGameObjectsWithTag(Tags.RewardArea);
        List<RewardArea> temp = new List<RewardArea>();
        foreach (GameObject obj in objs) {
            RewardArea area = obj.GetComponent<RewardArea>();
            if (area != null) {
                temp.Add(area);

                // Deactivate all rewards at the start.
                area.SetActive(false);
            }
            else {
                Debug.LogWarning(string.Format(Format_NoRewardAreaComponentFound, obj.name));
            }
        }
        rewards = temp.ToArray();
    }

    private void Start() {
        GetAllRewardsFromScene();

        //cache session
        SessionInfo.GetSessionInfo(out Session session);
        this.session = session;

        //disable robot movement
        robotMovement.SetMovementActive(false);

        //Prepare BasicLevelController
        EyeLink.Initialize();

        StartCoroutine(FadeInAndStartSession());

        Setup();// run any custom code from inherited members.
    }

    private IEnumerator FadeInAndStartSession() {
        onSessionTrigger.Invoke(SessionTrigger.ExperimentVersionTrigger, GameController.versionNum);

        robotMovement.MoveToWaypoint(startWaypoint);

        //fade in and wait for fadein to complete
        yield return fade.FadeIn();

        // start the first trial.
        StartCoroutine(GoNextTask(true)); // first task is always true.
    }

    /// <summary>
    /// Returns the next Target for the subject.
    /// 
    /// Override this method to customize the next target.
    /// </summary>
    /// <param name="currentTarget">Index of the current target</param>
    /// <returns>Index of the next target</returns>
    protected virtual int GetNextTarget(int currentTarget, RewardArea[] rewards) {
        // minimally inclusive, maximally exclusive, therefore you will get random numbers between 0 to the number of rewards
        int nextTarget = Random.Range(0, rewards.Length);

        // retries if the random target number generated is the same as the current target number
        while (nextTarget == currentTarget) {
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
            StartCoroutine(StartTask()); // show cue for same target
            yield break;
        }

        //check if all trials completed
        if (trialCounter >= session.numTrial) {
            StartCoroutine(FadeOutBeforeLevelEnd());
            yield break;
        }

        // check if a trial is considered cleared.
        if (IsTrialCompleteCondition()) {
            // checks if should pause else continue.
            if (isPaused) {
                Console.Write("ExperimentPaused");
            }
            yield return waitIfPaused;

            trialCounter++; // increment if a trial is completed

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
        cueController.SetHint(rewards[targetIndex].cueImage);
        StartCoroutine(StartTask());
    }

    protected virtual bool IsTrialCompleteCondition() {
        return true; // 1 task per trial
    }

    private IEnumerator StartTask() {
        Debug.Log("StartTask");
        PlayerAudio.instance.PlayStartClip(); // play start sound

        cueController.ShowCue();
        onSessionTrigger.Invoke(SessionTrigger.CueShownTrigger, targetIndex);

        yield return cueDisplayDuration;

        cueController.HideCue();
        cueController.ShowHint();

        rewards[targetIndex].SetActive(true); // enable reward
        robotMovement.SetMovementActive(true); // enable robot

        onSessionTrigger.Invoke(SessionTrigger.TrialStartedTrigger, targetIndex);

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
        rewardArea.SetActive(false); // disable reward

        StartCoroutine(GoNextTask(true));
    }

    private IEnumerator FadeOutBeforeLevelEnd() {
        //fade out when end
        yield return fade.FadeOut();
        onSessionFinishEvent.Invoke();
    }

    protected virtual IEnumerator InterTrial() {
        if (fadeoutDuringInterTrial) {
            //fadeout and wait for fade out to finish.
            yield return fade.FadeOut();
        }

        //delay for inter trial window
        float countDownTime = Session.getTrailIntermissionDuration() / 1000.0f;
        while (countDownTime > 0) {
            Console.Write(string.Format("Inter-trial time {0:F2}", countDownTime));
            yield return new WaitForSeconds(0.1f);
            countDownTime -= 0.1f;
        }

        //teleport back to start
        if (resetRobotPositionDuringInterTrial) {
            robotMovement.MoveToWaypoint(startWaypoint);
        }

        if (fadeoutDuringInterTrial) {
            //fade in and wait for fade in to finish
            yield return fade.FadeIn();
        }
    }

    private IEnumerator TrialTimer() {
        // convert to seconds
        float trialTimeLimit = Session.trialTimeLimit / 1000f;

        yield return new WaitForSeconds(trialTimeLimit);

        Console.Write("Trial Timeout");

        //trigger - timeout
        onSessionTrigger.Invoke(SessionTrigger.TimeoutTrigger, targetIndex);

        //play audio
        PlayerAudio.instance.PlayErrorClip();

        //disable robot movement
        robotMovement.SetMovementActive(false);
        cueController.HideHint();
        rewards[targetIndex].SetActive(false); // disable reward

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
