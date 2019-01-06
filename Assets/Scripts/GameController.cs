using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public static string versionInfo = "20171221 Taxi Continuous v4"; //UPDATE THIS WITH EACH COMPILATION
    public static string pportInfo = "v4";

    private static GameController _instance;
    public static GameController instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType(typeof(GameController)) as GameController;
                if (_instance == null) {
                    Debug.LogError("need at least one GameController");
                }
            }
            return _instance;
        }
    }

    public GameObject[] listToDisable;

    //drag in from editor
    public GameObject sessions;
    public GuiController guiController;
    public RewardsController rewardsController;
    public Fading fade;

    public string dataDirectory;
    private DirectoryInfo dataDirectoryIO;
    public StreamWriter fs;
    public int numTrials;
    //public int parallelPortAddr;

    private List<Dictionary<string, string>> sessionlist = new List<Dictionary<string, string>>();
    public int sessionCounter;

    private string fileTime;

    void OnEnable() {
        EventManager.StartListening("Reward", onLegacyReward);
        EventManager.StartListening("Start Experiment", StartExperiment);
        EventManager.StartListening("Level Ended", LevelEnd);
        EventManager.StartListening("Stop Experiment", StopExperiment);
    }

    void OnDisable() {
        EventManager.StopListening("Reward", onLegacyReward);
        EventManager.StopListening("Start Experiment", StartExperiment);
        EventManager.StopListening("Stop Experiment", StopExperiment);
        EventManager.StopListening("Level Ended", LevelEnd);
    }

    /// <summary>
    /// method to check if there is an area of code still blasting rewards events
    /// 
    /// current design will not be using Observer pattern for now.
    /// </summary>
    void onLegacyReward() {
        Debug.LogError("There is still Reward events");
    }

    void StartExperiment() {

        GuiController.experimentStatus = "started experiment";
        Debug.Log("start");

        sessionCounter = 0;

        //set data directory
        dataDirectory = GuiController.dirField.text;

        sessionlist = new List<Dictionary<string, string>>();
        
        //get sessionlist
        foreach (Transform transform in sessions.transform) {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            SessionPrefabScript session = transform.gameObject.GetComponent<SessionPrefabScript>();
            dict.Add("numTrials", session.numTrials);
            //dict.Add("level", session.level);
            sessionlist.Add(dict);
        }

        if (sessionlist.Count > 0) {
            sessionCounter++;

            //create new data file for session
            if (fs != null) {
                fs.Dispose();
                fs = null;
            }
            System.DateTime dateNow = System.DateTime.Now;
            fileTime = dateNow.Day.ToString() + dateNow.Month.ToString() + dateNow.Year.ToString() + dateNow.Hour.ToString() + dateNow.Minute.ToString() + dateNow.Second.ToString();
            fs = FileWriter.CreateFileInFolder(dataDirectory, "session_" + sessionCounter.ToString() + '_' + fileTime + ".txt");

            if (fs == null) {
                Debug.LogError("failed to create save files");
                EventManager.TriggerEvent("Stop Experiment");
            }
            else {
                fs.WriteLine("Version: {0}", versionInfo);
                fs.WriteLine("Trigger: {0}", pportInfo);
                fs.WriteLine("TaskType: Continuous");
                fs.WriteLine("PosterLocations: P1(-5,1.5,-7.55) P2(-7.55,1.5,5) P3(7.55,1.5,-5) P4(5,1.5,7.55) P5(5,1.5,2.45) P6(-5,1.5,-2.45)");
                fs.WriteLine("TrialType: {0}", sessionlist[0]["level"]);
                fs.WriteLine("SpecifiedRewardNo: {0}", InputRewardNo.inputrewardno);
                fs.WriteLine("CompletionWindow: {0}", GuiController.completionWindowTime);
                fs.WriteLine("TimeoutDuration: {0}", GuiController.timoutTime);
                fs.WriteLine("IntersessionInterval: {0}", GuiController.interSessionTime);
                fs.WriteLine("RewardTime: {0}", GuiController.rewardTime);
                //fs.WriteLine("RotationSpeed: {0}", GuiController.rotationSpeedSlider.value);
                //fs.WriteLine("TranslationSpeed: {0}", GuiController.translationSpeedSlider.value);
                //fs.WriteLine("JoystickDeadzone: {0}", GuiController.joystickDeadzoneSlider.value);
                fs.WriteLine("RewardViewCriteria: {0}", GuiController.rewardViewCriteriaSlider.value);
            }

            //set number of trials
            numTrials = int.Parse(sessionlist[0]["numTrials"]);

            //start session after delay
            StartCoroutine("StartNextSessionAfterDelay");

        }
        else {
            EventManager.TriggerEvent("Stop Experiment");
        }
    }

    IEnumerator StartNextSessionAfterDelay() {
        float countDownTime = (float)GuiController.interSessionTime / 1000.0f;
        while (countDownTime > 0) {
            GuiController.experimentStatus = string.Format("starting session {0} in {1:F2}", sessionCounter, countDownTime);
            yield return new WaitForSeconds(0.1f);
            countDownTime -= 0.1f;
        }
        SceneManager.LoadScene(sessionlist[0]["level"]);
        sessionlist.RemoveAt(0);
        GuiController.experimentStatus = string.Format("session {0} started", sessionCounter);
    }

    void StopExperiment() {

        StopCoroutine("StartNextSessionAfterDelay");
        GuiController.experimentStatus = "stopped experiment";

        if (fs != null) {
            fs.Dispose();
            fs = null;
        }

        SceneManager.LoadScene("Start");
    }

    void LevelEnd() {

        if (sessionlist.Count > 0) {
            sessionCounter++;

            //create new data file for session
            if (fs != null) {
                fs.Dispose();
                fs = null;
            }
            Debug.Log("close file");
            fs = FileWriter.CreateFileInFolder(dataDirectory, "session_" + sessionCounter.ToString() + '_' + fileTime + ".txt");
            if (fs == null) {
                Debug.LogError("failed to create save files");
                EventManager.TriggerEvent("Stop Experiment");
            }
            else {
                fs.WriteLine("Version: {0}", versionInfo);
                fs.WriteLine("Trigger: {0}", pportInfo);
                fs.WriteLine("TaskType: Continuous");
                fs.WriteLine("PosterLocations: P1(-5,1.5,-7.55) P2(-7.55,1.5,5) P3(7.55,1.5,-5) P4(5,1.5,7.55) P5(5,1.5,2.45) P6(-5,1.5,-2.45)");
                fs.WriteLine("TrialType = {0}", sessionlist[0]["level"]);
                fs.WriteLine("SpecifiedRewardNo: {0}", InputRewardNo.inputrewardno);
                fs.WriteLine("CompletionWindow: {0}", GuiController.completionWindowTime);
                fs.WriteLine("TimeoutDuration: {0}", GuiController.timoutTime);
                fs.WriteLine("IntersessionInterval: {0}", GuiController.interSessionTime);
                fs.WriteLine("RewardTime: {0}", GuiController.rewardTime);
                //fs.WriteLine("RotationSpeed: {0}", GuiController.rotationSpeedSlider.value);
                //fs.WriteLine("TranslationSpeed: {0}", GuiController.translationSpeedSlider.value);
                //fs.WriteLine("JoystickDeadzone: {0}", GuiController.joystickDeadzoneSlider.value);
                fs.WriteLine("RewardViewCriteria: {0}", GuiController.rewardViewCriteriaSlider.value);

                //set number of trials
                numTrials = int.Parse(sessionlist[0]["numTrials"]);

                //start session after delay
                StartCoroutine("StartNextSessionAfterDelay");
            }

        }
        else {
            EventManager.TriggerEvent("Stop Experiment");
        }
    }

    private void Start() {
        Application.targetFrameRate = 30;
    }
}
