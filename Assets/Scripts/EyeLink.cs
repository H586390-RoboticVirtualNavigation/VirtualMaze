using System;
using UnityEngine;
#if (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX)
#else
using ELink = SREYELINKLib.EyeLink;
#endif


public static class EyeLink {
#if (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX)
    private static bool openDummy = true;

    public static void TryEyemsg_Printf(String msg) {
        Debug.Log($"Did not send \"{msg}\" to Eyelink");
    }

    public static bool TryGetEyelinkConnectedStatus() {
        return false;
    }

    public static void Initialize() {
        //do nothing
    }

    public static void OnSessionTrigger(SessionTrigger trigger, int triggerValue) {

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

    }
#else

    private static bool openDummy = true;

    public static void TryEyemsg_Printf(String msg) {
        eyelink.sendMessage(msg);
        Debug.Log($"Sent \"{msg}\" to Eyelink");
    }

    public static bool TryGetEyelinkConnectedStatus() {
        if (eyelink != null) {
            return eyelink.isConnected();
        }
        return false;
    }

    public static ELink eyelink;

    public static void Initialize() {
        eyelink = new ELink();
        if (!openDummy) {
            eyelink.open();
        }
        else {
            eyelink.dummyOpen();
        }
    }

    public static void OnSessionTrigger(SessionTrigger trigger, int triggerValue) {
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
        }
        else {
            Debug.LogWarning("Eyelink not connected!");
        }
    }
#endif
}
