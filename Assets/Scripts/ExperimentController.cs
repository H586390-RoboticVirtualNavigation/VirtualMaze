using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        public int timeLimitDuration;

        public Settings(
            bool isTrialIntermissionFixed,
            bool postersEnabled,
            int fixedTrialIntermissionDuration,
            int maxTrialIntermissionDuration,
            int minTrialIntermissionDuration,
            int sessionIntermissionDuration,
            int timeoutDuration,
            int timeLimitDuration,
            string saveLocation
            ) {
            this.isTrialIntermissionFixed = isTrialIntermissionFixed;
            this.postersEnabled = postersEnabled;
            this.fixedTrialIntermissionDuration = fixedTrialIntermissionDuration;
            this.maxTrialIntermissionDuration = maxTrialIntermissionDuration;
            this.minTrialIntermissionDuration = minTrialIntermissionDuration;
            this.sessionIntermissionDuration = sessionIntermissionDuration;
            this.timeoutDuration = timeoutDuration;
            this.timeLimitDuration = timeLimitDuration;
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

    private bool started = false;
    private ExperimentLogger logger = null;
    private RobotMovement robot;
    //coroutine reference for properly stopping coroutine
    private Coroutine goNextLevelCoroutine;

    //drag in Unity Editor
    public SessionController sessionController;

    protected override void Awake() {
        base.Awake();
        robot = GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<RobotMovement>();
    }

    public void StartExperiment() {
        //ignore btn click if already started.
        if (started) return;

        Debug.Log("Experiment Started");
        started = true;
        sessionController.RestartIndex();

        if (logger != null) {
            //cleanup
            logger.CloseLog();
        }

        //initilize ExperimentLogger
        logger = new ExperimentLogger(SaveLocation, ExperimentLogger.GenerateDefaultExperimentID());

        goNextLevelCoroutine = StartCoroutine(GoToNextLevel(logger));
    }

    private IEnumerator GoToNextLevel(ExperimentLogger logger) {
        if (sessionController.HasNextLevel()) {
            Session session = sessionController.NextLevel();
            int sessionIndex = sessionController.index;

            //if logger fails to open
            if (!logger.OpenSessionLog(sessionIndex, session, SaveLoad.getCurrentSettings())) {
                Debug.LogError("failed to create save files");
                StopExperiment(logger);
                yield break; // stops the coroutine
            }

            //delay and display countdown
            float countDownTime = SessionIntermissionDuration / 1000.0f;
            while (countDownTime > 0) {
                Debug.Log("countdown" + countDownTime);
                //GuiController.experimentStatus = string.Format("starting session {0} in {1:F2}", sessionIndex, countDownTime);
                yield return new WaitForSeconds(0.1f);
                countDownTime -= 0.1f;
            }

            //prepare data for the session
            SessionInfo.SetSessionInfo(logger, TimeLimitDuration, session, TimeoutDuration);

            //start the scene
            SceneManager.LoadScene(session.level, LoadSceneMode.Single);

            //add listener to the session
            BasicLevelController.onSessionFinishEvent.AddListener(OnSessionEnd);

            //start logging robotmovement
            robot.OnRobotMoved += OnRobotMoved;

            //GuiController.experimentStatus = string.Format("session {0} started", sessionIndex);

        }
        else {
            StopExperiment(logger);
            yield break; // stops the coroutine
        }
    }

    public void StopExperiment(ExperimentLogger logger) {
        Debug.Log("Experiment Stopped");
        //coroutine will be not be null if it is still running
        if (goNextLevelCoroutine != null) {
            StopCoroutine(goNextLevelCoroutine);
        }
        started = false;

        //Clean up when Experiment is stopped adruptly.
        logger.CloseLog();
    }

    private void OnSessionEnd() {
        logger.CloseLog();
        robot.OnRobotMoved -= OnRobotMoved;
        goNextLevelCoroutine = StartCoroutine(GoToNextLevel(logger));
    }

    private void OnRobotMoved(Transform t) {
        logger.LogMovement(t);
    }

    public override Type GetSettingsType() {
        return typeof(Settings);
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
        TimeLimitDuration = settings.timeLimitDuration;
        SaveLocation = settings.saveLocation;
    }

    public override bool IsValid() {
        return base.IsValid();
    }
}
