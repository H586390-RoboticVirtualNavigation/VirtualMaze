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

    public bool PostersEnabled { get; set; }
    public string SaveLocation { get; set; }
    public int SessionIntermissionDuration { get; set; }

    public bool started { get; private set; }  = false;
    private ExperimentLogger logger = null;
    private RobotMovement robot;
    //coroutine reference for properly stopping coroutine
    private Coroutine goNextLevelCoroutine;

    // Caches the SessionTrigger for logging the trigger with the robot movement
    private SessionTrigger triggerCache = SessionTrigger.NoTrigger;
    private int rewardIndexCache = 0; // value will be ignored when NoTrigger, 0 based
    private BasicLevelController levelController;
    private bool isPaused = false;

    private WaitUntil waitIfPaused; 

    //drag in Unity Editor
    public SessionController sessionController;

    protected override void Awake() {
        base.Awake();
        robot = GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<RobotMovement>();
        waitIfPaused = new WaitUntil(() => !isPaused);
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += SceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= SceneLoaded;
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "Start") return;

        //add listener to the session
        GameObject levelControllerObject = GameObject.FindWithTag(Tags.LevelController);
        if (levelControllerObject == null) {
            Debug.LogError("No GameObject found with the tag " + Tags.LevelController);
            StopExperiment();
        }
        else {
            levelController =
                levelControllerObject.GetComponent<BasicLevelController>();
            Debug.Log(levelController.GetType().Name);
            levelController.onSessionFinishEvent.AddListener(OnSessionEnd);
            levelController.onSessionTrigger.AddListener(OnSessionTriggered);
            levelController.isPaused = isPaused;
        }

        //start logging robotmovement
        robot.OnRobotMoved += OnRobotMoved;
    }

    /// <summary>
    /// Toggles if the experiment should pause
    /// </summary>
    /// <returns>returns true if preparing to pause</returns>
    public bool TogglePause() {
        isPaused = !isPaused;

        if (isPaused) {
            robot.OnRobotMoved -= OnRobotMoved;
        }
        else {
            robot.OnRobotMoved += OnRobotMoved;
        }

        levelController.isPaused = isPaused;
        return isPaused;
    }

    public void StartExperiment() {
        //ignore btn click if already started.
        if (started) return;

        Console.Write("Experiment Started");
        started = true;
        sessionController.RestartIndex();

        if (logger != null) {
            //cleanup
            logger.CloseLog();
        }

        //initilize ExperimentLogger
        logger = new ExperimentLogger(SaveLocation, ExperimentLogger.GenerateDefaultExperimentID());

        goNextLevelCoroutine = StartCoroutine(GoToNextSession());
    }

    private IEnumerator GoToNextSession() {
        // checks if should pause else continue.
        if (isPaused) {
            Console.Write("ExperimentPaused");
        }
        yield return waitIfPaused;

        if (sessionController.HasNextLevel()) {
            Session session = sessionController.NextLevel();
            int sessionIndex = sessionController.index;

            //delay and display countdown
            float countDownTime = SessionIntermissionDuration / 1000.0f;
            while (countDownTime > 0) {
                Debug.Log("countdown" + countDownTime);
                //GuiController.experimentStatus = string.Format("starting session {0} in {1:F2}", sessionIndex, countDownTime);
                yield return new WaitForSeconds(0.1f);
                countDownTime -= 0.1f;
            }

            //if logger fails to open
            if (!logger.OpenSessionLog(sessionIndex, session, SaveLoad.getCurrentSettings())) {
                Debug.LogError("failed to create save files");
                StopExperiment();
                yield break; // stops the coroutine
            }

            //prepare data for the session
            SessionInfo.SetSessionInfo(session);

            //start the scene
            SceneManager.LoadScene(session.level, LoadSceneMode.Single);

            Console.Write(string.Format("session {0} started", sessionIndex));

        }
        else {
            StopExperiment();
            yield break; // stops the coroutine
        }
    }

    public void StopExperiment() {
        Debug.Log("Experiment Stopped");
        //coroutine will be not be null if coroutine is still running
        if (goNextLevelCoroutine != null) {
            StopCoroutine(goNextLevelCoroutine);
        }

        robot.OnRobotMoved -= OnRobotMoved;
        levelController?.StopLevel();
        started = false;
        //Clean up when Experiment is stopped adruptly.
        logger.CloseLog();
    }

    private void OnSessionEnd() {
        logger.CloseLog();
        robot.OnRobotMoved -= OnRobotMoved;
        goNextLevelCoroutine = StartCoroutine(GoToNextSession());
    }

    private void OnRobotMoved(Transform t) {
        //needs to be cached as this thid data is logged on every robotmovement.
        switch (triggerCache) {
            case SessionTrigger.NoTrigger:
                logger.LogMovement(t);
                break;
            case SessionTrigger.ExperimentVersionTrigger:
                logger.LogMovement(triggerCache, GameController.versionNum, t);
                // Consume the trigger
                triggerCache = SessionTrigger.NoTrigger;
                break;
            default:
                // logs need rewardIndex to be 1 based
                logger.LogMovement(triggerCache, rewardIndexCache + 1, t);
                // Consume the trigger
                triggerCache = SessionTrigger.NoTrigger;
                break;
        }
    }

    private void OnSessionTriggered(SessionTrigger trigger, int targetIndex) {
        // cache the trigger to be logged with the robot movement
        triggerCache = trigger;
        rewardIndexCache = targetIndex;
        Debug.Log(trigger);
    }

    public void ActivateExternalDisplay() {
        if (Display.displays.Length > 1) {
            Display d = Display.displays[1];
            if (!d.active) {
                d.Activate();
            }
        }
    }

    public override Type GetSettingsType() {
        return typeof(Settings);
    }

    public override ComponentSettings GetDefaultSettings() {
        return new Settings(false, true, -1, -1, -1, -1, -1, -1, "");
    }

    public override ComponentSettings GetCurrentSettings() {
        return new Settings(Session.isTrailIntermissionRandom, PostersEnabled,
            Session.fixedTrialIntermissionDuration, Session.maxTrialIntermissionDuration,
            Session.minTrialIntermissionDuration, SessionIntermissionDuration,
            Session.timeoutDuration, Session.trialTimeLimit, SaveLocation);
    }

    protected override void ApplySettings(ComponentSettings loadedSettings) {
        Settings settings = (Settings)loadedSettings;

        Session.isTrailIntermissionRandom = settings.isTrialIntermissionFixed;
        PostersEnabled = settings.postersEnabled;
        Session.fixedTrialIntermissionDuration = settings.fixedTrialIntermissionDuration;
        Session.maxTrialIntermissionDuration = settings.maxTrialIntermissionDuration;
        Session.minTrialIntermissionDuration = settings.minTrialIntermissionDuration;
        SessionIntermissionDuration = settings.sessionIntermissionDuration;
        Session.timeoutDuration = settings.timeoutDuration;
        Session.trialTimeLimit = settings.timeLimitDuration;
        SaveLocation = settings.saveLocation;
    }

    public override bool IsValid() {
        return base.IsValid();
    }
}
