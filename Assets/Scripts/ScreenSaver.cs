using Eyelink.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// TODO cancel button
/// </summary>

public class ScreenSaver : BasicGUIController {
    private const int Frame_Per_Batch = 200;

    private const int No_Missing = 0x0;
    private const int Ignore_Data = 0x1;
    private const int Approx_From_Session = 0x2;

    //acceptable time difference between edf and session triggers for approximation of missing trigger
    private const int Time_Diff_Accept = 20;

    //if each frame is allocated
    private const int Fixation_Per_Frame_Error = 150;

    //UI objects
    public FileSelector eyeLinkFileInput;
    public FileSelector sessionInput;
    public FileSelector folderInput;
    public InputField from;
    public InputField to;
    public Text sessionInfo;

    public GazePointPool gazePointPool;

    // Camera which renders the view of the subject
    public Camera viewport;

    public Transform robot;
    public FadeCanvas fadeController;

    // Used to check if UI in the canvas is hit.
    public GraphicRaycaster cueCaster;

    public CueController cueController;

    public RectTransform GazeCanvas;
    public Slider progressBar;

    private TestTrigger expected = TestTrigger.TrialStartedTrigger;

    private bool isloaded = false;
    private WaitUntil IsSceneLoaded;

    private static readonly Vector2Int minBound = Vector2Int.zero;
    private static readonly Vector2Int maxBound = new Vector2Int(1920, 1080);

    private void Awake() {
        IsSceneLoaded = new WaitUntil(() => isloaded);

        eyeLinkFileInput.OnPathSelected.AddListener(ChooseEyelinkFile);
        sessionInput.OnPathSelected.AddListener(ChooseSession);
        folderInput.OnPathSelected.AddListener(ChooseFolder);

        List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();

        // starts at 1 since index 0 is Start Scene
        for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++) {
            string sceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
            list.Add(new Dropdown.OptionData(sceneName));
        }
    }

    private void Start() {
        if (Application.isEditor) { //for testing purposes.
            ChooseEyelinkFile(@"D:\Desktop\NUS\FYP\rawdata\20180824");
            ChooseSession(@"D:\Desktop\NUS\FYP\rawdata\20180824");
            ChooseFolder(@"D:\Documents\GitHub\VirtualMaze\out");
        }
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode mode) {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        isloaded = true;
    }

    public void OnRender() {
        // check if file exists
        string sessionPath = sessionInput.text;
        if (!Directory.Exists(sessionPath) && !File.Exists(sessionPath)) {
            Debug.LogError($"{sessionPath} does not exist");
            return;
        }

        //check if directory exists
        string toFolderPath = folderInput.text;
        if (!Directory.Exists(toFolderPath)) {
            Debug.LogError($"{toFolderPath} does not exist");
            return;
        }

        string edfPath = eyeLinkFileInput.text;
        if (!File.Exists(edfPath)) { //check if file exist
            Debug.LogError($"{edfPath} does not exist");
            return;
        }

        StartCoroutine(ProcessSessionDataTask(sessionInput.text, eyeLinkFileInput.text, folderInput.text));
    }

    private bool IsCsvFile(string filePath) {
        return IsFileWithExtension(filePath, ".csv");
    }

    private bool IsTxtFile(string filePath) {
        return IsFileWithExtension(filePath, ".txt");
    }
    private bool IsEdfFile(string filePath) {
        return IsFileWithExtension(filePath, ".edf");
    }

    private bool isMatFile(string filePath) {
        return IsFileWithExtension(filePath, ".mat");
    }

    private bool IsFileWithExtension(string filePath, string extension) {
        return Path.GetExtension(filePath).Equals(extension, StringComparison.InvariantCultureIgnoreCase);
    }

    private IEnumerable<string> GetSessionFilesFromDirectory(string dirPath) {
        return Directory.EnumerateFiles(dirPath, "*.txt");
    }

    void ChooseSession(string filePath) {
        if (Directory.Exists(filePath)) {
            IEnumerable<string> filesToProcess = GetSessionFilesFromDirectory(filePath);
            sessionInfo.text = "";
            Console.Write($"Processing in the following order of:\n\n{string.Join("\n", filesToProcess)}");
            SetInputFieldValid(sessionInput);
            sessionInput.text = filePath;
            return;
        }
        else if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) {
            return;
        }


        bool success = false;
        int numFrames = 0;
        SessionContext context = null;

        if (IsTxtFile(filePath)) {
            try {
                SessionReader.ExtractInfo(filePath, out context, out numFrames);
                success = true;
            }
            catch (FormatException ex) {
                success = false;
                Debug.LogError(ex);
            }
        }
        else if (isMatFile(filePath)) {
            success = true;
        }

        if (success) {
            sessionInput.text = filePath;
            SetInputFieldValid(sessionInput);
            sessionInfo.text = $"{numFrames} frames";
            Console.Write(context?.ToJsonString(true));
        }
        else {
            SetInputFieldInvalid(sessionInput);
            Console.Write("Invalid Session File Detected, Unsupported File Type or Data");
        }
    }

    void ChooseEyelinkFile(string filePath) {
        if (string.IsNullOrEmpty(filePath)) return;

        eyeLinkFileInput.text = filePath;

        if (File.Exists(filePath)) {
            SetInputFieldValid(eyeLinkFileInput);
        }
        else {
            SetInputFieldInvalid(eyeLinkFileInput);
        }
    }

    void ChooseFolder(string file) {
        if (string.IsNullOrEmpty(file)) { return; }

        folderInput.text = file;
        if (Directory.Exists(file)) {
            SetInputFieldValid(folderInput);
        }
        else {
            SetInputFieldInvalid(folderInput);
        }
    }

    /// <summary>
    /// Determines which session file or the other file needs to load more data into their queue due to missing triggers
    /// </summary>
    /// <param name="sTrigger">Current trigger of the Session Logs</param>
    /// <param name="edfTrigger">Current Trigger of the EDF file</param>
    /// <returns>True if session file should be processed again to the next trigger</returns>
    private bool ShouldSessionFileCatchUp(SessionTrigger sTrigger, SessionTrigger edfTrigger) {
        //if one trigger is either trial ended or trial timeout and the other is Trial started
        bool premise1 = sTrigger >= SessionTrigger.TrialEndedTrigger && edfTrigger == SessionTrigger.TrialStartedTrigger;
        bool premise2 = edfTrigger >= SessionTrigger.TrialEndedTrigger && sTrigger == SessionTrigger.TrialStartedTrigger;

        bool premise3 = edfTrigger >= SessionTrigger.TrialEndedTrigger && sTrigger >= SessionTrigger.TrialEndedTrigger;

        if (premise1 || premise2 || premise3) {
            return sTrigger > edfTrigger;
        }
        else {
            return sTrigger < edfTrigger;
        }
    }

    private void PrepareScene(string sceneName) {
        if (SceneManager.GetActiveScene().name.Equals(sceneName)) {
            isloaded = true;
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        isloaded = false;
        SceneManager.LoadScene(sceneName);
    }

    public IEnumerator ProcessSessionDataTask(string sessionPath, string edfPath, string toFolderPath) {
        //Setup
        fadeController.gameObject.SetActive(false);
        EyeDataReader eyeReader = null;

        if (IsEdfFile(edfPath)) {
            eyeReader = new EDFReader(edfPath, out int errVal);
            if (errVal != 0) { //check if file can be parsed by library
                string error = $"Unable to open .edf file";
                Debug.LogError(error);
                Console.WriteError(error);
                yield break;
            }
        }
        else if (isMatFile(edfPath)) {
            try {
                eyeReader = new EyeMatReader(edfPath);
            }
            catch (Exception e) {
                Debug.LogException(e);
                Console.WriteError("Unable to open eye data mat file.");
                eyeReader = null;
            }
        }
        else {
            eyeReader = new EyeCsvReader(edfPath);
        }

        if (eyeReader == null) {
            yield break;
        }

        bool isMultipleSession = Directory.Exists(sessionPath);

        gazePointPool?.PreparePool();

        progressBar.value = 0;
        progressBar.gameObject.SetActive(true);

        cueController.SetMode(CueController.Mode.Recording);

        //process data
        if (isMultipleSession) {
            foreach (string path in GetSessionFilesFromDirectory(sessionPath)) {
                PrepareScene("Double Tee");

                yield return IsSceneLoaded;
                isloaded = false;

                string filename = $"{Path.GetFileNameWithoutExtension(path)}_{Path.GetFileNameWithoutExtension(edfPath)}.csv";

                using (RayCastRecorder recorder = new RayCastRecorder(toFolderPath, filename)) {
                    yield return ProcessSession(path, eyeReader, recorder);
                    progressBar.value = 0;
                    expected = TestTrigger.TrialStartedTrigger;
                }
            }
        }
        else {
            PrepareScene("Double Tee");

            string filename = $"{Path.GetFileNameWithoutExtension(sessionPath)}_{Path.GetFileNameWithoutExtension(edfPath)}.csv";

            yield return IsSceneLoaded;
            isloaded = false;

            using (RayCastRecorder recorder = new RayCastRecorder(toFolderPath, filename)) {
                yield return ProcessSession(sessionPath, eyeReader, recorder);
                expected = TestTrigger.TrialStartedTrigger;
            }
        }

        //cleanup
        //SceneManager.LoadScene("Start");
        fadeController.gameObject.SetActive(true);
        progressBar.gameObject.SetActive(false);
        cueController.SetMode(CueController.Mode.Experiment);
        //SessionStatusDisplay.ResetStatus();
    }

    private bool HasEdfSessionEnded(MessageEvent msgEvent, SessionTrigger trigger) {
        //there exits edf files with multiple sessions in them, each session is divided by a ExperimentVersionTrigger
        return msgEvent == null || msgEvent.dataType == DataTypes.NO_PENDING_ITEMS || trigger == SessionTrigger.ExperimentVersionTrigger;
    }

    private decimal LoadAndFix(Queue<SessionData> sessionFrames, ISessionDataReader sessionReader, Queue<AllFloatData> fixations, EyeDataReader eyeReader, Queue<MessageEvent> missingTriggers) {
        decimal sessionEventPeriod = LoadToNextTriggerSession(sessionReader, sessionFrames, out SessionData sessionTrigger);
        uint edfEventPeriod = LoadToNextTriggerEdf(eyeReader, fixations, missingTriggers, out MessageEvent data, out SessionTrigger edfTrigger);

        print($"ses: { sessionEventPeriod: 0.00}, edf: { edfEventPeriod}");
        return (sessionEventPeriod - edfEventPeriod);
    }

    private decimal LoadAndApproximate(Queue<SessionData> sessionFrames, ISessionDataReader sessionReader, Queue<AllFloatData> fixations, EyeDataReader eyeReader, out int status, out string reason) {
        decimal sessionEventPeriod = LoadToNextTriggerSession(sessionReader, sessionFrames, out SessionData sessionData);
        uint edfEventPeriod = LoadToNextTriggerEdf(eyeReader, fixations, out MessageEvent edfdata, out SessionTrigger edfTrigger);

        SessionTrigger missingTrigger = SessionTrigger.NoTrigger;
        status = No_Missing;
        reason = null;

        //check and account for missing trigger
        while (sessionData.trigger != edfTrigger && !HasEdfSessionEnded(edfdata, edfTrigger) && sessionReader.HasNext) {
            string errorMessage = $"Missing trigger detected at approx {fixations.Peek().time + sessionEventPeriod}. session: {sessionData.trigger}| edf: {edfTrigger}";
            Console.WriteError(errorMessage);
            Debug.LogWarning(errorMessage);

            //Assuming that the previous iteration is synchronized
            if (ShouldSessionFileCatchUp(sessionData.trigger, edfTrigger)) {
                //session file should catch up
                sessionEventPeriod += LoadToNextTriggerSession(sessionReader, sessionFrames, out sessionData);
                missingTrigger = edfTrigger;
            }
            else {
                //edfFile should catch up
                edfEventPeriod += LoadToNextTriggerEdf(eyeReader, fixations, out edfdata, out edfTrigger);
                missingTrigger = sessionData.trigger;
            }
        }

        decimal excessTime = (sessionEventPeriod - edfEventPeriod);

        // if missing trigger detected and the time difference between the 2 files is less than 20ms
        if (SessionTrigger.NoTrigger != missingTrigger && Math.Abs(excessTime) > Time_Diff_Accept) {
            status = Ignore_Data;
            reason = "Unable to approximate missing trigger.";
        }
        else if (SessionTrigger.NoTrigger != missingTrigger) {
            status = Approx_From_Session;
        }

        print($"ses: { sessionEventPeriod:F4}, edf: { edfEventPeriod:F4}, excess: {excessTime:F4} | {status} |{fixations.Peek().ToString()} {sessionFrames.Count}");
        return excessTime;
    }

    private ISessionDataReader CreateSessionReader(string filePath) {
        try {
            switch (Path.GetExtension(filePath).ToLower()) {
                case ".txt":
                    return new SessionReader(filePath);
                case ".mat":
                    return new MatSessionReader(filePath);
                default:
                    Debug.LogWarning($"File extension not supported{filePath}");
                    return null;
            }
        }
        catch (Exception e) {
            Debug.LogException(e);
            return null;
        }
    }

    private IEnumerator ProcessSession(string sessionPath, EyeDataReader eyeReader, RayCastRecorder recorder) {
        int frameCounter = 0;
        int trialCounter = 1;

        using (ISessionDataReader sessionReader = CreateSessionReader(sessionPath)) {
            if (sessionReader == null) {
                yield break;
            }

            //Move to first Trial Trigger
            AllFloatData data = PrepareFiles(sessionReader, eyeReader, SessionTrigger.TrialStartedTrigger);

            Queue<SessionData> sessionFrames = new Queue<SessionData>();
            Queue<AllFloatData> fixations = new Queue<AllFloatData>();
            Queue<MessageEvent> missingevents = new Queue<MessageEvent>();

            LoadMissingTriggers("", missingevents);

            //feed in current Data due to preparation moving the data pointer forward
            fixations.Enqueue(data);

            int debugMaxMissedOffset = 0;

            while (sessionReader.HasNext) {
                //add current to buffer since sessionData.timeDelta is the time difference from the previous frame.
                sessionFrames.Enqueue(sessionReader.CurrentData);

                decimal excessTime;
                int status = No_Missing;
                string reason = null;

                if (missingevents.Count > 0) {
                    excessTime = LoadAndFix(sessionFrames, sessionReader, fixations, eyeReader, missingevents);
                }
                else {
                    excessTime = LoadAndApproximate(sessionFrames, sessionReader, fixations, eyeReader, out status, out reason);
                }

                decimal timepassed = fixations.Peek().time;

                decimal timeOffset = excessTime / (sessionFrames.Count - 1);

                print($"timeError: {excessTime}|{timeOffset} for {sessionFrames.Count} frames @ {sessionReader.CurrentIndex} and {fixations.Count} fix");

                uint gazeTime = 0;

                decimal debugtimeOffset = 0;

                while (sessionFrames.Count > 0 && fixations.Count > 0) {
                    SessionData sessionData = sessionFrames.Dequeue();

                    decimal period;
                    if (sessionFrames.Count > 0) {
                        //peek since next sessionData holds the time it takes from this data to the next
                        period = (sessionFrames.Peek().timeDeltaMs) - timeOffset;
                    }
                    else {
                        //use current data's timedelta to approximate
                        period = (sessionData.timeDeltaMs) - timeOffset;
                    }

                    // does not matter if trigger in session file is missing since "true" timing is based on edf file
                    if (status == Approx_From_Session) {
                        //approx edf trigger from session file
                        SessionTrigger approxTrigger = sessionData.trigger;
                        if (approxTrigger != SessionTrigger.NoTrigger && ((ConvertToTestTrigger(approxTrigger) | expected) == expected)) {
                            expected = ProcessTrigger(sessionData.trigger, expected, cueController);
                            //if trigger is approximated, the apporximated trigger will always be the end of a frame because VirtualMaze logs
                            // once every update() which represents a frame
                            recorder.FlagEvent($"Approximated Trigger {sessionData.flag}");
                        }
                    }

                    debugtimeOffset += timeOffset;

                    timepassed += period;

                    MoveRobotTo(robot, sessionData);

                    while (gazeTime <= timepassed && fixations.Count > 0) {
                        AllFloatData currData = fixations.Dequeue();
                        gazeTime = currData.time;

                        bool isLastSampleInFrame = gazeTime > timepassed;

                        if (status == Ignore_Data) {
                            IgnoreData(currData, recorder, reason, isLastSampleInFrame);
                        }
                        else if (ProcessData(currData, recorder, isLastSampleInFrame) == SessionTrigger.TrialStartedTrigger) {
                            trialCounter++;
                            SessionStatusDisplay.DisplayTrialNumber(trialCounter);
                        }

                        //update UI objects
                        if (currData is MessageEvent) {
                            yield return new WaitForEndOfFrame();
                        }
                    }

                    frameCounter++;
                    frameCounter %= Frame_Per_Batch;
                    if (frameCounter == 0) {
                        progressBar.value = sessionReader.ReadProgress;
                        yield return null;
                    }
                    gazePointPool?.ClearScreen();
                }

                Debug.Log($"ses: {sessionFrames.Count}| fix: {fixations.Count}, timestamp {gazeTime:F4}, timepassed{timepassed:F4}");
                decimal finalExcess = gazeTime - timepassed;

                Debug.Log($"FINAL EXCESS: {finalExcess} | {finalExcess + sessionReader.CurrentData.timeDeltaMs} || {sessionReader.CurrentData.timeDeltaMs}");
                Debug.Log($"Frame End total time offset: {debugtimeOffset}");

                //clear queues to prepare for next trigger
                sessionFrames.Clear();

                if (Math.Abs(finalExcess) > 3) {
                    if (status != Ignore_Data)
                        Debug.LogError("excess to large");
                    else
                        Debug.LogError("excess to large so data ignored");
                }

                if (fixations.Count > 0) {
                    Debug.LogWarning($"{fixations.Count} fixations assumed to belong to next trigger");
                    while (fixations.Count > 0) {
                        debugMaxMissedOffset = Math.Max(fixations.Count, debugMaxMissedOffset);
                        //excess frames are taken to be belonging to the next frame, therefore is not last sample in frame
                        if (ProcessData(fixations.Dequeue(), recorder, false) == SessionTrigger.TrialStartedTrigger) {
                            trialCounter++;
                            SessionStatusDisplay.DisplayTrialNumber(trialCounter);
                        }
                    }
                }
            }

            Debug.LogError(debugMaxMissedOffset);
        }
    }

    private void LoadMissingTriggers(string v, Queue<MessageEvent> missingevents) {
        //missingevents.Enqueue(new MessageEvent(1822017, "Cue Offset 25", DataTypes.MESSAGEEVENT));
        //missingevents.Enqueue(new MessageEvent(2015432, "End Trial 34", DataTypes.MESSAGEEVENT));
        //missingevents.Enqueue(new MessageEvent(2078579, "Start Trial 11", DataTypes.MESSAGEEVENT));
        //missingevents.Enqueue(new MessageEvent(2090559, "Cue Offset 25", DataTypes.MESSAGEEVENT));
        //missingevents.Enqueue(new MessageEvent(2203264, "Timeout 41", DataTypes.MESSAGEEVENT));
        //missingevents.Enqueue(new MessageEvent(2209282, "Start Trial 11", DataTypes.MESSAGEEVENT));
        //missingevents.Enqueue(new MessageEvent(2210318, "Cue Offset 21", DataTypes.MESSAGEEVENT));
    }

    private SessionTrigger ProcessData(AllFloatData data, RayCastRecorder recorder, bool isLastSampleInFrame) {
        switch (data.dataType) {
            case DataTypes.SAMPLE_TYPE:
                Fsample fs = (Fsample)data;
                if (InScreenBounds(fs.rawRightGaze)) {
                    RaycastToScene(fs.RightGaze, out string objName, out Vector2 relativePos, out Vector3 objHitPos, out Vector3 gazePoint);
                    recorder.WriteSample(data.dataType, data.time, objName, relativePos, objHitPos, gazePoint, fs.rawRightGaze, robot.position, robot.rotation.eulerAngles.y, isLastSampleInFrame);

                    gazePointPool?.AddGazePoint(GazeCanvas, viewport, fs.RightGaze);
                }
                else {
                    //ignore if gaze is out of bounds
                    recorder.IgnoreSample(data.dataType, data.time, fs.rawRightGaze, robot.position, robot.rotation.eulerAngles.y, isLastSampleInFrame);
                }
                return SessionTrigger.NoTrigger;
            case DataTypes.MESSAGEEVENT:
                MessageEvent fe = (MessageEvent)data;
                expected = ProcessTrigger(fe.trigger, expected, cueController);

                recorder.FlagEvent(fe.message);

                return fe.trigger;
            default:
                //ignore others for now
                //Debug.LogWarning($"Unsupported EDF DataType Found! ({type})");
                return SessionTrigger.NoTrigger;
        }
    }

    private bool InScreenBounds(Vector2 gazeXY) {
        return !((gazeXY.x > maxBound.x) || (gazeXY.y > maxBound.y) || (gazeXY.x < minBound.x) || (gazeXY.y < minBound.y));
    }

    private void IgnoreData(AllFloatData data, RayCastRecorder recorder, string ignoreReason, bool isLastSampleInFrame) {
        switch (data.dataType) {
            case DataTypes.SAMPLE_TYPE:
                Fsample fs = (Fsample)data;
                //record the raw gaze data
                recorder.IgnoreSample(data.dataType, data.time, fs.rawRightGaze, robot.position, robot.rotation.eulerAngles.y, isLastSampleInFrame);

                break;
            case DataTypes.MESSAGEEVENT:
                MessageEvent fe = (MessageEvent)data;
                expected = ProcessTrigger(fe.trigger, expected, cueController);
                recorder.FlagEvent(fe.message);

                break;
            default:
                //ignore others for now
                //Debug.LogWarning($"Unsupported EDF DataType Found! ({type})");
                break;
        }
    }

    [Flags]
    public enum TestTrigger {
        NoTrigger = 0x0,
        TrialStartedTrigger = 0x1,
        CueShownTrigger = 0x2,
        TrialEndedTrigger = 0x4,
        TimeoutTrigger = 0x8,
        ExperimentVersionTrigger = 0xF
    }

    /// <summary>
    /// Converts SessionTrigger to TestTrigger so that missing triggers can be identified easily.
    /// </summary>
    /// <param name="trigger">SessionTrigger to Convert</param>
    /// <returns>TestTriggerEquilavant</returns>
    private static TestTrigger ConvertToTestTrigger(SessionTrigger trigger) {
        switch (trigger) {
            case SessionTrigger.CueOffsetTrigger:
                return TestTrigger.CueShownTrigger;
            case SessionTrigger.ExperimentVersionTrigger:
                return TestTrigger.ExperimentVersionTrigger;
            case SessionTrigger.NoTrigger:
                return TestTrigger.NoTrigger;
            case SessionTrigger.TimeoutTrigger:
                return TestTrigger.TimeoutTrigger;
            case SessionTrigger.TrialEndedTrigger:
                return TestTrigger.TrialEndedTrigger;
            case SessionTrigger.TrialStartedTrigger:
                return TestTrigger.TrialStartedTrigger;
        }
        return TestTrigger.NoTrigger;
    }

    private TestTrigger ProcessTrigger(AllFloatData data) {
        if (data.dataType == DataTypes.MESSAGEEVENT) {
            return ProcessTrigger(((MessageEvent)data).trigger, expected, cueController);
        }
        return expected;
    }

    /// <summary>
    /// Processes the Trigger by showing or hiding the cues.
    /// </summary>
    /// <param name="trigger">Current Trigger</param>
    public static TestTrigger ProcessTrigger(SessionTrigger trigger, TestTrigger expected, CueController cueController) {
        TestTrigger test = ConvertToTestTrigger(trigger);

        if ((test | expected) != expected) {
            Debug.LogError($"Expected:{expected}, Received: {trigger}");
        }

        switch (trigger) {
            case SessionTrigger.CueOffsetTrigger:
                cueController.HideCue();
                cueController.ShowHint();
                SessionStatusDisplay.DisplaySessionStatus("Trial Running");
                return TestTrigger.TimeoutTrigger | TestTrigger.TrialEndedTrigger;

            case SessionTrigger.TrialStartedTrigger:
                cueController.HideHint();
                cueController.ShowCue();
                SessionStatusDisplay.DisplaySessionStatus("Showing Cue");
                return TestTrigger.CueShownTrigger;

            case SessionTrigger.TimeoutTrigger:
            case SessionTrigger.TrialEndedTrigger:
                cueController.HideAll();
                if (trigger == SessionTrigger.TimeoutTrigger) {
                    SessionStatusDisplay.DisplaySessionStatus("Time out");
                }
                else {
                    SessionStatusDisplay.DisplaySessionStatus("Trial Ended");
                }

                return TestTrigger.TrialStartedTrigger;

            case SessionTrigger.ExperimentVersionTrigger:
                SessionStatusDisplay.DisplaySessionStatus("Next Session");
                return TestTrigger.TrialStartedTrigger;

            case SessionTrigger.NoTrigger:
            //do nothing
            default:
                Debug.LogError($"Unidentified Session Trigger: {trigger}");
                return TestTrigger.NoTrigger;
        }
    }

    /// <summary>
    /// Fires a raycast into the Scene based on the sample data to determine what object the sample data is fixating upon.
    /// </summary>
    /// <param name="gazeData">Data sameple from edf file</param>
    /// <param name="objName">Name of object the gazed object</param>
    /// <param name="relativePos">Local 2D offset of from the center of the object gazed</param>
    /// <param name="objHitPos">World position of the object in the scene</param>
    /// <param name="gazePoint">World position of the point where the gaze fixates the object</param>
    /// <returns>True if an object was in the path of the gaze</returns>
    private bool RaycastToScene(Vector3 gazeData,
                                 out string objName,
                                 out Vector2 relativePos,
                                 out Vector3 objHitPos,
                                 out Vector3 gazePoint) {
        Ray r = viewport.ScreenPointToRay(gazeData);

        if (Physics.Raycast(r, out RaycastHit hit, 200)) {
            Transform objhit = hit.transform;

            objName = hit.transform.name;
            relativePos = ComputeLocalPostion(objhit, hit); ;
            objHitPos = objhit.position;
            gazePoint = hit.point;

            return false;

        }
        else {
            objName = null;
            relativePos = Vector2.zero;
            gazePoint = Vector3.zero;
            objHitPos = Vector3.zero;

            return false;
        }
    }

    private readonly Vector3[] axes = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

    private Vector3 ModelNormal(Vector3 normal) {
        Vector3 result = Vector3.up;
        float min = float.MaxValue;
        foreach (Vector3 axis in axes) {
            float sqDist = (axis - normal).sqrMagnitude;
            if (sqDist < min) {
                min = sqDist;
                result = axis;
            }
            else if (sqDist < min) {
                Debug.LogWarning($"Normal is exactly between 2 axes. Expect unknown behaviour");
            }
        }

        return result;
    }

    /// <summary>
    /// </summary>
    /// <param name="objHit">Transform of the object hit by the raycast</param>
    /// <param name="hit"></param>
    /// <returns>2D location of the point fixated by that gaze relative to the center of the image</returns>
    private Vector2 ComputeLocalPostion(Transform objHit, RaycastHit hit) {
        Vector3 hitNormal = ModelNormal(hit.normal);

        Vector3 normal;

        Vector3 dist = hit.point - objHit.position;

        if (objHit.name.ToLower().Contains("image")) {
            normal = hitNormal;
            dist = Quaternion.FromToRotation(hit.normal, normal) * dist;
        }
        else if (objHit.name.ToLower().Contains("poster")) {
            normal = ModelNormal(objHit.forward); //blue axis used as normal
        }
        else if (Mathf.Abs(hitNormal.y) == 1) { //either hit the floor or ceiling
            normal = ModelNormal(objHit.up); //green axis used as normal
        }
        else { //any other object
            normal = ModelNormal(objHit.right); //green axis used as normal
        }

        dist = Quaternion.FromToRotation(normal, Vector3.back) * dist;  //orientate for stabilization
        dist = Quaternion.FromToRotation(Vector3.back, Vector3.up) * dist; //orientate to top down veiw

        Vector2 result = new Vector2(dist.x, dist.z);

        if (normal == Vector3.forward) {
            result *= -1; //rotate 180 degrees
        }

        return result;
    }

    private AllFloatData PrepareFiles(ISessionDataReader sessionReader, EyeDataReader eyeReader, SessionTrigger firstOccurance) {
        FindNextSessionTrigger(sessionReader, firstOccurance);
        return FindNextEdfTrigger(eyeReader, firstOccurance);
    }

    /// <summary>
    /// Moves session Reader to point to the session reader.
    /// </summary>
    /// <param name="sessionReader">Session reader to move</param>
    /// <param name="trigger">SessionTrigger to move to</param>
    private void FindNextSessionTrigger(ISessionDataReader sessionReader, SessionTrigger trigger) {
        //move sessionReader to point to first trial
        while (sessionReader.Next()) {
            if (sessionReader.CurrentData.trigger == trigger) {
                MoveRobotTo(robot, sessionReader.CurrentData);
                break;
            }
        }
    }

    /// <summary>
    /// Moves the current pointer to point to the next trigger. Any data between the current point and the 
    /// next trigger is ignored.
    /// </summary>
    /// <param name="eyeReader"></param>
    /// <param name="trigger">The next trigger to find</param>
    /// <returns>Current data pointed to by the eyeReader</returns>
    private AllFloatData FindNextEdfTrigger(EyeDataReader eyeReader, SessionTrigger trigger) {
        AllFloatData data = null;

        //move edfFile to point to first trial
        bool foundNextTrigger = false;
        while (!foundNextTrigger) {
            data = eyeReader.GetNextData();

            if (data.dataType == DataTypes.MESSAGEEVENT) {
                MessageEvent ev = (MessageEvent)data;

                foundNextTrigger = ev.trigger == trigger;
            }
            else if (data.dataType == DataTypes.NO_PENDING_ITEMS) {
                foundNextTrigger = true;
            }
        }

        return data;
    }

    /// <summary>
    /// Loads data from the next data point to the next trigger (inclusive) and returns the total taken from the current 
    /// position top the next Trigger
    /// 
    /// TODO: since the time delta is time difference of the last frame, total time should include the time difference of the 
    /// next frame after the trigger.
    /// </summary>
    /// <param name="reader">The SessionReader to be read</param>
    /// <param name="frames">the Queue to store the data</param>
    /// <param name="trigger">The trigger where the loading stops</param>
    /// <returns>Total time taken from one current trigger to the next</returns>
    private decimal LoadToNextTriggerSession(ISessionDataReader reader, Queue<SessionData> frames, out SessionData data) {
        decimal totalTime = 0; // reader.CurrentData.timeDeltaMs;

        data = null;
        bool isNextEventFound = false;

        // Conditon evaluation is Left to Right and it short circuits.
        // Please do not change the order of this if conditon.
        while (!isNextEventFound && reader.Next()) {
            data = reader.CurrentData;
            frames.Enqueue(data);
            totalTime += data.timeDeltaMs;

            isNextEventFound = data.trigger != SessionTrigger.NoTrigger;
        }

        return totalTime;
    }

    private uint LoadToNextTriggerEdf(EyeDataReader reader, Queue<AllFloatData> fixations, Queue<MessageEvent> fillerEvent, out MessageEvent latest, out SessionTrigger edfTrigger) {

        bool isNextEventFound = false;

        while (!isNextEventFound) {
            AllFloatData data = reader.GetNextData();

            if (data == null) {
                isNextEventFound = true;
                continue;
            }

            DataTypes type = data.dataType;

            fixations.Enqueue(data);

            if (type == DataTypes.MESSAGEEVENT) {
                isNextEventFound = true;
                MessageEvent ev = (MessageEvent)data;
                latest = ev;
                edfTrigger = ev.trigger;
                return ev.time - fixations.Peek().time;
            }
            else if (fillerEvent != null && data.time >= fillerEvent.Peek().time) {

                isNextEventFound = true;
                MessageEvent ev = fillerEvent.Dequeue();
                latest = ev;
                edfTrigger = ev.trigger;

                fixations.Enqueue(ev);

                return ev.time - fixations.Peek().time;
            }
            else if (type == DataTypes.NO_PENDING_ITEMS) {
                break;
            }
        }

        latest = null;
        edfTrigger = SessionTrigger.NoTrigger;
        return 0;
    }

    /// <summary>
    /// Loads data from the next data point to the next trigger (inclusive) and returns the total taken from the current 
    /// position top the next Trigger
    /// </summary>
    private uint LoadToNextTriggerEdf(EyeDataReader reader, Queue<AllFloatData> fixations, out MessageEvent latest, out SessionTrigger edfTrigger) {
        return LoadToNextTriggerEdf(reader, fixations, null, out latest, out edfTrigger);
    }

    /// <summary>
    /// Positions the robot as stated in the Session file.
    /// </summary>
    /// <param name="robot">Transfrom of the robot to move</param>
    /// <param name="reader">Session data of the Object</param>
    private void MoveRobotTo(Transform robot, SessionData reader) {
        RobotMovement.MoveRobotTo(robot, reader.config);
        cueController.UpdatePosition();
    }

    private void SaveScreen(Camera cam, string filename) {
        Texture2D tex = new Texture2D(cam.pixelWidth, cam.pixelHeight, TextureFormat.RGB24, false);
        tex.ReadPixels(cam.pixelRect, 0, 0);
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        Destroy(tex);

        File.WriteAllBytes(filename, bytes);
    }
}
