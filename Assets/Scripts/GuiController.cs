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
    public string joyStickPort;
    public string parallelPort;
    public string interTrialFixed;
    public string interTrialMin;
    public string interTrialMax;
    public string completionWindow;
    public string rewardTime;
    public string timeOut;
    public string interSessionTime;
    public bool randomizeInterTrialTime;
    public bool useJoystick;

    public float rotationSpeed;
    public float translationSpeed;
    public float joystickDeadzone;
    public float rewardViewCriteria;

    public bool enableReverse;
    public bool enableForward;
    public bool enableRight;
    public bool enableLeft;
    public bool enablePoster;

    public List<Dictionary<string, string>> sessionList;
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

    public GameObject sessionPrefab;
    public GameObject settingsOptionPrefab;
    public static InputField dirField;
    public static InputField rewardPortField;
    public InputField parallelPortField;

    public RobotMovement robotMovement;
    public SaveLoad saveController;

    private static FileBrowser filebrowser;
    private static Toggle interTrialRandomize;
    private static InputField joystickPortField;
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
    private static Button addSessionsButton;
    private static GameObject verticalSessionsPanel;
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
    private static Toggle useJoystickToggle;
    public static Slider rotationSpeedSlider;
    public static Slider translationSpeedSlider;
    public static Slider joystickDeadzoneSlider;
    public static Slider rewardViewCriteriaSlider;

    private static Toggle enableReverseToggle;
    private static Toggle enableForwardToggle;
    private static Toggle enableRightToggle;
    private static Toggle enableLeftToggle;
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

    private bool _syncImageOn;
    private bool syncImageOn {
        get {
            return _syncImageOn;
        }
        set {
            if (value) {
                syncImage.color = Color.black;
            }
            else {
                syncImage.color = Color.white;
            }
            _syncImageOn = value;
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
        addSessionsButton = GameObject.Find("SessionsPanel/AddSessionButton").GetComponent<Button>();
        verticalSessionsPanel = GameObject.Find("SessionsPanel/ScrollPanel/SessionsItemPanel");
        startStatus = GameObject.Find("StartPanel/startStatus").GetComponent<Text>();
        dirStatus = GameObject.Find("StartPanel/dirStatus").GetComponent<Text>();
        dirField = GameObject.Find("StartPanel/DirectoryField").GetComponent<InputField>();
        directoryValid = GameObject.Find("StartPanel/directoryValid").GetComponent<Toggle>();
        startButton = GameObject.Find("StartPanel/StartButton").GetComponent<Button>();
        syncImage = GameObject.Find("PhotoDiode").GetComponent<Image>();
        joystickPortField = GameObject.Find("PortsPanel/JoystickPort").GetComponentInChildren<InputField>();
        rewardPortField = GameObject.Find("PortsPanel/RewardPort").GetComponentInChildren<InputField>();
        parallelPortField = GameObject.Find("PortsPanel/ParallelPort").GetComponentInChildren<InputField>();
        joyStickButton = GameObject.Find("PortsPanel/JoystickPort").GetComponentInChildren<Button>();
        rewardButton = GameObject.Find("PortsPanel/RewardPort").GetComponentInChildren<Button>();
        ExperimentStatusText = GameObject.Find("PortsPanel/ExperimentStatus").GetComponent<Text>();
        settingsField = GameObject.Find("SettingsPanel/SettingsField").GetComponentInChildren<InputField>();
        settingsMenu = GameObject.Find("SettingsPanel/SettingsField/menu");
        useJoystickToggle = GameObject.Find("SettingsPanel/UseJoystick").GetComponent<Toggle>();
        rotationSpeedSlider = GameObject.Find("RotationSpeedSlider").GetComponent<Slider>();
        joystickDeadzoneSlider = GameObject.Find("JoystickDeadzoneSlider").GetComponent<Slider>();
        translationSpeedSlider = GameObject.Find("TranslationSpeedSlider").GetComponent<Slider>();
        rewardViewCriteriaSlider = GameObject.Find("RewardViewCriteriaSlider").GetComponent<Slider>();
        replayServer = GameObject.Find("PortsPanel/ReplayServer");

        enableReverseToggle = GameObject.Find("Reverse").GetComponent<Toggle>();
        enableForwardToggle = GameObject.Find("Forward").GetComponent<Toggle>();
        enableRightToggle = GameObject.Find("Right").GetComponent<Toggle>();
        enableLeftToggle = GameObject.Find("Left").GetComponent<Toggle>();
        enablePoster = GameObject.Find("ShowPosters").GetComponent<Toggle>();
    }

    void Start() {
        Debug.Log("START");
        filebrowser.InitWithDirectory(Application.dataPath);
        filebrowser.fileBrowserCancelEvent += FileBrowserCancel;
        filebrowser.fileBrowserChooselEvent += FileBrowserChoose;
        filebrowser.display = false;

        HideIntertrialComponents(interTrialRandomize.isOn);
        InputFieldEndEdit();
        addSessionsButton.onClick.AddListener(AddSession);

        //off sync
        syncImageOn = false;
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

    public void OnJoystickClick() {

        if (joyStickButton.GetComponentInChildren<Text>().text.Equals("Open")) {
            if (SerialController.instance.JoystickOpen(joystickPortField.text)) {
                joyStickButton.GetComponentInChildren<Text>().text = "Close";
                joystickPortField.image.color = Color.green;
            }
            else {
                joystickPortField.image.color = Color.red;
            }
        }
        else {
            joyStickButton.GetComponentInChildren<Text>().text = "Open";
            joystickPortField.image.color = Color.red;
        }
    }

    public void OnRewardClick() {

        if (rewardButton.GetComponentInChildren<Text>().text.Equals("On Valve")) {
            if (SerialController.instance.RewardValveOn(rewardPortField.text) == true) {
                rewardButton.GetComponentInChildren<Text>().text = "Off Valve";
                rewardPortField.image.color = Color.green;
            }
            else {
                //unable to open serial
                experimentStatus = "cant open reward serial";
                rewardPortField.image.color = Color.red;
            }
        }
        else {
            SerialController.instance.RewardValveOff();
            rewardButton.GetComponentInChildren<Text>().text = "On Valve";
        }
    }

    bool parallelflip = false;

    public void OnParallelTestClick() {

        try {
            int addr = int.Parse(parallelPortField.text, System.Globalization.NumberStyles.HexNumber);

            if (parallelflip) {
                ParallelPort.TryOut32(addr, 255);
            }
            else {
                ParallelPort.TryOut32(addr, 0);
            }
            parallelflip = !parallelflip;
            parallelPortField.image.color = Color.green;

        }
        catch (System.Exception e) {
            experimentStatus = e.ToString();
            parallelPortField.image.color = Color.red;
        }
    }

    int numSyncs = 2000;
    float timeBetweenSyncs = 0.06f;
    float accTime = 0;
    bool startSync = false;

    public void StartPhotoDiode() {
        startSync = true;
        numSyncs = 2000;
    }

    void FixedUpdate() {

        //listen for ESC
        if (Input.GetKeyDown(KeyCode.Escape)) {
            guiEnable = !guiEnable;
        }
        if (startSync && numSyncs > 0) {
            accTime += Time.deltaTime;
            if (accTime >= timeBetweenSyncs) {
                accTime = 0;
                syncImageOn = !syncImageOn;
                numSyncs--;
                OnParallelTestClick();
            }
        }
        else if (startSync && numSyncs <= 0) {
            startSync = false;
            numSyncs = 2000;
            accTime = 0;
        }
    }

    void OnEnable() {
        interTrialRandomize.onValueChanged.AddListener(OnRandomize);
        EventManager.StartListening("Start Experiment", StartExperiment);
        EventManager.StartListening("Stop Experiment", StopExperiment);
    }

    void OnDisable() {
        interTrialRandomize.onValueChanged.RemoveListener(OnRandomize);
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

    void AddSession() {
        GameObject session = Instantiate(sessionPrefab) as GameObject;
        session.transform.SetParent(verticalSessionsPanel.transform);
        session.transform.localPosition = Vector3.zero;
        session.transform.localRotation = Quaternion.identity;
        session.transform.localScale = new Vector3(1, 1, 1);
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
        foreach (Transform element in verticalSessionsPanel.transform) {
            if (element.gameObject.GetComponent<SessionPrefabScript>().valid == false)
                return false;
        }

        return true;
    }

    public void SaveButtonClicked() {
        string settingsName = settingsField.text;

        if (string.IsNullOrEmpty(settingsName)) {
            experimentStatus = Msg_EmptySettingsName;
        }
        else {
            bool isReplaced = saveController.SaveSetting(settingsName);
            if (isReplaced) {
                experimentStatus = settingsName + Msg_UpdatedSettingsMsg;
            }
            else {
                experimentStatus = settingsName + Msg_AddedSettings;
            }
        }
    }

    public void LoadSettingsButtonClicked() {
        //load settings list
        List<string> settingsList = saveController.SettingsList;

        //remove current options
        foreach (Transform child in settingsMenu.transform) {
            Destroy(child.gameObject);
        }

        //Change this ui to dropdown list?
        //create option prefabs
        foreach (string settingName in settingsList) {
            GameObject option = Instantiate(settingsOptionPrefab) as GameObject;
            option.GetComponentInChildren<Text>().text = settingName;
            option.transform.SetParent(settingsMenu.transform);
            option.transform.localPosition = Vector3.zero;
            option.transform.localRotation = Quaternion.identity;
            option.transform.localScale = new Vector3(1, 1, 1);

            option.GetComponent<Button>().onClick.AddListener(
                delegate { OptionClicked(option.GetComponentInChildren<Text>().text); }
            );
        }
    }

    void OptionClicked(string settingName) {
        saveController.ApplySettings(settingName);
        settingsField.text = settingName;
        experimentStatus = settingName + Msg_LoadedSettings;
    }

    public void DeleteSetting() {
        string settingName = settingsField.text;
        if (saveController.DeleteSetting(settingName)) {
            experimentStatus = settingName + Msg_DeletedSettings;
            settingsField.text = "";
        }
        else {
            experimentStatus = settingName + Msg_DeletedSettingsFailed;
        }

    }

    public void CloseSettings() {
        foreach (Transform child in settingsMenu.transform) {
            Destroy(child.gameObject);
        }
    }
}
