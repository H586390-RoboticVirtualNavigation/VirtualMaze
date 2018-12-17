using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BasicLevelController : MonoBehaviour
{

    protected GameController gameController;
    protected GameObject robot;
    protected RobotMovement robotMovement;
    protected Fading fade;
    protected int numTrials;
    protected int trialCounter;
    protected bool trigger;
    protected int triggerValue;
    protected float elapsedTime;
    protected float completionTime;
    protected bool inTrial;
    protected int thisRewardIndex;
    protected int lastValve;
    public Reward[] rewards;
    public Transform startWaypoint;

    void OnEnable()
    {
        EventManager.StartListening("Entered Reward Area", EnteredReward);
    }

    void OnDisable()
    {
        EventManager.StopListening("Entered Reward Area", EnteredReward);
    }

    void Awake()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        fade = GameObject.Find("FadeCanvas").GetComponent<Fading>();
        robot = GameObject.Find("Robot");
        robotMovement = robot.GetComponent<RobotMovement>();
    }

    void Start()
    {

        inTrial = false;

        //disable robot movement
        robotMovement.enabled = false;

        //get completiontime
        completionTime = (float)GuiController.completionWindowTime / 1000.0f;

        //set numTrials
        numTrials = gameController.numTrials;
        trialCounter = 1;   //start with first trial
        trigger = false;

        StartCoroutine("FadeInAndStart");
    }

    void Update()
    {

        //increment elapsedTime for Timeout
        elapsedTime += Time.deltaTime;

        //save position data
        if (gameController.fs != null)
        {

            if (trigger)
            {

                // send parallel port
                if (GameController.instance.parallelPortAddr != -1)
                {
                    ParallelPort.Out32(GameController.instance.parallelPortAddr, triggerValue);
                    ParallelPort.Out32(GameController.instance.parallelPortAddr, 0);
                }
                gameController.fs.WriteLine("{0} {1:F8} {2:F2} {3:F2} {4:F2}",
                                            triggerValue,
                                            Time.deltaTime,
                                            robot.transform.position.x,
                                            robot.transform.position.z,
                                            robot.transform.eulerAngles.y);
                trigger = false;
            }
            else
            {
                gameController.fs.WriteLine("     {0:F8} {1:F2} {2:F2} {3:F2}",
                                            Time.deltaTime,
                                            robot.transform.position.x,
                                            robot.transform.position.z,
                                            robot.transform.eulerAngles.y);
            }
        }
    }

    virtual protected void EnteredReward()
    {
        Reward entered = Reward.rewardTriggered;
        Debug.Log(rewards.Length);

        if (entered.enableReward)
        {


            // enable the next reward and disable all other rewards
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].enableReward = false;    // disable all rewards
                rewards[i].gameObject.SetActive(false);
                if (rewards[i].Equals(entered))
                {
                    thisRewardIndex = i;
                    Debug.Log("reward is " + i);
                }
            }
            int nextRewardIndex = (thisRewardIndex + 1);

            //reward
            if (entered.mainReward && !rewards[lastValve].Equals(entered))
            {
                EventManager.TriggerEvent("Reward");
                trigger = true;
                triggerValue = 2;
                lastValve = thisRewardIndex;
                PlayerAudio.instance.PlayRewardClip();
            }



            //session ends

            if (nextRewardIndex > rewards.Length)
            {
                //increment trial
                trialCounter++;
                Debug.Log(trialCounter);

                //disable robot movement
                robotMovement.enabled = false;

                nextRewardIndex = 0;

                //new trial
                StartCoroutine("InterTrial");
            }


            if (trialCounter >= numTrials)
            {
                StopCoroutine("Timeout");

                //disable robot movement
                robotMovement.enabled = false;

                StartCoroutine("FadeOutBeforeLevelEnd");
            }

            // enable next
            Debug.Log("next index " + nextRewardIndex);
            rewards[nextRewardIndex].gameObject.SetActive(true);
            rewards[nextRewardIndex].enableReward = true;
            Debug.Log(nextRewardIndex);
        }
        else if (!entered.enableReward)
        {
            Debug.Log("wrong reward");
            PlayerAudio.instance.PlayErrorClip();
            if (rewards[thisRewardIndex - 1].Equals(entered))
            {
                rewards[thisRewardIndex].enableReward = true;
                rewards[thisRewardIndex + 1].enableReward = false;
            }
        }

    }

    void SetPositionToStart()
    {
        //set robot's position and rotation to start
        Vector3 startpos = robot.transform.position;
        startpos.x = startWaypoint.position.x;
        startpos.z = startWaypoint.position.z;
        robot.transform.position = startpos;

        Quaternion startrot = robot.transform.rotation;
        startrot.y = startWaypoint.rotation.y;
        robot.transform.rotation = startrot;

        EventManager.TriggerEvent("Teleported To Start");
    }

    IEnumerator FadeInAndStart()
    {

        //go to start
        SetPositionToStart();

        //fade in
        fade.FadeIn();
        while (fade.fadeInDone == false)
        {
            yield return new WaitForSeconds(0.05f);
        }

        //enable robot movement
        robotMovement.enabled = true;

        //trigger - start trial
        trigger = true;
        triggerValue = 1;

        //play start clip
        PlayerAudio.instance.PlayStartClip();

        //update experiment status
        GuiController.experimentStatus = string.Format("session {0} trial {1}", gameController.sessionCounter, trialCounter);

        //reset elapsed time
        elapsedTime = 0;
        StartCoroutine("Timeout");

        inTrial = true;
    }


    IEnumerator FadeOutBeforeLevelEnd()
    {

        inTrial = false;

        //fade out when end
        fade.FadeOut();
        while (fade.fadeOutDone == false)
        {
            yield return new WaitForSeconds(0.05f);
        }
        EventManager.TriggerEvent("Level Ended");
    }

    virtual protected IEnumerator InterTrial()
    {

        inTrial = false;


        fade.FadeOut();
        while (fade.fadeOutDone == false)
        {
            yield return new WaitForSeconds(0.05f);
        }

        //teleport back to start
        SetPositionToStart();

        //delay for inter trial window
        float countDownTime = (float)GuiController.interTrialTime / 1000.0f;
        while (countDownTime > 0)
        {
            GuiController.experimentStatus = string.Format("Inter-trial time {0:F2}", countDownTime);
            yield return new WaitForSeconds(0.1f);
            countDownTime -= 0.1f;
        }


        fade.FadeIn();
        while (fade.fadeInDone == false)
        {
            yield return new WaitForSeconds(0.05f);
        }


        //disable robot movement
        robotMovement.enabled = true;

        //trigger - new trial
        trigger = true;
        triggerValue = 1;

        //reset elapsed time
        elapsedTime = 0;
        StartCoroutine("Timeout");

        //play audio
        PlayerAudio.instance.PlayStartClip();

        //update experiment status
        GuiController.experimentStatus = string.Format("session {0} trial {1}", gameController.sessionCounter, trialCounter);

        inTrial = true;
    }

    virtual protected IEnumerator Timeout()
    {

        while (true)
        {

            //time out
            if (elapsedTime > completionTime)
            {

                inTrial = false;

                //trigger - timeout
                trigger = true;
                triggerValue = 4;

                //play audio
                PlayerAudio.instance.PlayErrorClip();

                //disable robot movement
                robotMovement.enabled = false;

                // fade out
                fade.FadeOut();
                while (fade.fadeOutDone == false)
                {
                    yield return new WaitForSeconds(0.05f);
                }

                //teleport back to start
                SetPositionToStart();

                //delay for timeout
                float countDownTime = (float)GuiController.timoutTime / 1000.0f;
                while (countDownTime > 0)
                {
                    GuiController.experimentStatus = string.Format("timeout {0:F2}", countDownTime);
                    yield return new WaitForSeconds(0.1f);
                    countDownTime -= 0.1f;
                }

                // fade in
                fade.FadeIn();
                while (fade.fadeInDone == false)
                {
                    yield return new WaitForSeconds(0.05f);
                }

                //play audio
                PlayerAudio.instance.PlayStartClip();

                //update experiment status, considered the same trial
                GuiController.experimentStatus = string.Format("session {0} trial {1}",
                                                                gameController.sessionCounter,
                                                                trialCounter);

                //trigger - start trial
                trigger = true;
                triggerValue = 1;

                //disable robot movement
                robotMovement.enabled = true;

                //reset elapsed time
                elapsedTime = 0;

                inTrial = true;

            }
            else if (inTrial)
            {
                //update experiment status, considered the same trial
                GuiController.experimentStatus = string.Format("session {0} trial {1}\ntimeout in {2:F2}",
                                                                gameController.sessionCounter,
                                                                trialCounter,
                                                                completionTime - elapsedTime);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
