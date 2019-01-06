using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentController : ConfigurableComponent {
    [Serializable]
    public class Settings : ComponentSettings {
        public bool isTrialIntermissionFixed;
        public bool postersEnabled;

        public int fixedTrialIntermissionDuration;
        public int maxTrialIntermissionDuration;
        public int minTrialIntermissionDuration;

        public string saveLocation;
        public int sessionIntermissionDuration;
        public int timeoutDuration;
        public int timeLimitField;

        public Settings(
            bool isTrialIntermissionFixed,
            bool postersEnabled,
            int fixedTrialIntermissionDuration,
            int maxTrialIntermissionDuration,
            int minTrialIntermissionDuration,
            int sessionIntermissionDuration,
            int timeoutDuration,
            int timeLimitField,
            string saveLocation
            ) {
            this.isTrialIntermissionFixed = isTrialIntermissionFixed;
            this.postersEnabled = postersEnabled;
            this.fixedTrialIntermissionDuration = fixedTrialIntermissionDuration;
            this.maxTrialIntermissionDuration = maxTrialIntermissionDuration;
            this.minTrialIntermissionDuration = minTrialIntermissionDuration;
            this.sessionIntermissionDuration = sessionIntermissionDuration;
            this.timeoutDuration = timeoutDuration;
            this.timeLimitField = timeLimitField;
            this.saveLocation = saveLocation;
        }
    }

    public bool IsTrialIntermissionFixed { get; set; }
    public bool PostersEnabled { get; set; }


    public int FixedTrialIntermissionDuration { get; set; }
    public int MaxTrialIntermissionDuration { get; set; }
    public int MinTrialIntermissionDuration { get; set; }

    public string SaveLocation { get; set; }
    public int SessionIntermissionDuration { get; set; }
    public int TimeoutDuration { get; set; }
    public int TimeLimitDuration { get; set; }

    //drag in Unity Editor
    public SessionController sessionController;

    protected override void Awake() {
        ApplySettings(GetDefaultSettings());

        SaveLoad.RegisterConfigurableComponent(this);
    }

    public void StartExperiment() {
        sessionController.RestartSessions();
        GoToNextLevel();
    }

    private void GoToNextLevel() {
        if (sessionController.HasNextLevel()) {
            Session session = sessionController.NextLevel();
        }
        else {
            StopExperiment();
        }
    }

    public void StopExperiment() {
        //do something
    }

    public override String GetSettingsID() {
        return typeof(Settings).FullName;
    }

    public override ComponentSettings GetDefaultSettings() {
        return new Settings(false, true, -1, -1, -1, -1, -1, -1, "");
    }

    public override ComponentSettings GetCurrentSettings() {
        return new Settings(IsTrialIntermissionFixed, PostersEnabled,
            FixedTrialIntermissionDuration, MaxTrialIntermissionDuration,
            MinTrialIntermissionDuration, SessionIntermissionDuration,
            TimeoutDuration, TimeLimitDuration, SaveLocation);
    }

    protected override void ApplySettings(ComponentSettings loadedSettings) {
        Settings settings = (Settings)loadedSettings;

        IsTrialIntermissionFixed = settings.isTrialIntermissionFixed;
        PostersEnabled = settings.postersEnabled;
        FixedTrialIntermissionDuration = settings.fixedTrialIntermissionDuration;
        MaxTrialIntermissionDuration = settings.maxTrialIntermissionDuration;
        MinTrialIntermissionDuration = settings.minTrialIntermissionDuration;
        SessionIntermissionDuration = settings.sessionIntermissionDuration;
        TimeoutDuration = settings.timeoutDuration;
        TimeLimitDuration = settings.timeLimitField;
        SaveLocation = settings.saveLocation;
    }

    public override bool IsValid() {
        return base.IsValid();
    }
}
