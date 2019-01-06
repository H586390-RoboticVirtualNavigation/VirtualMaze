using System.Collections;
using System.IO.Ports;
using System;
using UnityEngine;

public class RewardsController : ConfigurableComponent {
    [Serializable]
    public class Settings : ComponentSettings {
        public string portNum;
        public int rewardDurationMilliSecs;

        public Settings(string portNum, int rewardDurationMilliSecs) {
            this.portNum = portNum;
            this.rewardDurationMilliSecs = rewardDurationMilliSecs;
        }
    }

    public string portNum;
    public int rewardDurationMilliSecs;
    private const int buadRate = 9600;
    private static SerialPort rewardSerial;
    public bool isPortOpen { get; private set; }

    public bool RewardValveOn() {
        //dispose if exists
        if (rewardSerial != null) {
            rewardSerial.Close();
            rewardSerial.Dispose();
        }

        //open port
        try {
            rewardSerial = new SerialPort(portNum, buadRate);
            rewardSerial.ReadTimeout = 60;
            rewardSerial.DtrEnable = true;
            rewardSerial.RtsEnable = true;

            rewardSerial.Open();
        }
        catch {
            return false;
        }
        isPortOpen = true;
        return true;
    }

    public void RewardValveOff() {
        if (rewardSerial != null) {
            rewardSerial.Close();
            rewardSerial.Dispose();
            rewardSerial = null;
        }
        isPortOpen = false;
    }

    public void Reward() {
        StartCoroutine(RewardRoutine());
    }

    private IEnumerator RewardRoutine() {
        //PlayerAudio.instance.PlayRewardClip ();
        RewardValveOn();
        //delay for rewardTime seconds
        yield return new WaitForSecondsRealtime(rewardDurationMilliSecs / 1000.0f);
        RewardValveOff();
    }

    void OnApplicationQuit() {
        //prevent blocking of serial port
        RewardValveOff();
    }

    public override ComponentSettings GetCurrentSettings() {
        return new Settings(portNum, rewardDurationMilliSecs);
    }

    public override ComponentSettings GetDefaultSettings() {
        return new Settings("", 1000);
    }

    public override String GetSettingsID() {
        return typeof(Settings).FullName;
    }

    protected override void ApplySettings(ComponentSettings loadedSettings) {
        //turn the reward off before settings are changed
        RewardValveOff();

        Settings settings = (Settings)loadedSettings;

        portNum = settings.portNum;
        rewardDurationMilliSecs = settings.rewardDurationMilliSecs;
    }
}
