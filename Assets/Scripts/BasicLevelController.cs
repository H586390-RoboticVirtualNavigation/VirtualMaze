using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using System.Runtime.InteropServices; //important for DLLs
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;

public class BasicLevelController : MonoBehaviour {
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

    protected int numTrials;
    protected float completionTime;

    protected int trialCounter;
    protected bool trigger;
    protected int triggerValue;
    protected float elapsedTime;
    protected bool inTrial;
    protected int thisRewardIndex;
    protected int nextRewardIndex;
    protected int thisTarget;

    protected int lastValve;

    private int[] order;
    //private int round;
    public Transform startWaypoint;
    public GameObject[] cues;
    public static UnityEvent onSessionFinishEvent = new UnityEvent();

    //protected bool showcue1;
    //protected bool showcue2;

    //reference to coroutine to properly stop it.
    private Coroutine timeoutTimer;

    //These are only available when ExperiementController starts a new session/level
    protected GameObject robot;
    protected ParallelPort parallelPort;
    protected RobotMovement robotMovement;
    protected Fading fade;
    protected CueController cueController;

    //drag and drop from Unity Editor
    public Reward[] rewards;
    

    void OnEnable() {
        EventManager.StartListening("Entered Reward Area", EnteredReward);
    }

    void OnDisable() {
        EventManager.StopListening("Entered Reward Area", EnteredReward);
    }

    void Awake() {
        parallelPort = FindObjectOfType<ParallelPort>();
        robotMovement = FindObjectOfType<RobotMovement>();
        fade = FindObjectOfType<Fading>();
        robot = robotMovement.gameObject;
        cueController = robot.GetComponentInChildren<CueController>();
    }

    void Start() {

        inTrial = false;

        //disable robot movement
        robotMovement.enabled = false;

        //get completiontime
        completionTime = SessionInfo.trialTimeLimit / 1000.0f;
        //set numTrials
        numTrials = SessionInfo.session.numTrial;

        trialCounter = 1; // start with 1st trial	
        trigger = false;
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

        // Deactivate all rewards at the start
        for (int i = 0; i < rewards.Length; i++) {
            rewards[i].gameObject.SetActive(false);
        }

        //Shuffle();
        GetNextTarget();
        StartCoroutine(ShowHint()); // show cue for first target
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
        if (trigger) {
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
            trigger = false;
            //ParallelPort.TryOut32(GameController.instance.parallelPortAddr, 0); // clear parallel port
            parallelPort.WriteTrigger(0);

        }


        //if (showcue1) {
        //    var clone = Instantiate(cues[thisTarget], robot.transform.position + robot.transform.forward * 0.5f + robot.transform.up * 1.2f, robot.transform.rotation); // present cue (indicate location in space)
        //    clone.transform.localScale = new Vector3(0.5f, 0.3f, 0.005f); // size of central cue
        //    Destroy(clone, 0.02f); // remove cue at update rate so that cue appears to follow camera 
        //}

        //if (showcue2) {
        //    var clone = Instantiate(cues[thisTarget], robot.transform.position + robot.transform.forward * 0.5f + robot.transform.up * 1.46f, robot.transform.rotation); // present cue (indicate location in space)
        //    clone.transform.localScale = new Vector3(0.1f, 0.06f, 0.005f); // size of peripheral cue
        //    Destroy(clone, 0.02f); // remove cue at update rate so that cue appears to follow camera 
        //}
    }

    void GetNextTarget() {
        //int nextTarget;
        //nextTarget = rewardCount - rewards.Length*round;
        //thisTarget = order[nextTarget];

        int nextTarget;
        // minimally inclusive, maximally exclusive, therefore you will get random numbers between 0 to the number of rewards
        nextTarget = UnityEngine.Random.Range(0, cues.Length); 
        while (nextTarget == thisTarget) // retries if the random target number generated is the same as the current target number
        {
            nextTarget = UnityEngine.Random.Range(0, cues.Length);
        }
        thisTarget = nextTarget;
        //testing only will redo this line of code
        //cueController.hintImage = cues[thisTarget].GetComponent<MeshRenderer>().material.shader;
    }

    IEnumerator ShowHint() {
        yield return new WaitForSeconds(2); // ITI
        PlayerAudio.instance.PlayStartClip(); // play start sound

        cueController.ShowCue();
        //showcue1 = true; // show central cue
        trigger = true;
        triggerValue = 11 + thisTarget;

        yield return new WaitForSeconds(1); // duration to present central cue
        cueController.HideCue();


        rewards[thisTarget].gameObject.SetActive(true); // enable reward
        robotMovement.enabled = true; // enable robot

        cueController.ShowHint();
        trigger = true;
        triggerValue = 21 + thisTarget;

        elapsedTime = 0;
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

    virtual protected void EnteredReward() {
        cueController.HideHint(); // remove peripheral cue

        Reward entered = Reward.rewardTriggered;
        Debug.Log(Reward.rewardTriggered); // log reward ID
        robotMovement.enabled = false; // disable robot
        rewards[thisTarget].gameObject.SetActive(false); // disable reward
        //rewardCount++;
        trialCounter++;

        EventManager.TriggerEvent("Reward");
        trigger = true;
        triggerValue = 31 + thisTarget;

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
        //if (rewardCount > Convert.ToInt32(InputRewardNo.inputrewardno)-1) //end session once user-specified number of rewards is reached
        if (trialCounter >= numTrials) //end session once user-specified number of trials is reached
        {
            //increment trial
            //trialCounter++;
            //Debug.Log("trial = " + trialCounter);

            //disable robot movement
            robotMovement.enabled = false;

            nextRewardIndex = 0;

            //new trial
            trigger = true;
            triggerValue = 31 + thisTarget;

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
        //    robotMovement.enabled = false;

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

    void SetPositionToStart() {
        //set robot's position and rotation to start
        Vector3 startpos = robot.transform.position;
        startpos.x = startWaypoint.position.x;
        startpos.z = startWaypoint.position.z;
        robot.transform.position = startpos;

        Quaternion startrot = robot.transform.rotation;
        startrot.y = startWaypoint.rotation.y;
        robot.transform.rotation = startrot;

        //EventManager.TriggerEvent ("Teleported To Start");
    }

    IEnumerator FadeInAndStart() {

        //send pport trigger
        trigger = true;
        triggerValue = 84; // 80 + version number

        //go to start
        SetPositionToStart();

        //fade in
        fade.FadeIn();
        while (fade.fadeInDone == false) {
            yield return new WaitForSeconds(0.05f);
        }

        //enable robot movement
        //robotMovement.enabled = true;

        //trigger - start trial
        //trigger = true;
        //triggerValue = 1;

        //play start clip
        //PlayerAudio.instance.PlayStartClip ();

        //update experiment status
        //GuiController.experimentStatus = string.Format("session {0} trial {1}", gameController.sessionCounter, trialCounter);

        //reset elapsed time
        //elapsedTime = 0;
        //StartCoroutine ("Timeout");
        lastValve = 1000;

        inTrial = true;
    }


    IEnumerator FadeOutBeforeLevelEnd() {

        inTrial = false;


        //fade out when end
        fade.FadeOut();
        while (fade.fadeOutDone == false) {
            yield return new WaitForSeconds(0.05f);
        }

        onSessionFinishEvent.Invoke();
    }

    virtual protected IEnumerator InterTrial() {

        inTrial = false;
        StopTimeoutTimer();

        fade.FadeOut();
        while (fade.fadeOutDone == false) {
            yield return new WaitForSeconds(0.05f);
        }

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
        SetPositionToStart();

        fade.FadeIn();
        while (fade.fadeInDone == false) {
            yield return new WaitForSeconds(0.05f);
        }

        //disable robot movement
        //robotMovement.enabled = true;

        //trigger - new trial
        //trigger = true;
        //triggerValue = 1;


        //reset elapsed time
        //elapsedTime = 0;
        //StartCoroutine ("Timeout");

        //play audio
        //PlayerAudio.instance.PlayStartClip ();

        //update experiment status
        //GuiController.experimentStatus = string.Format("session {0} trial {1}", gameController.sessionCounter, trialCounter);

        inTrial = true;

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
            //    robotMovement.enabled = false;
            //    PlayerAudio.instance.PlayErrorClip();
            //    StartCoroutine("FadeOutBeforeLevelEnd");
            //}

            //time out
            if (completionTime > 0) {
                if (elapsedTime > completionTime) {
                    Debug.Log("timeout");
                    inTrial = false;

                    //trigger - timeout
                    trigger = true;
                    triggerValue = 41 + thisTarget;

                    //play audio
                    PlayerAudio.instance.PlayErrorClip();

                    //disable robot movement
                    robotMovement.enabled = false;
                    cueController.HideHint();
                    rewards[thisTarget].gameObject.SetActive(false); // disable reward

                    //                    // fade out
                    //                    fade.FadeOut();
                    //                    while (fade.fadeOutDone == false)
                    //                    {
                    //                        yield return new WaitForSeconds(0.05f);
                    //                    }



                    trialCounter++;
                    inTrial = false;
                    yield return new WaitForSeconds(SessionInfo.timeoutDuration / 1000f);


                    inTrial = true;
                    StartCoroutine(ShowHint()); // show cue for next target
                    yield break;//stops the coroutine


                    //Debug.Log("trial:" + trialCounter);
                    //if (trialCounter > numTrials)
                    //{
                    //      StopTimeoutTimer();

                    //disable robot movement
                    //    robotMovement.enabled = false;

                    //    StartCoroutine("FadeOutBeforeLevelEnd");
                    //}
                    //else
                    //{
                    //    //disable robot movement
                    //    robotMovement.enabled = false;
                    //
                    //    nextRewardIndex = 0;
                    //
                    //    StartCoroutine("InterTrial");
                    ////teleport back to start
                    //SetPositionToStart();

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
                    //robotMovement.enabled = true;

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
        timeoutTimer = StartCoroutine(Timeout());
    }

    //helper methods to stop Timer
    private void StopTimeoutTimer() {
        if (timeoutTimer != null) {
            StopCoroutine(timeoutTimer);
        }
    }
}
