using System;
using System.Threading;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DataViewer : BasicGUIController, CueController.ITriggerActions {
    private const int Main_Screen = 0;
    private const int Sub_Screen = 1;
    private List<Trial> trials = new List<Trial>();
    private RewardArea[] rewards = null;

    public AudioSource hahasource;

    //Drag and drop
    public CanvasGroup gui;
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
                fadeController.Alpha = 0f;
            }
            else if (FrameIndex >= trials[TrialIndex].GetFrameCount() - 1) {
                fadeController.Alpha = 1f;
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
        processBtn.onClick.AddListener(OnProcessBtnClicked);

        trialSelect.onValueChanged.AddListener(OnTrialSelected);

        scrubber.onValueChanged.AddListener(OnScrubber);
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
        BasicLevelController controller = GameObject.FindWithTag(Tags.LevelController).GetComponent<BasicLevelController>();
        rewards = controller.rewards;

        TrialIndex = 0;
    }

    private void OnScrubber(float value) {
        if (value == _frameindex) {
            return;
        }

        IsPlaying = false;
        int toFrame = Convert.ToInt32(value);
        FrameIndex = toFrame;
        //ShowFrame(trials[TrialIndex], toFrame);
    }

    // Update is called once per frame 
    void Update() {
        if (IsVisible()) {
            ProcessKeyPress();
        }
        if (IsPlaying) {
            ShowNextFrame(false);
        }
    }

    private void ToggleSubjectScreen() {
        if (trials.Count > 0) {
            gui.SetVisibility(IsShowingSubjectScreen);
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

        print(rewards);

        if (rewards != null) {
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
                //ShowFrame(trial, Frameindex);
            }
            else {
                IsPlaying = false;
            }
        }
    }

    private void ShowPrevFrame() {
        if (FrameIndex > 0) {
            FrameIndex--;

            //ShowFrame(trials[TrialIndex], Frameindex);
        }
        else {
            IsPlaying = false;
        }
    }

    public void ShowFrame(Trial trial, int frameNum) {
        pool.ClearScreen();

        Frame frame = trial.GetFrameAt(frameNum);


        hahasource.PlayOneShot(frame.GetAudioClip());

        CueController.ProcessTrigger(trial.GetLatestTriggerAtFrame(frameNum), cueController, this);

        RobotMovement.MoveRobotTo(robot, frame.Config);

        foreach (PlaybackData data in frame) {
            if (data is PlaybackSample sample) {
                pool.AddGazePoint(gazeRect, subjectView, sample.gaze);
            }
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

    private void ProcessKeyPress() {
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
        sessionStatus.text = "Cue Shown";
    }

    public void CueOffsetTriggerAction() {
        sessionStatus.text = "TrialRunning";
    }

    public void TrialEndedTriggerAction() {
        sessionStatus.text = "Trial Success";
    }

    public void TimeoutTriggerAction() {
        sessionStatus.text = "Trial Timeout";
    }

    public void ExperimentVersionTriggerAction() {
        //do nothing
    }

    public void NoTriggerAction() {
        //do nothing
    }

    public void DefaultAction() {
        sessionStatus.text = "Impossible to show";
    }
}

public class Trial {
    private List<Frame> frames = new List<Frame>();
    private Dictionary<int, SessionTrigger> triggerMap = new Dictionary<int, SessionTrigger>();

    private Frame currentFrame = null;
    public int RewardIndex { get; private set; }
    private string _trialName;

    public string TrialName {
        get => _trialName;
        set {
            _trialName = value;
            RewardIndex = (_trialName[_trialName.Length - 1] - '0') - 1;
            Debug.Log(RewardIndex);
        }
    }

    public Trial(string trialName) {
        TrialName = trialName;
    }

    public void AddData(PlaybackData playBackData) {
        if (currentFrame == null) {
            NextFrame(null);
        }
        //event data not added into trial as they have the same time as the sample before or after it
        if (playBackData is PlaybackEvent ev) {
            triggerMap.Add(frames.Count - 1, ev.trigger);
        }
        else {
            currentFrame.AddData(playBackData);
        }
    }

    public void NextFrame(RobotConfiguration config = null) {
        if (currentFrame != null) {
            currentFrame.Config = config;
        }

        currentFrame = new Frame();
        frames.Add(currentFrame);
    }

    public Frame GetFrameAt(int i) {
        //clamp values
        int index = Math.Min(frames.Count - 1, i);
        index = Math.Max(i, 0);

        return frames[index];
    }

    public int GetFrameCount() {
        return frames.Count;
    }

    public SessionTrigger GetLatestTriggerAtFrame(int frameNum) {
        int latestIndex = -1;
        foreach (int triggerIndex in triggerMap.Keys) {
            if (frameNum >= triggerIndex) {
                latestIndex = Math.Max(latestIndex, triggerIndex);
            }
        }

        if (triggerMap.TryGetValue(latestIndex, out SessionTrigger value)) {
            return value;
        }
        else {
            return SessionTrigger.NoTrigger;
        }
    }
}

public class Frame : IEnumerable<PlaybackData> {
    List<PlaybackData> fixations = new List<PlaybackData>();
    public RobotConfiguration Config { get; set; }

    public int DataCount { get => fixations.Count; }

    private uint startTime;
    private uint endTime;

    private AudioClip spikeTrain = null;

    private const int Sampling_Rate = 48000;
    private const int Samples_Per_Millis = Sampling_Rate / 1000;

    private static float[] tone = null;
    private static float[] negTone = null;


    public Frame() {
        if (tone == null) {
            tone = new float[Samples_Per_Millis];
            negTone = new float[Samples_Per_Millis];
            for (int i = 0; i < Samples_Per_Millis; i++) {
                tone[i] = 1f;
                negTone[i] = -1f;
            }
        }
    }

    public void AddData(PlaybackData playBackData) {
        if (fixations.Count == 0) {
            startTime = playBackData.timestamp;
        }
        fixations.Add(playBackData);

        endTime = playBackData.timestamp;
    }

    public AudioClip GetAudioClip() {
        if (spikeTrain == null) {
            if (fixations.Count != 0) {
                spikeTrain = AudioClip.Create(ToString(), fixations.Count * Samples_Per_Millis, 1, Sampling_Rate, false);
            }

            for (int i = 0; i < fixations.Count; i++) {
                if (fixations[i].HasSpike) {
                    spikeTrain.SetData(tone, Samples_Per_Millis * i);
                }

            }
        }
        return spikeTrain;
    }

    IEnumerator<PlaybackData> IEnumerable<PlaybackData>.GetEnumerator() {
        return ((IEnumerable<PlaybackData>)fixations).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable<PlaybackData>)fixations).GetEnumerator();
    }
}

public abstract class PlaybackData {
    public readonly DataTypes type;
    public readonly uint timestamp;
    public bool HasSpike;

    public PlaybackData(DataTypes type, uint timestamp) {
        this.type = type;
        this.timestamp = timestamp;
    }
}

public class PlaybackEvent : PlaybackData {
    public readonly string message;
    public readonly SessionTrigger trigger;

    public PlaybackEvent(string message, SessionTrigger trigger, DataTypes type, uint timestamp) : base(type, timestamp) {
        this.message = message;
        this.trigger = trigger;
    }
}

public class PlaybackSample : PlaybackData {
    public readonly Vector2 gaze;
    public readonly Vector3 pos;
    public readonly float rotY;

    public PlaybackSample(Vector2 gaze, Vector3 pos, float rotY, DataTypes type, uint timestamp) : base(type, timestamp) {
        this.gaze = gaze;
        this.pos = pos;
        this.rotY = rotY;
    }
}