using System;
using UnityEngine;

public class EyeLink {
#if (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX)
    private static bool useLibrary = false;
#else
    private static bool useLibrary = true;
#endif

    private static bool openDummy = true;

    public static void TryEyemsg_Printf(String msg) {
        if (!useLibrary) {
            return;
        }

        eyelink.sendMessage(msg);
        Debug.Log($"Sent \"{msg}\" to Eyelink");
    }

    public static bool TryGetEyelinkConnectedStatus() {
        if (!useLibrary) {
            return false;
        }

        if (eyelink != null) {
            return eyelink.isConnected();
        }
        return false;
    }

    public static SREYELINKLib.EyeLink eyelink;

    public static void Initialize() {
        if (!useLibrary) {
            return;
        }

        eyelink = new SREYELINKLib.EyeLink();
        if (!openDummy) {
            eyelink.open();
        }
        else {
            eyelink.dummyOpen();
        }
    }

    public static void OnSessionTrigger(SessionTrigger trigger, int triggerValue) {
        if (!useLibrary) {
            return;
        }

        bool isconnected = TryGetEyelinkConnectedStatus();

        if (isconnected) {
            int flag = (int)trigger + triggerValue + 1;
            switch (trigger) {
                case SessionTrigger.CueOffsetTrigger:
                    TryEyemsg_Printf($"Cue Offset {flag}");
                    break;
                case SessionTrigger.TrialStartedTrigger:
                    TryEyemsg_Printf($"Start Trial {flag}");
                    break;
                case SessionTrigger.TrialEndedTrigger:
                    TryEyemsg_Printf($"End Trial {flag}");
                    break;
                case SessionTrigger.TimeoutTrigger:
                    TryEyemsg_Printf($"Timeout {flag}");
                    break;
                case SessionTrigger.ExperimentVersionTrigger:
                    TryEyemsg_Printf($"Trigger Version {(int)trigger + GameController.versionNum}");
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
        else {
            Debug.LogWarning("Eyelink not connected!");
        }
    }
}
