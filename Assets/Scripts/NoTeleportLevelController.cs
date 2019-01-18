using UnityEngine;
using System.Collections;

public class NoTeleportLevelController : BasicLevelController
{

    override protected void OnRewardTriggered(RewardArea rewardArea)
    {
        Reward entered = Reward.rewardTriggered;

        if (entered.enableReward)
        {
            //reward
            if (entered.mainReward && !rewards[lastValve].Equals(entered))
            {
                EventManager.TriggerEvent("Reward");

                //trigger = true;
                //triggerValue = 2;
                onSessionTrigger.Invoke(SessionTrigger.TrialStartedTrigger, targetIndex);

                lastValve = thisRewardIndex;
                PlayerAudio.instance.PlayRewardClip();
            }

            // enable the next reward and disable all other rewards
            for (int i = 0; i < rewards.Length; i++)
            {
                // syy: legacy method to control blinking of BlinkReward(have not implemented properly)
                // in RewardArea
                //rewards[i].enableReward = false;  

                // disable all rewards
                rewards[i].SetActive(false);
                if (rewards[i].Equals(entered))
                {
                    thisRewardIndex = i;
                    Debug.Log("reward is " + i);
                }
            }
            int nextRewardIndex = (thisRewardIndex + 1);

            //session ends

            if (nextRewardIndex > rewards.Length - 1)
            {
                //increment trial
                Debug.Log(totalTrialCounter);

                //disable robot movement
                robotMovement.enabled = false;

                nextRewardIndex = 0;

                //new trial
                StartCoroutine("InterTrial");
            }


            if (totalTrialCounter >= numTrials)
            {
                StopCoroutine("Timeout");

                //disable robot movement
                robotMovement.enabled = false;

                StartCoroutine("FadeOutBeforeLevelEnd");
            }

            // enable next
            rewards[nextRewardIndex].SetActive(true);

            // syy: legacy method to control blinking of BlinkReward(have not implemented properly)
            // in RewardArea
            //rewards[nextRewardIndex].enableReward = true;

            Debug.Log(nextRewardIndex);
        }
        else if (!entered.enableReward)
        {
            Debug.Log("wrong reward");
            PlayerAudio.instance.PlayErrorClip();
            if (rewards[thisRewardIndex - 1].Equals(entered))
            {
                // syy: legacy method to control blinking of BlinkReward(have not implemented properly)
                // in RewardArea
                //rewards[thisRewardIndex].enableReward = true;
                //rewards[thisRewardIndex + 1].enableReward = false;
            }
        }
    }

    override protected IEnumerator InterTrial()
    {

        //delay for inter trial window
        float countDownTime = (float)GuiController.interTrialTime / 1000.0f;
        while (countDownTime > 0)
        {
            GuiController.experimentStatus = string.Format("Inter-trial time {0:F2}", countDownTime);
            yield return new WaitForSeconds(0.1f);
            countDownTime -= 0.1f;
        }

        //disable robot movement
        robotMovement.enabled = true;

        //trigger - new trial
        //trigger = true;
        //triggerValue = 1;
        onSessionTrigger.Invoke(SessionTrigger.TrialStartedTrigger, targetIndex);

        //reset elapsed time
        StartCoroutine("Timeout");

        //play audio 
        PlayerAudio.instance.PlayStartClip();

        //update experiment status
        //GuiController.experimentStatus = string.Format("session {0} trial {1}", gameController.sessionCounter, trialCounter);
    }

    override protected IEnumerator Timeout()
    {

        while (true)
        {

            //time out
            if (elapsedTime > trialTimeLimit)
            {

                //trigger - timeout
                //trigger = true;
                //triggerValue = 4;
                onSessionTrigger.Invoke(SessionTrigger.TimeoutTrigger, targetIndex);

                //play audio
                PlayerAudio.instance.PlayErrorClip();

                //disable robot movement
                robotMovement.enabled = false;

                //delay for timeout
                float countDownTime = (float)GuiController.timoutTime / 1000.0f;
                while (countDownTime > 0)
                {
                    GuiController.experimentStatus = string.Format("timeout {0:F2}", countDownTime);
                    yield return new WaitForSeconds(0.1f);
                    countDownTime -= 0.1f;
                }

                //play audio
                PlayerAudio.instance.PlayStartClip();

                //update experiment status, considered the same trial
                //GuiController.experimentStatus = string.Format("session {0} trial {1}",
                //                                                gameController.sessionCounter,
                //                                                trialCounter);

                //trigger - start trial
                //trigger = true;
                //triggerValue = 1;
                onSessionTrigger.Invoke(SessionTrigger.TrialStartedTrigger, targetIndex);

                //disable robot movement
                robotMovement.enabled = true;

            }

            yield return new WaitForSeconds(0.1f);
        }
    }

}
