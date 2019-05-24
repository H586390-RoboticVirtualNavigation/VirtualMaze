using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices; //important for DLLs
using UnityEngine;
using SREYELINKLib;

public class EyeLink {
    private static bool openDummy = false;

    public static void TryEyemsg_Printf(String msg) {
            eyelink.sendMessage(msg);
    }

    public static bool TryGetEyelinkConnectedStatus() {
        if (eyelink != null) {
            return eyelink.isConnected();
        }
        return false;
    }

    public static SREYELINKLib.EyeLink eyelink;

    public static void Initialize() {
        eyelink = new SREYELINKLib.EyeLink();
        if (!openDummy) {
            eyelink.open();
        }
        else {
            eyelink.dummyOpen();
        }
    }

    public static void OnSessionTrigger(SessionTrigger trigger, int triggerValue) {
        bool isconnected = TryGetEyelinkConnectedStatus();
        Debug.Log("EL connected?:" + isconnected);
        if (isconnected) {
            switch (trigger) {
                case SessionTrigger.CueShownTrigger:
                    TryEyemsg_Printf("Cue Offset " + triggerValue);
                    break;
                case SessionTrigger.TrialStartedTrigger:
                    TryEyemsg_Printf("Start Trial " + triggerValue);
                    break;
                case SessionTrigger.TrialEndedTrigger:
                    TryEyemsg_Printf("End Trial " + triggerValue);
                    break;
                case SessionTrigger.TimeoutTrigger:
                    TryEyemsg_Printf("Timeout " + triggerValue);
                    break;
                case SessionTrigger.ExperimentVersionTrigger:
                    TryEyemsg_Printf("Trigger Version " + GameController.versionNum);
                    break;
            }
            #region OriginalCode
            //int EL = TryGetEyelinkConnectedStatus();
            //Debug.Log("EL:" + EL);

            //if (EL == 2) {
            //    if (triggerValue < 20) {
            //        TryEyemsg_Printf("Start Trial " + triggerValue);
            //        //Debug.Log(current_time());
            //    }
            //    else if (triggerValue > 20 && triggerValue < 30) {
            //        TryEyemsg_Printf("Cue Offset " + triggerValue);
            //    }
            //    else if (triggerValue > 30 && triggerValue < 40) {
            //        TryEyemsg_Printf("End Trial " + triggerValue);
            //        //close_eyelink_connection();
            //    }
            //    else if (triggerValue > 40 && triggerValue < 50) {
            //        TryEyemsg_Printf("Timeout " + triggerValue);
            //        //close_eyelink_connection();
            //    }
            //    else if (triggerValue > 80) {
            //        TryEyemsg_Printf("Trigger Version " + triggerValue);
            //        //close_eyelink_connection();
            //    }
            //}
            #endregion
        }
    }
}
