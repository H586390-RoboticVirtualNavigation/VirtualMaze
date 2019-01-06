using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Settings {
    public string settingName;
    public string rewardPort;

    public string interTrialFixed;
    public string interTrialMin;
    public string interTrialMax;
    public string completionWindow;
    public string rewardTime;
    public string timeOut;
    public string interSessionTime;
    public bool randomizeInterTrialTime;

    public float rewardViewCriteria;

    public bool enablePoster;

}

public class GuiController : MonoBehaviour {
    private const string Msg_EmptySettingsName = "No Settings Name Found! Please enter a name for the current Settings";

    //to be appended at the end of the settings name
    private const string Msg_UpdatedSettingsMsg = " updated!";
    private const string Msg_AddedSettings = " added!";
    private const string Msg_LoadedSettings = " loaded!";
    private const string Msg_DeletedSettings = " loaded!";
    private const string Msg_DeletedSettingsFailed = " failed to delete";

    public UnityEvent test = new UnityEvent();


    public static InputField dirField;
    public static InputField rewardPortField;

    public RobotMovement robotMovement;

    private static FileBrowser filebrowser;
    private static Toggle interTrialRandomize;

    private static InputField interTrialMin;
    private static InputField interTrialMax;
    private static InputField interTrialFixed;
    private static InputField completionWindowFixed;
    private static InputField rewardFixed;
    private static InputField timeoutFixed;
    private static InputField interSessionFixed;
    private static Toggle interTrialValid;
    private static Toggle interSessionValid;
    private static Toggle completionWindowValid;
    private static Toggle rewardValid;
    private static Toggle timeoutValid;
    private static Toggle directoryValid;
    private static Text startStatus;
    private static Text dirStatus;
    private static Button startButton;
    private static Button joyStickButton;
    private static GameObject replayServer;
    private static Button rewardButton;
    private static Image syncImage;
    private static Text ExperimentStatusText;
    private static InputField settingsField;
    private static GameObject settingsMenu;

    public static Slider rewardViewCriteriaSlider;

    private static Toggle enablePoster;

    public static string experimentStatus {
        set {
            ExperimentStatusText.text = value;
        }
    }

    public static int interTrialTime {
        get {
            if (interTrialRandomize.isOn) {
                int min, max;
                if (int.TryParse(interTrialMin.text, out min) && int.TryParse(interTrialMax.text, out max) && max >= min) {
                    return (int)Random.Range((float)min, (float)max);
                }
                else {
                    return -1;
                }
            }
            else {
                int val;
                if (int.TryParse(interTrialFixed.text, out val)) {
                    return val;
                }
                else {
                    return -1;
                }
            }
        }
    }

    public static int interSessionTime {
        get {
            int val;
            if (int.TryParse(interSessionFixed.text, out val)) {
                return val;
            }
            else {
                return -1;
            }
        }
    }

    public static int completionWindowTime {
        get {
            int val;
            if (int.TryParse(completionWindowFixed.text, out val)) {
                return val;
            }
            else {
                return -1;
            }
        }
    }

    public static int rewardTime {
        get {
            int val;
            if (int.TryParse(rewardFixed.text, out val)) {
                return val;
            }
            else {
                return -1;
            }
        }
    }

    public static int timoutTime {
        get {
            int val;
            if (int.TryParse(timeoutFixed.text, out val)) {
                return val;
            }
            else {
                return -1;
            }
        }
    }

    private bool _guiEnable;
    public bool guiEnable {
        get {
            return _guiEnable;
        }
        set {
            if (value) {
                this.gameObject.GetComponent<CanvasGroup>().alpha = 1f;
                this.gameObject.GetComponent<CanvasGroup>().interactable = true;
                this.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
            }
            else {
                this.gameObject.GetComponent<CanvasGroup>().alpha = 0f;
                this.gameObject.GetComponent<CanvasGroup>().interactable = false;
                this.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
            _guiEnable = value;
        }
    }

    void Awake() {

        filebrowser = GameObject.Find("FileBrowser").GetComponent<FileBrowser>();
        interTrialRandomize = GameObject.Find("InterTrial/randomize").GetComponent<Toggle>();
        interTrialMin = GameObject.Find("InterTrial/minInput").GetComponent<InputField>();
        interTrialMax = GameObject.Find("InterTrial/maxInput").GetComponent<InputField>();
        interTrialFixed = GameObject.Find("InterTrial/fixedInput").GetComponent<InputField>();
        completionWindowFixed = GameObject.Find("CompletionWindow/fixedInput").GetComponent<InputField>();
        rewardFixed = GameObject.Find("Reward/fixedInput").GetComponent<InputField>();
        timeoutFixed = GameObject.Find("Timeout/fixedInput").GetComponent<InputField>();
        interTrialValid = GameObject.Find("InterTrial/valid").GetComponent<Toggle>();

        interSessionFixed = GameObject.Find("InterSession/fixedInput").GetComponent<InputField>();
        interSessionValid = GameObject.Find("InterSession/valid").GetComponent<Toggle>();

        completionWindowValid = GameObject.Find("CompletionWindow/valid").GetComponent<Toggle>();
        rewardValid = GameObject.Find("Reward/valid").GetComponent<Toggle>();
        timeoutValid = GameObject.Find("Timeout/valid").GetComponent<Toggle>();
        startStatus = GameObject.Find("StartPanel/startStatus").GetComponent<Text>();
        dirStatus = GameObject.Find("StartPanel/dirStatus").GetComponent<Text>();
        dirField = GameObject.Find("StartPanel/DirectoryField").GetComponent<InputField>();
        directoryValid = GameObject.Find("StartPanel/directoryValid").GetComponent<Toggle>();
        startButton = GameObject.Find("StartPanel/StartButton").GetComponent<Button>();
        syncImage = GameObject.Find("PhotoDiode").GetComponent<Image>();

        rewardPortField = GameObject.Find("PortsPanel/RewardPort").GetComponentInChildren<InputField>();

        joyStickButton = GameObject.Find("PortsPanel/JoystickPort").GetComponentInChildren<Button>();
        rewardButton = GameObject.Find("PortsPanel/RewardPort").GetComponentInChildren<Button>();
        ExperimentStatusText = GameObject.Find("PortsPanel/ExperimentStatus").GetComponent<Text>();

        settingsMenu = GameObject.Find("SettingsPanel/SettingsField/menu");


        rewardViewCriteriaSlider = GameObject.Find("RewardViewCriteriaSlider").GetComponent<Slider>();
        replayServer = GameObject.Find("PortsPanel/ReplayServer");

        //rotationSpeedSlider = GameObject.Find("RotationSpeedSlider").GetComponent<Slider>();
        //translationSpeedSlider = GameObject.Find("TranslationSpeedSlider").GetComponent<Slider>();
        //enableReverseToggle = GameObject.Find("Reverse").GetComponent<Toggle>();
        //enableForwardToggle = GameObject.Find("Forward").GetComponent<Toggle>();
        //enableRightToggle = GameObject.Find("Right").GetComponent<Toggle>();
        //enableLeftToggle = GameObject.Find("Left").GetComponent<Toggle>();
        //enablePoster = GameObject.Find("ShowPosters").GetComponent<Toggle>();
    }

    void Start() {
        Debug.Log("START");
        filebrowser.InitWithDirectory(Application.dataPath);
        filebrowser.fileBrowserCancelEvent += FileBrowserCancel;
        filebrowser.fileBrowserChooselEvent += FileBrowserChoose;
        filebrowser.display = false;

        HideIntertrialComponents(interTrialRandomize.isOn);
        InputFieldEndEdit();
    }

    public void OnCalibrate() {
        SceneManager.LoadScene("calib_scene");
    }

    public void OnPosterToggle(bool value) {
        GameObject posters = GameObject.Find("Poster");
        if (posters != null) {
            foreach (Transform child in posters.transform) {
                child.gameObject.GetComponent<MeshRenderer>().enabled = value;
            }
        }
    }

    public void OnReplayServerClick() {
        SceneManager.LoadScene("Replay");
    }

    void FixedUpdate() {

        //listen for ESC
        if (Input.GetKeyDown(KeyCode.Escape)) {
            guiEnable = !guiEnable;
        }
    }

    void OnEnable() {
        //interTrialRandomize.onValueChanged.AddListener(OnRandomize);
        EventManager.StartListening("Start Experiment", StartExperiment);
        EventManager.StartListening("Stop Experiment", StopExperiment);
    }

    void OnDisable() {
        //interTrialRandomize.onValueChanged.RemoveListener(OnRandomize);
        EventManager.StopListening("Start Experiment", StartExperiment);
        EventManager.StopListening("Stop Experiment", StopExperiment);
    }

    void OnRandomize(bool value) {
        HideIntertrialComponents(value);
        InputFieldEndEdit();
    }

    void HideIntertrialComponents(bool value) {
        if (value) {
            interTrialMin.gameObject.SetActive(true);
            interTrialMax.gameObject.SetActive(true);
            interTrialFixed.gameObject.SetActive(false);
        }
        else {
            interTrialMin.gameObject.SetActive(false);
            interTrialMax.gameObject.SetActive(false);
            interTrialFixed.gameObject.SetActive(true);
        }
    }

    public void InputFieldEndEdit() {
        int value;

        //check all input fields if the value is valid
        if (interTrialRandomize.isOn) {
            int min, max;
            if (int.TryParse(interTrialMin.text, out min) && int.TryParse(interTrialMax.text, out max) && max >= min) {
                ToggleValid(interTrialValid, true);
            }
            else {
                ToggleValid(interTrialValid, false);
            }
        }
        else {

            if (int.TryParse(interTrialFixed.text, out value)) {
                ToggleValid(interTrialValid, true);
            }
            else {
                ToggleValid(interTrialValid, false);
            }
        }

        if (int.TryParse(interSessionFixed.text, out value)) {
            ToggleValid(interSessionValid, true);
        }
        else {
            ToggleValid(interSessionValid, false);
        }

        if (int.TryParse(completionWindowFixed.text, out value)) {
            ToggleValid(completionWindowValid, true);
        }
        else {
            ToggleValid(completionWindowValid, false);
        }

        if (int.TryParse(rewardFixed.text, out value)) {
            ToggleValid(rewardValid, true);
        }
        else {
            ToggleValid(rewardValid, false);
        }

        if (int.TryParse(timeoutFixed.text, out value)) {
            ToggleValid(timeoutValid, true);
        }
        else {
            ToggleValid(timeoutValid, false);
        }

        //check directory
        CheckDirectory(dirField.text);
    }

    void ToggleValid(Toggle toggle, bool value) {
        toggle.isOn = value;
        toggle.GetComponentInChildren<Image>().color = value ? Color.green : Color.red;
    }

    public void OpenDirectory() {
        filebrowser.display = true;
        guiEnable = false;
    }

    void FileBrowserCancel() {
        filebrowser.display = false;
        guiEnable = true;
    }

    void FileBrowserChoose(string path) {
        filebrowser.display = false;
        dirField.text = path;
        guiEnable = true;
    }

    public void CheckDirectory(string path) {

        try {
            if (Directory.Exists(path)) {
                dirStatus.text = "saving to folder";
                dirStatus.color = Color.green;
                ToggleValid(directoryValid, true);
            }
            else {
                dirStatus.text = "invalid folder";
                dirStatus.color = Color.red;
                ToggleValid(directoryValid, false);
            }
        }

        catch (System.Exception ex) {
            Debug.LogError(ex.ToString());
            dirStatus.text = "check folder path";
            dirStatus.color = Color.red;
            ToggleValid(directoryValid, false);
        }
    }

    public void StartExperimentClicked() {
        Debug.Log("start button");
        if (startButton.GetComponentInChildren<Text>().text == "Start") {
            Debug.Log("start button");
            if (CheckValidToggles()) {
                EventManager.TriggerEvent("Start Experiment");
            }
            else {
                startStatus.text = "check parameters";
                startStatus.color = Color.red;
                Debug.Log("errors, cant start experiment");
            }
        }
        else {
            EventManager.TriggerEvent("Stop Experiment");
        }
    }

    void StartExperiment() {
        startStatus.text = "started";
        startStatus.color = Color.green;
        startButton.GetComponentInChildren<Text>().text = "Stop";
    }

    void StopExperiment() {
        startStatus.text = "stopped";
        startStatus.color = Color.red;
        startButton.GetComponentInChildren<Text>().text = "Start";
    }

    public bool CheckValidToggles() {

        //check toggles
        if (interTrialValid.isOn == false)
            return false;
        if (completionWindowValid.isOn == false)
            return false;
        if (rewardValid.isOn == false)
            return false;
        if (timeoutValid.isOn == false)
            return false;
        if (directoryValid.isOn == false)
            return false;

        //check each session prefab for valid
        //foreach (Transform element in verticalSessionsPanel.transform) {
        //    if (element.gameObject.GetComponent<SessionPrefabScript>().valid == false)
        //        return false;
        //}

        return true;
    }
}
