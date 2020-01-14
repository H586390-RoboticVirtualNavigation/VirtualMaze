using RockVR.Video;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class DataViewer : BasicGUIController, CueController.ITriggerActions {
    private const int Main_Screen = 0;
    private const int Sub_Screen = 1;
    private List<Trial> trials = new List<Trial>();
    private RewardArea[] rewards = null;
    private bool isRecording = false;

    public AudioSource audioSource;

    [SerializeField]
    private GameObject binWallPrefab = null;

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
    public CanvasGroup subMenu; // only appears when trials are loaded
    public Dropdown trialSelect;
    public FadeCanvas fadeController;

    [SerializeField]
    private RecordCanvas recordCanvas = null;

    //public Text sessionStatus;
    //public Text frameNumStatus;
    //public Text trialNumStatus;
    public Text isPlayingStatus;
    public Text dataIgnoredStatus;

    [SerializeField]
    private InputField savePath = null;

    public Button recordTrialButton;

    public bool IsPlaying {
        get => _isPlaying;
        private set {
            _isPlaying = value;
            if (_isPlaying) {
                isPlayingStatus.text = "Playing";
            }
            else {
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

            recordCanvas.FrameNum = $"Frame: {_frameindex + 1}";
        }
    }

    public int TrialIndex {
        get => _trialIndex;
        set {
            _trialIndex = value;
            SelectTrial(_trialIndex);
        }
    }

    private int _frameindex = 0;
    private int _trialIndex = 0;
    private bool _isPlaying;

    [SerializeField]
    private Camera miniplayer = null;

    private void Start() {
        dataViewerGUI.SetVisibility(false);

        recordCanvas.Hide();

        processBtn.onClick.AddListener(OnProcessBtnClicked);

        trialSelect.onValueChanged.AddListener(OnTrialSelected);

        scrubber.onValueChanged.AddListener(OnScrubber);
        recordTrialButton.onClick.AddListener(StartRecording);
        subMenu.SetVisibility(false);
        savePath.text = PathConfig.SaveFolder;
    }

    private void StartRecording() {
        if (isRecording) {
            return;
        }
        else {
            if (!string.IsNullOrEmpty(savePath.text) && FileBrowser.IsValidFolder(savePath.text)) {
                SetInputFieldValid(savePath, true);
                PathConfig.saveFolder = savePath.text;
                StartCoroutine(Record());
            }
            else {
                SetInputFieldValid(savePath, false);
            }

        }
    }

    private IEnumerator Record() {
        bool isRecordingValid = false;
        int retries = 0, maxRetries = 5;

        while (!isRecordingValid && retries < maxRetries) {
            if (retries > 0) {
                Console.Write($"Recording Trial {TrialIndex + 1}, Retrying {retries}/{maxRetries}");
            }
            else {
                Console.Write($"Recording Trial {TrialIndex + 1}");
            }
            // move to start of the trial
            FrameIndex = 0;
            videoCaptureCtrl.StartCapture();
            isRecording = true;
            //return command to 
            yield return null;

            //play frame 1 again to record the audio
            FrameIndex = 0;
            IsPlaying = true;
            while (IsPlaying) {
                yield return null;
            }
            videoCaptureCtrl.StopCapture();

            //in windows, this code only checks the size of the screen captures
            // but is is also a good indicator as size of the failed capture is roughly 262kB only

            isRecordingValid = IsRecordingValid(PathConfig.lastVideoFile);

            if (!isRecordingValid) {
                retries++;
            }
            yield return null;

        }
        subjectView.targetTexture = null;

        if (!isRecordingValid) {
            Console.Write($"Recording Trial {TrialIndex + 1} Failed! Please try again.");
            isRecording = false;
        }
        else {
            videoCaptureCtrl.MuxComplete += OnMuxComplete;
            Console.Write($"Processing");
            yield return new WaitUntil(() => !isRecording);
            Console.Write($"Recording Trial {TrialIndex + 1} Completed @ {PathConfig.lastVideoFile}");
        }
    }

    private void OnMuxComplete() {
        videoCaptureCtrl.MuxComplete -= OnMuxComplete;
        isRecording = false;
    }

    private bool IsRecordingValid(string filePath) {
        FileInfo info = new FileInfo(filePath);
        int thresholdMB = 1 * 1024 * 1024;
        return info.Length > thresholdMB;
    }

    private void OnTrialSelected(int value) {
        if (isRecording) {
            trialSelect.value = TrialIndex;
        }
        else {
            TrialIndex = value;
        }
    }

    private void FillTrialOptions(List<Dropdown.OptionData> options) {
        int counter = 1;
        foreach (Trial t in trials) {
            options.Add(new Dropdown.OptionData($"{counter:000}: {t.TrialName}"));
            counter++;
        }
    }

    private void OnProcessBtnClicked() {
        if (isRecording) {
            return;
        }

        StartCoroutine(LoadData());
    }

    private IEnumerator LoadData() {
        string path = dataFileField.text;

        if (!string.IsNullOrEmpty(path)) {
            if (File.Exists(path)) {
                SetInputFieldValid(dataFileField, true);
                SpikeTimeParser spikeReader = TryCreateSpikeTrainFile(spikeTrainFileField.text);

                trials.Clear();
                Console.Write("Loading Trials......");
                yield return null;

                yield return PrepareScene();

                RaycastDataLoader.Load(path, trials, spikeReader);

                trialSelect.ClearOptions();
                FillTrialOptions(trialSelect.options);

                TrialIndex = 0;
                trialSelect.value = -1;

                subMenu.SetVisibility(true);
                recordCanvas.Show();
                Console.Write($"{path} loaded");
            }
            else {
                SetInputFieldValid(dataFileField, false);
            }
        }
    }

    private SpikeTimeParser TryCreateSpikeTrainFile(string path) {
        if (!string.IsNullOrEmpty(path)) {
            if (File.Exists(path)) {
                SetInputFieldValid(spikeTrainFileField, true);
                return new SpikeTimeParser(path);
            }
            else {
                SetInputFieldValid(spikeTrainFileField, false);
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
    }

    private void OnScrubber(float value) {
        if (isRecording)
            return;

        if (value == _frameindex) {
            return;
        }

        IsPlaying = false;
        int toFrame = Convert.ToInt32(value);
        FrameIndex = toFrame;
    }

    // Update is called once per frame 
    void Update() {
        if (IsVisible() && !isRecording) {
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
                miniplayer.enabled = true;
            }
            else {
                subjectView.targetDisplay = 0;
                miniplayer.enabled = false;
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

        recordCanvas.TrialNum = $"Trial: {_trialIndex + 1}";

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

    List<Vector2> frameGazeCache = new List<Vector2>();

    DoubleTeeBinMapper m = new DoubleTeeBinMapper(40);

    public void ShowFrame(Trial trial, int frameNum) {
        BinWallManager.ResetWalls();
        pool.ClearScreen();

        Frame frame = trial.GetFrameAt(frameNum);

        audioSource.PlayOneShot(frame.GetAudioClip());

        if (frame.Config != null) {
            RobotMovement.MoveRobotTo(robot, frame.Config);
            cueController.UpdatePosition(robot);
        }

        dataIgnoredStatus.gameObject.SetActive(frame.Config == null);
        Image i = null;

        foreach (PlaybackData data in frame) {
            if (data is PlaybackSample sample) {
                frameGazeCache.Add(sample.gaze);
                Image temp = pool.AddGazePoint(gazeRect, subjectView, sample.gaze);
                if (temp != null) {
                    i = temp;
                }
            }
        }
        if (i != null) {
            i.color = Color.red;
        }

        BinWallManager.DisplayGazes(frameGazeCache, subjectView, binWallPrefab, m);

        SimulateFade();
        CueController.ProcessTrigger(trial.GetLatestTriggerAtFrame(frameNum), cueController, this);
        recordCanvas.TimeStatus = trial.GetDurationOf(_frameindex);
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
            if (IsPlaying && FrameIndex == trials[TrialIndex].GetFrameCount() - 1) {
                FrameIndex = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.Tab)) {
            print($"ShowHideSubject {IsShowingSubjectScreen}");
            ToggleSubjectScreen();
        }
    }

    public void TrialStartedTriggerAction() {
        if (FrameIndex == 0) {
            PlayerAudio.instance.PlayStartClip();
        }

        recordCanvas.TrialStatus = "Cue Shown";
        fadeController.Alpha = 0;
    }

    public void CueOffsetTriggerAction() {
        recordCanvas.TrialStatus = "TrialRunning";
        fadeController.Alpha = 0;
    }

    public void TrialEndedTriggerAction() {
        recordCanvas.TrialStatus = "Trial Success";
    }

    public void TimeoutTriggerAction() {
        PlayerAudio.instance.PlayStartClip();
        recordCanvas.TrialStatus = "Trial Timeout";
    }

    public void ExperimentVersionTriggerAction() {
        //do nothing
    }

    public void NoTriggerAction() {
        //do nothing
    }

    public void DefaultAction() {
        recordCanvas.TrialStatus = "Impossible to happen";
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
