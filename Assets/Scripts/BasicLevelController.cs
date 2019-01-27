using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Runtime.InteropServices; //important for DLLs
using System;
using System.Collections.Generic;
using SREyelink32;


public class BasicLevelController : MonoBehaviour {
    private const string Format_NoRewardAreaComponentFound = "{0} does not have a RewardAreaComponent";
    
    [DllImport("eyelink_core64")]
    private static extern int eyemsg_printf(string message);

    [DllImport("eyelink_core64")]
    private static extern int eyelink_is_connected();

    [DllImport("eyelink_core64")]
    private static extern int current_time();

    [DllImport("eyelink_core64")]
    private static extern int open_eyelink_connection(int mode);

    [DllImport("eyelink_core64")]
    private static extern void close_eyelink_connection();

    [DllImport("eyelink_core64")]
    private static extern int eyelink_broadcast_open();

    protected int numTrials { get; private set; }
    protected int trialCounter { get; private set; } = 0;
    protected float trialTimeLimit { get; private set; }
    protected float timeoutDuration { get; private set; }
    protected int totalTrialCounter { get; private set; }
    protected float elapsedTime { get; private set; }
    protected int triggerValue;

    protected int thisRewardIndex;
    protected int nextRewardIndex;

    protected int targetIndex;

    protected int lastValve;

    private int[] order;

    // Broadcasts when the session is finshed.
    public UnityEvent onSessionFinishEvent = new UnityEvent();

    // Broadcasts when any sessionTriggers happens.
    public SessionTriggerEvent onSessionTrigger = new SessionTriggerEvent();

    //reference to coroutine to properly stop it.
    private Coroutine timeoutTimer;

    //These are only available when ExperiementController starts a new session/level
    protected GameObject robot;
    protected ParallelPort parallelPort;
    protected RobotMovement robotMovement;
    protected Fading fade;
    protected CueController cueController;

    /// <summary>
    /// Gameobjects tagged as "RewardArea" in the scene will be populated in here.
    /// </summary>
    protected RewardArea[] rewards;

    //drag and drop from Unity Editor
    public Transform startWaypoint;

    void Awake() {
        // FindObjectOfType is computationally expensive, look for other ways to do this
        parallelPort = FindObjectOfType<ParallelPort>();
        fade = FindObjectOfType<Fading>();

        robot = GameObject.FindGameObjectWithTag(Tags.Player);
        robotMovement = robot.GetComponent<RobotMovement>();
        cueController = robot.GetComponentInChildren<CueController>();
    }

    private void OnEnable() {
        RewardArea.OnRewardTriggered += OnRewardTriggered;
    }

    private void OnDisable() {
        RewardArea.OnRewardTriggered -= OnRewardTriggered;
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

    void Start() {
        GetAllRewardsFromScene();

        //get parameters for this session
        SessionInfo.GetSessionInfo(
            out int trialTimeLimit,
            out Session session,
            out float timeoutDuration
            );


        //convert to seconds
        this.trialTimeLimit = trialTimeLimit / 1000f;
        this.timeoutDuration = timeoutDuration / 1000f;

        //set numTrials
        numTrials = session.numTrial;

        //disable robot movement
        robotMovement.SetMovementActive(false);
        
        //Prepare BasicLevelController
        totalTrialCounter = 1; // start with 1st trial	
        lastValve = 1000;

        //rewardCount = 0; // start reward counter
        //round = 0; // round number, increases by 1 each time all rewards are collected once
        try {
            //initilise the dll
            open_eyelink_connection(-1);
            //listen in eyelink connection. See eyelink documentation.
            eyelink_broadcast_open();
        }
        catch (DllNotFoundException e) {
            GuiController.experimentStatus = e.ToString();
            Debug.Log(e.ToString());
        }

        StartCoroutine(FadeInAndStart());
    }

    void TryEyemsg_Printf(String msg) {
        try {
            eyemsg_printf(msg);
        }
        catch (DllNotFoundException e) {
            GuiController.experimentStatus = e.ToString();
            Debug.LogException(e);
        }
    }

    int TryGetEyelinkConnectedStatus() {
        int result = 0;
        try {
            result = eyelink_is_connected();
        }
        catch (DllNotFoundException e) {
            GuiController.experimentStatus = e.ToString();
            Debug.LogException(e);
        }
        return result;
    }

    void Update() {

        //increment elapsedTime for Timeout
        elapsedTime += Time.deltaTime;

        //send trigger to parallel port
        if (false) {//code block to be moved. keep here as reference for now
            //used to enter this if block after a trigger happened.
            int EL = TryGetEyelinkConnectedStatus();
            Debug.Log("EL:" + EL);

            if (EL == 2) {
                if (triggerValue < 20) {
                    TryEyemsg_Printf("Start Trial " + triggerValue);
                    //Debug.Log(current_time());
                }
                else if (triggerValue > 20 && triggerValue < 30) {
                    TryEyemsg_Printf("Cue Offset " + triggerValue);
                }
                else if (triggerValue > 30 && triggerValue < 40) {
                    TryEyemsg_Printf("End Trial " + triggerValue);
                    //close_eyelink_connection();
                }
                else if (triggerValue > 40 && triggerValue < 50) {
                    TryEyemsg_Printf("Timeout " + triggerValue);
                    //close_eyelink_connection();
                }
                else if (triggerValue > 80) {
                    TryEyemsg_Printf("Trigger Version " + triggerValue);
                    //close_eyelink_connection();
                }
            }

            parallelPort.WriteTrigger(triggerValue);
            //ParallelPort.TryOut32(GameController.instance.parallelPortAddr, triggerValue); // uncomment lines (124 and 139) to send triggers to Ripple

            // send parallel port
            //if (GameController.instance.parallelPortAddr != -1) {
            //	ParallelPort.Out32 (GameController.instance.parallelPortAddr, triggerValue);	
            //	ParallelPort.Out32 (GameController.instance.parallelPortAddr, 0);	
            //}

            //ParallelPort.TryOut32(GameController.instance.parallelPortAddr, 0); // clear parallel port
            parallelPort.WriteTrigger(0);

        }
    }

    void GetNextTarget() {
        //int nextTarget;
        //nextTarget = rewardCount - rewards.Length*round;
        //thisTarget = order[nextTarget];

        int nextTarget;
        // minimally inclusive, maximally exclusive, therefore you will get random numbers between 0 to the number of rewards
        nextTarget = UnityEngine.Random.Range(0, rewards.Length);
        while (nextTarget == targetIndex) // retries if the random target number generated is the same as the current target number
        {
            nextTarget = UnityEngine.Random.Range(0, rewards.Length);
        }
        targetIndex = nextTarget;
        //increment number of trials started.
        trialCounter++;
        cueController.SetHint(rewards[targetIndex].cueImage);
    }

    IEnumerator ShowHint() {
        yield return new WaitForSeconds(2); // ITI
        PlayerAudio.instance.PlayStartClip(); // play start sound

        cueController.ShowCue();
        //showcue1 = true; // show central cue

        onSessionTrigger.Invoke(SessionTrigger.CueShownTrigger, targetIndex);

        yield return new WaitForSeconds(1); // duration to present central cue
        cueController.HideCue();

        rewards[targetIndex].SetActive(true); // enable reward
        robotMovement.SetMovementActive(true); // enable robot

        cueController.ShowHint();

        onSessionTrigger.Invoke(SessionTrigger.TrialStartedTrigger, targetIndex);

        StartTimeoutTimer();
    }

    //void Shuffle() // Create randomly shuffled array 
    //{
    //    order = new int[rewards.Length];
    //    for (int i = 0; i < rewards.Length; i++) // fill array with reward indices
    //    {
    //        order[i] = i;
    //    }
    //    for (var j = order.Length - 1; j > 0; j--) // shuffle array
    //    {
    //        var r = UnityEngine.Random.Range(0, j);
    //        var tmp = order[j];
    //        order[j] = order[r];
    //        order[r] = tmp;
    //    }
    //    Debug.Log("random order: " + order[0] + order[1] + order[2] + order[3]);
    //}

    virtual protected void OnRewardTriggered(RewardArea rewardArea) {
        //check if triggered reward is the reward we are looking for
        if (!rewardArea.Equals(rewards[targetIndex])) {
            return;
        }

        cueController.HideHint(); // remove hint

        //Reward entered = Reward.rewardTriggered;
        Debug.Log(rewardArea.target.name); // log reward name
        robotMovement.SetMovementActive(false); // disable robot movement
        rewardArea.SetActive(false); // disable reward

        //increment total trial counter
        totalTrialCounter++;

        onSessionTrigger.Invoke(SessionTrigger.TrialEndedTrigger, targetIndex);

        StopTimeoutTimer();

        //PlayerAudio.instance.PlayStartClip();

        //if (entered.enableReward)
        //{


        //    // enable the next reward and disable all other rewards
        //    for (int i = 0; i < rewards.Length; i++)
        //    {
        //        rewards[i].enableReward = false;    // disable all rewards
        //        rewards[i].gameObject.SetActive(false);
        //        if (rewards[i].Equals(entered))
        //        {
        //            thisRewardIndex = i;
        //            Debug.Log("reward is " + i);
        //        }
        //    }
        //    nextRewardIndex = (thisRewardIndex + 1);
        //
        //    //reward
        //
        //    if (entered.mainReward) {
        //        if (lastValve == 1000)
        //        {
        //            EventManager.TriggerEvent("Reward");
        //            trigger = true;
        //            triggerValue = 2;
        //            lastValve = thisRewardIndex;
        //            PlayerAudio.instance.PlayRewardClip();
        //        }
        //        else if (!rewards[lastValve].Equals(entered))
        //        {
        //            EventManager.TriggerEvent("Reward");
        //            trigger = true;
        //            triggerValue = 2;
        //            lastValve = thisRewardIndex;
        //            PlayerAudio.instance.PlayRewardClip();
        //        }
        //            }

        //reshuffle reward order after all rewards have been collected once
        //if (rewardCount % rewards.Length == 0)
        //{
        //    round = round + 1;
        //    Shuffle();
        //}

        //re-enable all rewards after 4th reward is obtained
        //if (rewardCount % 4 == 0)
        //{
        //    rewards[0].enableReward = true;
        //    rewards[1].enableReward = true;
        //    rewards[2].enableReward = true;
        //    rewards[3].enableReward = true;
        //}

        //session ends

        //if (nextRewardIndex > rewards.Length - 1)
        if (trialCounter >= numTrials) //end session once user-specified number of trials is reached
        {
            //increment trial
            //trialCounter++;
            //Debug.Log("trial = " + trialCounter);

            //disable robot movement
            robotMovement.SetMovementActive(false);

            //new trial
            onSessionTrigger.Invoke(SessionTrigger.TrialEndedTrigger, targetIndex);

            StartCoroutine("FadeOutBeforeLevelEnd");
        }

        else if (trialCounter < numTrials) {
            GetNextTarget(); // get next random target
            StartCoroutine(ShowHint()); // show cue for next target
        }

        //Debug.Log(numTrials);
        //if (trialCounter > numTrials)
        //{
        //    StopCoroutine("Timeout");

        //disable robot movement
        //    robotMovement.SetMovementActive(false);

        //    StartCoroutine("FadeOutBeforeLevelEnd");
        //}

        //    // enable next
        //    if (nextRewardIndex != thisRewardIndex)
        //    {
        //        Debug.Log("next index " + nextRewardIndex);
        //        rewards[nextRewardIndex].gameObject.SetActive(true);
        //        rewards[nextRewardIndex].enableReward = true;
        //        Debug.Log("next" + nextRewardIndex);
        //    }
        //}
        //else if (!entered.enableReward)
        //{
        //    Debug.Log("wrong reward");
        //    PlayerAudio.instance.PlayErrorClip();
        //    if (rewards[thisRewardIndex - 1].Equals(entered))
        //    {
        //        rewards[thisRewardIndex].enableReward = true;
        //        rewards[thisRewardIndex + 1].enableReward = false;
        //    }
        //}
    }

    IEnumerator FadeInAndStart() {

        //send pport trigger
        onSessionTrigger.Invoke(SessionTrigger.ExperimentVersionTrigger, GameController.versionNum);

        //go to start
        robotMovement.MoveToWaypoint(startWaypoint);
        //EventManager.TriggerEvent ("Teleported To Start");

        //fade in and wait for fadein to complete
        yield return fade.FadeIn();

        //enable robot movement
        //robotMovement.SetMovementActive(true);

        //trigger - start trial
        //trigger = true;
        //triggerValue = 1;

        //play start clip
        //PlayerAudio.instance.PlayStartClip ();

        //StartCoroutine ("Timeout");
        lastValve = 1000;


        //Shuffle();
        GetNextTarget();
        StartCoroutine(ShowHint()); // show cue for first target
    }


    IEnumerator FadeOutBeforeLevelEnd() {
        //fade out when end
        yield return fade.FadeOut();

        onSessionFinishEvent.Invoke();
    }

    virtual protected IEnumerator InterTrial() {
        StopTimeoutTimer();

        //fadeout and wait for fade out to finish.
        yield return fade.FadeOut();

        //delay for inter trial window
        float countDownTime = (float)GuiController.interTrialTime / 1000.0f;
        while (countDownTime > 0) {
            GuiController.experimentStatus = string.Format("Inter-trial time {0:F2}", countDownTime);
            yield return new WaitForSeconds(0.1f);
            countDownTime -= 0.1f;
        }

        //rewards[0].gameObject.SetActive(true);
        //rewards[0].enableReward = true;

        lastValve = 1000;

        //teleport back to start
        robotMovement.MoveToWaypoint(startWaypoint);
        //EventManager.TriggerEvent ("Teleported To Start");

        //fade in and wait for fade in to finish
        yield return fade.FadeIn();

        //enable robot movement
        //robotMovement.SetMovementActive(true);

        //trigger - new trial
        //trigger = true;
        //triggerValue = 1;


        //reset elapsed time
        //elapsedTime = 0;
        //StartCoroutine ("Timeout");

        //play audio
        //PlayerAudio.instance.PlayStartClip ();

        //GetNextTarget();
        StartCoroutine(ShowHint()); // show cue for first target
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    virtual protected IEnumerator Timeout() {
        //runs every 0.1 sec
        while (true) {

            //if (elapsedTime > completionTime)
            //{
            //    robotMovement.SetMovementActive(false);
            //    PlayerAudio.instance.PlayErrorClip();
            //    StartCoroutine("FadeOutBeforeLevelEnd");
            //}

            //time out
            if (trialTimeLimit > 0) {
                if (elapsedTime > trialTimeLimit) {
                    Debug.Log("timeout");

                    //trigger - timeout
                    onSessionTrigger.Invoke(SessionTrigger.TimeoutTrigger, targetIndex);

                    //play audio
                    PlayerAudio.instance.PlayErrorClip();

                    //disable robot movement
                    robotMovement.SetMovementActive(false);
                    cueController.HideHint();
                    rewards[targetIndex].SetActive(false); // disable reward

                    //                    // fade out
                    //                    fade.FadeOut();
                    //                    while (fade.fadeOutDone == false)
                    //                    {
                    //                        yield return new WaitForSeconds(0.05f);
                    //                    }

                    totalTrialCounter++;
                    yield return new WaitForSeconds(timeoutDuration);

                    StartCoroutine(ShowHint()); // show cue for next target
                    yield break;//stops the coroutine


                    //Debug.Log("trial:" + trialCounter);
                    //if (trialCounter > numTrials)
                    //{
                    //      StopTimeoutTimer();

                    //disable robot movement
                    //robotMovement.SetMovementActive(false);

                    //    StartCoroutine("FadeOutBeforeLevelEnd");
                    //}
                    //else
                    //{
                    //    //disable robot movement
                    //      robotMovement.SetMovementActive(false);
                    //
                    //    nextRewardIndex = 0;
                    //
                    //    StartCoroutine("InterTrial");
                    ////teleport back to start
                    //robotMovement.MoveToWaypoint(startWaypoint);
                    //EventManager.TriggerEvent ("Teleported To Start");

                    ////delay for timeout
                    //float countDownTime = (float)GuiController.timoutTime / 1000.0f;
                    //while (countDownTime > 0)
                    //{
                    //    GuiController.experimentStatus = string.Format("timeout {0:F2}", countDownTime);
                    //    yield return new WaitForSeconds(0.1f);
                    //    countDownTime -= 0.1f;
                    //}

                    //rewards[0].gameObject.SetActive(true);
                    //rewards[0].enableReward = true;

                    //// fade in
                    //Debug.Log("fade in");
                    //fade.FadeIn();
                    //while (fade.fadeInDone == false)
                    //{
                    //    yield return new WaitForSeconds(0.05f);
                    //}

                    ////play audio
                    //PlayerAudio.instance.PlayStartClip();

                    ////update experiment status, considered the same trial
                    //GuiController.experimentStatus = string.Format("session {0} trial {1}",
                    //                                                gameController.sessionCounter,
                    //                                                trialCounter);

                    ////trigger - start trial
                    //trigger = true;
                    //triggerValue = 1;

                    ////disable robot movement
                    //robotMovement.SetMovementActive(true);

                    ////reset elapsed time
                    //elapsedTime = 0;

                    //inTrial = true;
                    //}
                }

            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    //helper methods to start Timer
    private void StartTimeoutTimer() {
        StopTimeoutTimer();
        //reset elapsed time
        elapsedTime = 0;
        timeoutTimer = StartCoroutine(Timeout());
    }

    //helper methods to stop Timer
    private void StopTimeoutTimer() {
        if (timeoutTimer != null) {
            StopCoroutine(timeoutTimer);
        }
    }

    //inner classes

    /// <summary>
    /// Broadcasts the SessionTrigger and the current reward index
    /// </summary>
    public class SessionTriggerEvent : UnityEvent<SessionTrigger, int> { }
}
