using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class ParallelPort : ConfigurableComponent {
    private bool parallelflip = false;
    public int portHexAddress;

    [DllImport("inpoutx64.dll", EntryPoint = "Out32")]
    private static extern void Out32(int address, int value);

    [Serializable]
    public class Settings : ComponentSettings {
        public int portHexAddress;
        public Settings(int portHexAddress) {
            this.portHexAddress = portHexAddress;
        }
    }

    public static void TryOut32(int address, int value) {
        try {
            Out32(address, value);
        }
        catch (System.DllNotFoundException e) {
            Debug.LogException(e);
        }
    }

    public void WriteTrigger(int value) {
        if (portHexAddress == -1) {
            Out32(portHexAddress, value);
            Out32(portHexAddress, 0);
        }
    }

    public void SimpleTest() {
        if (parallelflip) {
            TryOut32(portHexAddress, 255);
        }
        else {
            TryOut32(portHexAddress, 0);
        }

        parallelflip = !parallelflip;
    }

    public override Type GetSettingsType() {
        return typeof(Settings);
    }

    public override ComponentSettings GetDefaultSettings() {
        return new Settings(0);
    }

    public override ComponentSettings GetCurrentSettings() {
        return new Settings(portHexAddress);
    }

    protected override void ApplySettings(ComponentSettings loadedSettings) {
        Settings s = (Settings)loadedSettings;

        portHexAddress = s.portHexAddress;
    }

    public void OnSessionTrigger(SessionTrigger trigger, int triggerValue) {
        WriteTrigger(triggerValue);

        //for reference for now. to be removed
        //switch (trigger) {
        //    case SessionTrigger.CueShownTrigger:
                
        //        break;
        //    case SessionTrigger.TrialStartedTrigger:
        //        TryEyemsg_Printf("Start Trial " + triggerValue);
        //        break;
        //    case SessionTrigger.TrialEndedTrigger:
        //        TryEyemsg_Printf("End Trial " + triggerValue);
        //        break;
        //    case SessionTrigger.TimeoutTrigger:
        //        TryEyemsg_Printf("Timeout " + triggerValue);
        //        break;
        //    case SessionTrigger.ExperimentVersionTrigger:
        //        TryEyemsg_Printf("Trigger Version " + GameController.versionNum);
        //        break;
        //}
    }
}
