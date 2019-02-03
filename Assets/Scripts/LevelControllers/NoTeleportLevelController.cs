using UnityEngine;
using System.Collections;

public class NoTeleportLevelController : BasicLevelController
{

    //override protected void OnRewardTriggered(RewardArea rewardArea)
    //{
    //    Reward entered = Reward.rewardTriggered;

    //    if (entered.enableReward)
    //    {
    //        //reward
    //        if (entered.mainReward && !rewards[lastValve].Equals(entered))
    //        {
    //            EventManager.TriggerEvent("Reward");

    //            //trigger = true;
    //            //triggerValue = 2;
    //            onSessionTrigger.Invoke(SessionTrigger.TrialStartedTrigger, targetIndex);

    //            lastValve = thisRewardIndex;
    //            PlayerAudio.instance.PlayRewardClip();
    //        }

    //        // enable the next reward and disable all other rewards
    //        for (int i = 0; i < rewards.Length; i++)
    //        {
    //            // syy: legacy method to control blinking of BlinkReward(have not implemented properly)
    //            // in RewardArea
    //            //rewards[i].enableReward = false;  

    //            // disable all rewards
    //            rewards[i].SetActive(false);
    //            if (rewards[i].Equals(entered))
    //            {
    //                thisRewardIndex = i;
    //                Debug.Log("reward is " + i);
    //            }
    //        }
    //        int nextRewardIndex = (thisRewardIndex + 1);

    //        //session ends

    //        if (nextRewardIndex > rewards.Length - 1)
    //        {
    //            //increment trial
    //            Debug.Log(totalTrialCounter);

    //            //disable robot movement
    //            robotMovement.enabled = false;

    //            nextRewardIndex = 0;

    //            //new trial
    //            StartCoroutine("InterTrial");
    //        }


    //        if (totalTrialCounter >= numTrials)
    //        {
    //            StopCoroutine("Timeout");

    //            //disable robot movement
    //            robotMovement.enabled = false;

    //            StartCoroutine("FadeOutBeforeLevelEnd");
    //        }

    //        // enable next
    //        rewards[nextRewardIndex].SetActive(true);

    //        // syy: legacy method to control blinking of BlinkReward(have not implemented properly)
    //        // in RewardArea
    //        //rewards[nextRewardIndex].enableReward = true;

    //        Debug.Log(nextRewardIndex);
    //    }
    //    else if (!entered.enableReward)
    //    {
    //        Debug.Log("wrong reward");
    //        PlayerAudio.instance.PlayErrorClip();
    //        if (rewards[thisRewardIndex - 1].Equals(entered))
    //        {
    //            // syy: legacy method to control blinking of BlinkReward(have not implemented properly)
    //            // in RewardArea
    //            //rewards[thisRewardIndex].enableReward = true;
    //            //rewards[thisRewardIndex + 1].enableReward = false;
    //        }
    //    }
    //}
}
