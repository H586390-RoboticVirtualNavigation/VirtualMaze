using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using RockVR.Video;


public class DataViewer : BasicGUIController, CueController.ITriggerActions {
    private const int Main_Screen = 0;
    private const int Sub_Screen = 1;
    private List<Trial> trials = new List<Trial>();
    private RewardArea[] rewards = null;
    private bool isRecording = false;

    public AudioSource audioSource;

    //Drag and drop
    public VideoCaptureCtrl videoCaptureCtrl;
    public CanvasGroup gui;
    public CanvasGroup dataViewerGUI;
    public CanvasGroup selfMenu;
    public Camera subjectView;
    public RectTransform gazeRect;
    public GazePointPool pool;
    public CueController cueController;
    public Transform robot;

    public Slider scrubber;

    public InputField dataFileField;
    public InputField spikeTrainFileField;

    public Button processBtn;
    public Dropdown trialSelect;
    public FadeCanvas fadeController;

    public Text sessionStatus;
    public Text frameNumStatus;
    public Text trialNumStatus;
    public Text isPlayingStatus;
    public Text dataIgnoredStatus;

    [SerializeField]
    private string customPath;

    [SerializeField]
    private Button recordTrialButton;

    public bool IsPlaying {
        get => _isPlaying;
        private set {
            _isPlaying = value;
            if (_isPlaying) {
                videoCaptureCtrl.StartCapture();
                isPlayingStatus.text = "Playing";
            }
            else {
                videoCaptureCtrl.StopCapture();
                isPlayingStatus.text = "Stopped";
            }
        }
    }
    public bool IsShowingSubjectScreen { get; private set; } = false;

    public int FrameIndex {
        get => _frameindex;
        set {
            _frameindex = value;
            scrubber.value = _frameindex;

            if (FrameIndex >= -1) {
                ShowFrame(trials[TrialIndex], _frameindex);
            }

            frameNumStatus.text = $"Frame: {_frameindex + 1}";
        }
    }

    public int TrialIndex {
        get => _trialIndex;
        set {
            _trialIndex = value;
            SelectTrial(_trialIndex);

            trialNumStatus.text = $"Trial: {_trialIndex + 1}";
        }
    }

    private int _frameindex = 0;
    private int _trialIndex = 0;
    private bool _isPlaying;

    private void Start() {
        dataViewerGUI.SetVisibility(false);

        processBtn.onClick.AddListener(OnProcessBtnClicked);

        trialSelect.onValueChanged.AddListener(OnTrialSelected);

        scrubber.onValueChanged.AddListener(OnScrubber);
        recordTrialButton.onClick.AddListener(StartRecording);
    }

    private void StartRecording() {
        if (isRecording) {

        }
        else {
            if(FileBrowser.IsValidFolder(customPath)) {

            }
        }
    }

    private void OnTrialSelected(int value) {
        TrialIndex = value;
    }

    private void GetTrialNameList(List<Dropdown.OptionData> options) {
        int counter = 1;
        foreach (Trial t in trials) {
            options.Add(new Dropdown.OptionData($"{counter:000}: {t.TrialName}"));
            counter++;
        }
    }

    public class SpikeTimeParser : ICsvLineParser<decimal> {
        public decimal Parse(string[] data) {
            //convert to milliseconds
            return decimal.Parse(data[0]) * 1000m;
        }

        public void ParseHeader(StreamReader reader) {
            //throw away header;
            reader.ReadLine();
        }
    }

    private void OnProcessBtnClicked() {
        string path = dataFileField.text;

        if (!string.IsNullOrEmpty(path)) {
            if (File.Exists(path)) {
                SetInputFieldValid(dataFileField);
                CsvReader<decimal> spikeReader = TryCreateSpikeTrainFile(spikeTrainFileField.text);

                trials.Clear();
                RaycastDataLoader.Load(path, trials, spikeReader);

                trialSelect.ClearOptions();
                TrialIndex = 0;

                GetTrialNameList(trialSelect.options);

                SelectTrial(0);

                trialSelect.gameObject.SetActive(true);

                StartCoroutine(PrepareScene());
            }
            else {
                SetInputFieldInvalid(dataFileField);
            }
        }
    }

    private CsvReader<decimal> TryCreateSpikeTrainFile(string path) {
        if (!string.IsNullOrEmpty(path)) {
            if (File.Exists(path)) {
                SetInputFieldValid(spikeTrainFileField);
                return new CsvReader<decimal>(path, new SpikeTimeParser());
            }
            else {
                SetInputFieldInvalid(spikeTrainFileField);
                return null;
            }
        }
        else {
            SetInputFieldNeutral(spikeTrainFileField);
            return null;
        }
    }

    private IEnumerator PrepareScene() {
        AsyncOperation sceneLoadingOperation = SceneManager.LoadSceneAsync("Double Tee");
        sceneLoadingOperation.allowSceneActivation = true;
        while (!sceneLoadingOperation.isDone) {
            yield return null;
        }

        rewards = RewardArea.GetAllRewardsFromScene();

        TrialIndex = 0;
    }

    private void OnScrubber(float value) {
        if (value == _frameindex) {
            return;
        }

        IsPlaying = false;
        int toFrame = Convert.ToInt32(value);
        FrameIndex = toFrame;
    }

    // Update is called once per frame 
    void Update() {
        if (IsVisible()) {
            ProcessKeyDown();
            ProcessKeyPress();
        }
        if (IsPlaying) {
            ShowNextFrame(false);
        }
    }

    private void ToggleSubjectScreen() {
        if (trials.Count > 0) {
            gui.SetVisibility(IsShowingSubjectScreen);
            dataViewerGUI.SetVisibility(!IsShowingSubjectScreen);
            if (IsShowingSubjectScreen) {
                subjectView.targetDisplay = 1;
            }
            else {
                subjectView.targetDisplay = 0;
            }
            IsShowingSubjectScreen = !IsShowingSubjectScreen;
        }
    }

    private void PrevTrial() {
        Debug.Log(TrialIndex);
        if (TrialIndex > 0) {
            TrialIndex--;
            trialSelect.value = TrialIndex;
        }
    }

    private void NextTrial() {
        if (TrialIndex < trials.Count - 1) {
            TrialIndex++;
            trialSelect.value = TrialIndex;
        }
    }

    private void SelectTrial(int trialNum) {

        pool.ClearScreen();
        Trial t = trials[trialNum];
        scrubber.maxValue = t.GetFrameCount() - 1;
        FrameIndex = 0;

        if (rewards != null) {
            print($"{t.RewardIndex}, {rewards.Length}");
            cueController.SetTargetImage(rewards[t.RewardIndex].cueImage);
        }
    }

    float timepassed = 0;
    float timeToClear = 0;

    private void ShowNextFrame(bool forceShow) {
        timepassed += Time.deltaTime * 1000;
        if (forceShow || timepassed >= timeToClear) {
            timepassed = 0;

            Trial trial = trials[TrialIndex];

            timeToClear = trial.GetFrameAt(FrameIndex).DataCount;

            if (FrameIndex < trial.GetFrameCount() - 1) {
                FrameIndex++;
            }
            else {
                IsPlaying = false;
            }
        }
    }

    private void ShowPrevFrame() {
        if (FrameIndex > 0) {
            FrameIndex--;
        }
        else {
            IsPlaying = false;
        }
    }

    public void ShowFrame(Trial trial, int frameNum) {
        pool.ClearScreen();

        Frame frame = trial.GetFrameAt(frameNum);

        audioSource.PlayOneShot(frame.GetAudioClip());

        if (frame.Config != null) {
            RobotMovement.MoveRobotTo(robot, frame.Config);
        }

        dataIgnoredStatus.gameObject.SetActive(frame.Config == null);
        print("AMi ahsdasd");
        Image i = null;

        foreach (PlaybackData data in frame) {
            if (data is PlaybackSample sample) {
                i = pool.AddGazePoint(gazeRect, subjectView, sample.gaze);
            }
            else if (data is PlaybackEvent evnt){
                print(evnt.trigger);
                CueController.ProcessTrigger(evnt.trigger, cueController, this);
            }
        }
        if (i != null) {
            i.color = Color.red;
        }
    }

    private bool Clamp(int value, int min, int max, out int clamped) {
        clamped = Math.Min(max, value);//clamp to max if value exceeds max
        clamped = Math.Max(min, value);//clamp to min if value exceeds min
        return clamped == value;
    }

    private bool IsVisible() {
        return selfMenu.alpha > 0;
    }

    //framerate dependent
    private readonly int pressDelay = 6;
    private int counter = 6;

    private void ProcessKeyPress() {
        if (Input.GetKey(KeyCode.RightArrow)) {
            if (counter > 0) {
                counter--;
            }
            else {
                ShowNextFrame(true);
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow)) {
            if (counter > 0) {
                counter--;
            }
            else {
                ShowPrevFrame();
            }
        }

        if (Input.GetKeyUp(KeyCode.RightArrow)) {
            counter = pressDelay;
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow)) {
            counter = pressDelay;
        }
    }

    private void ProcessKeyDown() {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            print("previous trial");
            IsPlaying = false;
            PrevTrial();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            print("next Trial");
            IsPlaying = false;
            NextTrial();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            print("previous frame");
            IsPlaying = false;
            ShowPrevFrame();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            print("next frame");
            IsPlaying = false;
            ShowNextFrame(true);
            print(FrameIndex);
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            print($"PlayPause {IsPlaying}");
            IsPlaying = !IsPlaying;
        }
        if (Input.GetKeyDown(KeyCode.Tab)) {
            print($"ShowHideSubject {IsShowingSubjectScreen}");
            ToggleSubjectScreen();
        }
    }

    public void TrialStartedTriggerAction() {
        PlayerAudio.instance.PlayStartClip();
        sessionStatus.text = "Cue Shown";
        fadeController.Alpha = 0;
    }

    public void CueOffsetTriggerAction() {
        sessionStatus.text = "TrialRunning";
        fadeController.Alpha = 0;
    }

    public void TrialEndedTriggerAction() {
        sessionStatus.text = "Trial Success";
        SimulateFade();
    }

    public void TimeoutTriggerAction() {
        PlayerAudio.instance.PlayStartClip();
        sessionStatus.text = "Trial Timeout";
        SimulateFade();
    }

    public void ExperimentVersionTriggerAction() {
        //do nothing
    }

    public void NoTriggerAction() {
        //do nothing
    }

    public void DefaultAction() {
        sessionStatus.text = "Impossible to happen";
    }

    public void SimulateFade() {
        float alpha = 0;
        Trial currentTrial = trials[TrialIndex];
        int triggerFrameNum = currentTrial.GetFrameNumAtTrigger(SessionTrigger.TrialEndedTrigger);

        if (triggerFrameNum != -1) {
            for (int i = triggerFrameNum; i < FrameIndex && alpha < 1000f; i++) {
                alpha += currentTrial.GetFrameAt(i).DataCount;
            }

            //approximate to check if current frame shoule be starting to fade in
            int approxFadein = currentTrial.GetFrameCount() - (int)(1000 * ((FrameIndex - triggerFrameNum) / (alpha)));

            if (FrameIndex > approxFadein && alpha != 0) {
                alpha = 0f;
                for (int i = currentTrial.GetFrameCount() - 1; i > FrameIndex && alpha < 1000f; i--) {
                    alpha += currentTrial.GetFrameAt(i).DataCount;
                }
            }
        }

        fadeController.Alpha = alpha * 0.001f;
    }
}
