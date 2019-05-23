using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices; //important for DLLs
using UnityEngine;
using SREYELINKLib;

/// <summary>
/// temporary holding area for eyelink commands
/// </summary>
public class EyeLink {

    [DllImport("eyelink_core64")]
    private static extern int eyemsg_printf(string message);

    [DllImport("eyelink_core64")]
    private static extern int eyelink_is_connected();

    [DllImport("eyelink_core64")]
    private static extern int current_time();

    [DllImport("eyelink_core64")]
    private static extern int open_eyelink_connection(int mode);
    public static int OpenEyelinkConnection(int mode) {
        return open_eyelink_connection(mode);
    }

    [DllImport("eyelink_core64")]
    private static extern void close_eyelink_connection();

    [DllImport("eyelink_core64")]
    private static extern int eyelink_broadcast_open();
    public static int EyelinkBroadcastOpen() {
        return eyelink_broadcast_open();
    }

    public void TryEyemsg_Printf(String msg) {
        try {
            eyemsg_printf(msg);
        }
        catch (DllNotFoundException e) {
            GuiController.experimentStatus = e.ToString();
            Debug.LogException(e);
        }
    }

    public int TryGetEyelinkConnectedStatus() {
        //if(eyelink!= null) {
        //    return eyelink.isConnected();
        //}
        //return false;
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

    public static SREYELINKLib.EyeLink eyelink;

    public static void Initialize() {
        eyelink = new SREYELINKLib.EyeLink();

        //try {
        //    //initilise the dll
        //    EyeLink.OpenEyelinkConnection(-1);
        //    //listen in eyelink connection. See eyelink documentation.
        //    EyeLink.EyelinkBroadcastOpen();
        //}
        //catch (DllNotFoundException e) {
        //    GuiController.experimentStatus = e.ToString();
        //    Debug.Log(e.ToString());
        //}
    }

    public void OnSessionTrigger(SessionTrigger trigger, int triggerValue) {
        int EL = TryGetEyelinkConnectedStatus();
        Debug.Log("EL:" + EL);
        if (EL == 2) {
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
            //close_eyelink_connection();

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
