using Eyelink.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// TODO cancel button
/// trial number
/// </summary>

public class ScreenSaver : BasicGUIController {
    private int framePerBatch = 100;

    //UI objects
    public InputField eyeLinkFileInput;
    public InputField sessionInput;
    public InputField folderInput;
    public FileBrowser fb;
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

    // for debugging
    public LineRenderer lineRenderer;

    private void Awake() {
        eyeLinkFileInput.onEndEdit.AddListener(ChooseEyelinkFile);
        sessionInput.onEndEdit.AddListener(ChooseSession);
        folderInput.onEndEdit.AddListener(ChooseFolder);

        List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();

        // starts at 1 since index 0 is Start Scene
        for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++) {
            string sceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
            list.Add(new Dropdown.OptionData(sceneName));
        }

        if (true) { //for testing purposes.
            ChooseEyelinkFile(@"D:\Program Files (D)\SR Research\EyeLink\EDF_Access_API\Example\181026.edf");
            ChooseSession(@"D:\Desktop\FYP Init\session01\RawData_T1-400\ShortVer.txt");
            ChooseFolder(@"D:\Documents\GitHub\VirtualMaze\out");
        }
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode mode) {
        BasicLevelController levelcontroller = FindObjectOfType<BasicLevelController>();
        levelcontroller.gameObject.SetActive(false);

        SceneManager.sceneLoaded -= OnSceneLoaded;

        StartCoroutine(ProcessSessionDataTask(sessionInput.text, eyeLinkFileInput.text, folderInput.text));
    }

    public void OnRender() {
        // check if file exists
        string sessionPath = sessionInput.text;

        if (!File.Exists(sessionPath)) {
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



        SessionReader.ExtractInfo(sessionPath, out SessionContext context, out int numframes);

        progressBar.maxValue = numframes;

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(context.trialName);
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

    private bool IsFileWithExtension(string filePath, string extension) {
        return Path.GetExtension(filePath).Equals(extension, StringComparison.InvariantCultureIgnoreCase);
    }

    public void onBrowseSession() {
        fb.OnFileBrowserExit += ChooseSession;
        if (File.Exists(sessionInput.text)) {
            fb.Show(Path.GetDirectoryName(sessionInput.text));
        }
        else {
            fb.Show(Application.dataPath);
        }
    }

    public void onBrowseEyelink() {
        fb.OnFileBrowserExit += ChooseEyelinkFile;
        if (File.Exists(eyeLinkFileInput.text)) {
            fb.Show(Path.GetDirectoryName(eyeLinkFileInput.text));
        }
        else {
            fb.Show(Application.dataPath);
        }
    }

    public void onBrowseFolder() {
        fb.OnFileBrowserExit += ChooseFolder;
        if (File.Exists(folderInput.text)) {
            fb.Show(folderInput.text);
        }
        else {
            fb.Show(Application.dataPath);
        }
    }

    void ChooseSession(string filePath) {
        fb.OnFileBrowserExit -= ChooseSession;

        sessionInput.text = filePath;
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) {
            SetInputFieldInvalid(sessionInput);
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

        if (success) {
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
        fb.OnFileBrowserExit -= ChooseEyelinkFile;
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
        fb.OnFileBrowserExit -= ChooseFolder;
        if (string.IsNullOrEmpty(file)) { return; }

        folderInput.text = file;
        if (Directory.Exists(file)) {
            folderInput.GetComponent<Image>().color = Color.green;
        }
        else {
            folderInput.GetComponent<Image>().color = Color.red;
        }
    }

    void BrowserCancel(string path) {
        fb.OnFileBrowserExit -= ChooseFolder;
        fb.OnFileBrowserExit -= ChooseSession;
    }

    void Start() {
        fb.OnFileBrowserExit += BrowserCancel;
    }

    /// <summary>
    /// Determines which session file or the other file needs to load more data into their queue due to missing triggers
    /// </summary>
    /// <param name="sTrigger">Current trigger of the Session Logs</param>
    /// <param name="edfTrigger">Current Trigger of the EDF file</param>
    /// <returns>True if session file should be processed again to the next trigger</returns>
    private bool ShouldSessionFileCatchUp(SessionTrigger sTrigger, SessionTrigger edfTrigger) {
        //if one trigger is either trial ended or trial timeout and the other is Trial started
        bool premise3 = sTrigger >= SessionTrigger.TrialEndedTrigger && edfTrigger == SessionTrigger.TrialStartedTrigger;
        bool premise4 = edfTrigger >= SessionTrigger.TrialEndedTrigger && sTrigger == SessionTrigger.TrialStartedTrigger;

        if (premise3 || premise4) {
            return sTrigger > edfTrigger;
        }
        else {
            return sTrigger < edfTrigger;
        }
    }

    private IEnumerator ProcessSessionDataTask(string sessionPath, string edfPath, string toFolderPath) {
        //Setup
        fadeController.gameObject.SetActive(false);
        EyeDataReader eyeReader = null;

        if (IsEdfFile(edfPath)) {
            eyeReader = new EDFReader(edfPath, out int errVal);
            if (errVal != 0) { //check if file can be parsed by library
                String error = $"Unable to open .edf file";
                Debug.LogError(error);
                Console.WriteError(error);
                yield break;
            }
        }
        else {
            eyeReader = new EyeCsvReader(edfPath);
        }

        gazePointPool?.PreparePool();

        progressBar.value = 0;
        progressBar.gameObject.SetActive(true);

        string filename = $"{Path.GetFileNameWithoutExtension(sessionPath)}_{Path.GetFileNameWithoutExtension(edfPath)}.csv";
        RayCastRecorder recorder = new RayCastRecorder(toFolderPath, filename);

        //process data
        yield return ProcessSession(sessionPath, eyeReader, recorder);


        //cleanup
        recorder.Close();
        SceneManager.LoadScene("Start");
        fadeController.gameObject.SetActive(true);
        progressBar.gameObject.SetActive(false);
        expected = TestTrigger.TrialStartedTrigger;
        SessionStatusDisplay.ResetStatus();
    }

    private IEnumerator ProcessSession(string sessionPath, EyeDataReader eyeReader, RayCastRecorder recorder) {
        int frameCounter = 0;
        int trialCounter = 1;

        SessionReader sessionReader = new SessionReader(sessionPath);

        //Move to first Trial Trigger
        AllFloatData data = PrepareFiles(sessionReader, eyeReader, SessionTrigger.TrialStartedTrigger);

        Queue<SessionData> sessionFrames = new Queue<SessionData>();
        Queue<AllFloatData> fixations = new Queue<AllFloatData>();

        DataTypes latestType = DataTypes.NULL;

        //feed in current Data
        fixations.Enqueue(data);

        int debugMaxMissedOffset = 0;

        while (sessionReader.HasNext() && latestType != DataTypes.NO_PENDING_ITEMS) {
            //buffer since sessionData.timeDelta is the time difference from the previous frame.
            sessionFrames.Enqueue(sessionReader.currData);

            float sessionEventPeriod = LoadToNextTriggerSession(sessionReader, sessionFrames, out SessionTrigger sessionTrigger);
            uint edfEventPeriod = LoadToNextTriggerEdf(eyeReader, fixations, out SessionTrigger edfTrigger, out uint timestamp, out latestType);

            bool missingTriggerDetected = false;
            bool isMissingEdfTrigger = false;
            SessionTrigger missingTrigger;

            //check and account for missing trigger
            while (sessionTrigger != edfTrigger && latestType != DataTypes.NO_PENDING_ITEMS && sessionReader.HasNext()) {
                missingTriggerDetected = true;
                string errorMessage = $"Missing trigger found at approx {timestamp}. session: {sessionTrigger}| edf: {edfTrigger}";
                Console.WriteError(errorMessage);
                Debug.LogWarning(errorMessage);

                //Assuming that the previous iteration is synchronized
                if (ShouldSessionFileCatchUp(sessionTrigger, edfTrigger)) {
                    //session file should catch up
                    sessionEventPeriod += LoadToNextTriggerSession(sessionReader, sessionFrames, out sessionTrigger);
                    isMissingEdfTrigger = true;
                    missingTrigger = edfTrigger;
                }
                else {
                    //edfFile should catch up
                    edfEventPeriod += LoadToNextTriggerEdf(eyeReader, fixations, out edfTrigger, out timestamp, out latestType);
                    isMissingEdfTrigger = false;
                    missingTrigger = sessionTrigger;
                }
            }

            float excessTime = (sessionEventPeriod * 1000f - edfEventPeriod);
            float timepassed = fixations.Peek().time;

            // if missing trigger detected and the time difference between the 2 files is less than 20ms
            if (missingTriggerDetected && Math.Abs(excessTime) > 20) {
                IgnoreData(fixations, recorder);
            }

            float timeOffset = excessTime / (sessionFrames.Count - 1);

            print($"timeError: {excessTime}|{timeOffset} for {sessionFrames.Count} frames, ses: {sessionEventPeriod: 0.00}, edf: {edfEventPeriod}");

            uint gazeTime = 0;

            float debugtimeOffset = 0;

            while (sessionFrames.Count > 0 && fixations.Count > 0) {
                SessionData sessionData = sessionFrames.Dequeue();
                AllFloatData currData = fixations.Peek();

                float period;
                if (sessionFrames.Count > 0) {
                    //peek since next sessionData holds the time it takes from this data to the next
                    period = (sessionFrames.Peek().timeDeltaMs) - timeOffset;
                }
                else {
                    //use current data's timedelta to approximate
                    period = (sessionData.timeDeltaMs) - timeOffset;
                }

                // does not matter if trigger in session file is missing since "true" timing is based on edf file
                if (missingTriggerDetected && isMissingEdfTrigger) {
                    SessionTrigger approxTrigger = sessionData.trigger;
                    if (approxTrigger != SessionTrigger.NoTrigger) {
                        ProcessTrigger(sessionData.trigger);
                        recorder.WriteEvent(DataTypes.MESSAGEEVENT, currData.time, $"Approximated Trigger {sessionData.flag}");
                    }
                }

                debugtimeOffset += timeOffset;

                timepassed += period;

                MoveRobotTo(robot, sessionData);

                //due to the nature of floats, (1.0 == 10.0 / 10.0) might not return true every time
                //therefore use Mathf.Approximately()
                while ((gazeTime < timepassed || Mathf.Approximately(timepassed, gazeTime)) && fixations.Count > 0) {
                    currData = fixations.Dequeue();
                    gazeTime = currData.time;
                    if (ProcessData(currData, recorder) == SessionTrigger.TrialStartedTrigger) {
                        trialCounter++;
                        SessionStatusDisplay.DisplayTrialNumber(trialCounter);
                    }
                }

                frameCounter++;
                frameCounter %= framePerBatch;
                if (frameCounter == 0) {
                    progressBar.value += framePerBatch;
                    yield return null;
                }
                gazePointPool?.ClearScreen();
            }

            Debug.LogWarning($"ses: {sessionFrames.Count}| fix: {fixations.Count}, timestamp {gazeTime}, timepassed{timepassed: 0.00}");
            float finalExcess = gazeTime - timepassed;

            Debug.LogWarning($"final excess: {finalExcess} | {finalExcess + sessionReader.currData.timeDeltaMs} || {sessionReader.currData.timeDeltaMs}");
            Debug.LogWarning($"whats this?: {debugtimeOffset} | {timepassed} vs {sessionEventPeriod} vs {edfEventPeriod}");
            if (Math.Abs(finalExcess) > 3) {
                Debug.LogError("SEE ABOVE");
            }

            //clear queues to prepare for next trigger
            sessionFrames.Clear();
            while (fixations.Count > 0) {
                debugMaxMissedOffset = Math.Max(fixations.Count, debugMaxMissedOffset);

                if (ProcessData(fixations.Dequeue(), recorder) == SessionTrigger.TrialStartedTrigger) {
                    trialCounter++;
                    SessionStatusDisplay.DisplayTrialNumber(trialCounter);
                }
            }
        }

        Debug.LogError(debugMaxMissedOffset);
        sessionReader.Close();
    }

    private SessionTrigger ProcessData(AllFloatData data, RayCastRecorder recorder) {
        switch (data.dataType) {
            case DataTypes.SAMPLE_TYPE:
                Fsample fs = (Fsample)data;

                RaycastGazeData(fs, cueCaster, out string objName, out Vector2 relativePos, out Vector3 objHitPos, out Vector3 gazePoint);
                recorder.WriteSample(data.dataType, data.time, objName, relativePos, objHitPos, gazePoint, fs.rightGaze, robot.position, robot.rotation.eulerAngles.y);

                gazePointPool?.AddGazePoint(GazeCanvas, viewport, fs.rightGaze);

                return SessionTrigger.NoTrigger;
            case DataTypes.MESSAGEEVENT:
                FEvent fe = (FEvent)data;
                ProcessTrigger(fe.trigger);

                recorder.WriteEvent(fe.dataType, fe.time, fe.message);

                return fe.trigger;
            default:
                //ignore others for now
                //Debug.LogWarning($"Unsupported EDF DataType Found! ({type})");
                return SessionTrigger.NoTrigger;
        }
    }

    private void IgnoreData(Queue<AllFloatData> fixations, RayCastRecorder recorder) {
        while (fixations.Count > 0) {
            AllFloatData data = fixations.Dequeue();

            switch (data.dataType) {
                case DataTypes.SAMPLE_TYPE:
                    Fsample fs = (Fsample)data;
                    recorder.IgnoreEvent(fs.dataType, fs.time, fs.rightGaze);

                    break;
                case DataTypes.MESSAGEEVENT:
                    FEvent fe = (FEvent)data;
                    ProcessTrigger(fe.trigger);
                    recorder.WriteEvent(fe.dataType, fe.time, $"Data ignored {fe.message}");

                    break;
                default:
                    //ignore others for now
                    //Debug.LogWarning($"Unsupported EDF DataType Found! ({type})");
                    break;
            }
        }
    }

    [Flags]
    enum TestTrigger {
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
    private TestTrigger ConvertToTestTrigger(SessionTrigger trigger) {
        switch (trigger) {
            case SessionTrigger.CueShownTrigger:
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

    private void ProcessTrigger(AllFloatData data) {
        if (data.dataType == DataTypes.MESSAGEEVENT) {
            ProcessTrigger(((FEvent)data).trigger);
        }
    }

    /// <summary>
    /// Processes the Trigger by showing or hiding the cues.
    /// </summary>
    /// <param name="trigger">Current Trigger</param>
    private void ProcessTrigger(SessionTrigger trigger) {
        TestTrigger test = ConvertToTestTrigger(trigger);

        if ((test | expected) != expected) {
            Debug.LogError($"Expected:{expected}, Received: {trigger}");
        }

        switch (trigger) {
            case SessionTrigger.CueShownTrigger:
                cueController.HideCue();
                cueController.ShowHint();
                expected = TestTrigger.TimeoutTrigger | TestTrigger.TrialEndedTrigger;
                SessionStatusDisplay.DisplaySessionStatus("Trial Running");
                break;

            case SessionTrigger.TrialStartedTrigger:
                cueController.HideHint();
                cueController.ShowCue();
                SessionStatusDisplay.DisplaySessionStatus("Showing Cue");
                expected = TestTrigger.CueShownTrigger;
                break;

            case SessionTrigger.TimeoutTrigger:
            case SessionTrigger.TrialEndedTrigger:
                cueController.HideAll();
                if (trigger == SessionTrigger.TimeoutTrigger) {
                    SessionStatusDisplay.DisplaySessionStatus("Time out");
                }
                else {
                    SessionStatusDisplay.DisplaySessionStatus("Trial Ended");
                }

                expected = TestTrigger.TrialStartedTrigger;
                break;

            case SessionTrigger.NoTrigger:
                //do nothing
                break;

            case SessionTrigger.ExperimentVersionTrigger:
                SessionStatusDisplay.DisplaySessionStatus("Next Session");
                expected = TestTrigger.TrialStartedTrigger;
                break;
            default:
                Debug.LogError($"Unidentified Session Trigger: {trigger}");
                break;
        }
    }

    /// <summary>
    /// Fires a raycast into the UI or Scene based on the sample data to determine what object the sample data is fixating upon.
    /// </summary>
    /// <param name="sample">Data sameple from edf file</param>
    /// <param name="gRaycaster">GraphicRaycaster of UI seen by the subject</param>
    /// <param name="objName">Name of object the gazed object</param>
    /// <param name="relativePos">Local 2D offset of from the center of the object gazed</param>
    /// <param name="objHitPos">World position of the object in the scene</param>
    /// <param name="gazePoint">World position of the point where the gaze fixates the object</param>
    /// <returns>True if an object was in the path of the gaze</returns>
    private bool RaycastGazeData(Fsample sample,
                                 GraphicRaycaster gRaycaster,
                                 out string objName,
                                 out Vector2 relativePos,
                                 out Vector3 objHitPos,
                                 out Vector3 gazePoint) {
        //check for UI raycasts first
        if (RaycastToUI(sample, gRaycaster, out objName, out relativePos, out objHitPos, out gazePoint)) {
            return true;
        }
        else {
            //check for scene raycast
            return RaycastToScene(sample, out objName, out relativePos, out objHitPos, out gazePoint);
        }
    }

    /// <summary>
    /// Fires a raycast into the Scene based on the sample data to determine what object the sample data is fixating upon.
    /// </summary>
    /// <param name="sample">Data sameple from edf file</param>
    /// <param name="objName">Name of object the gazed object</param>
    /// <param name="relativePos">Local 2D offset of from the center of the object gazed</param>
    /// <param name="objHitPos">World position of the object in the scene</param>
    /// <param name="gazePoint">World position of the point where the gaze fixates the object</param>
    /// <returns>True if an object was in the path of the gaze</returns>
    private bool RaycastToScene(Fsample sample,
                                 out string objName,
                                 out Vector2 relativePos,
                                 out Vector3 objHitPos,
                                 out Vector3 gazePoint) {
        Ray r = viewport.ScreenPointToRay(sample.rightGaze);

        if (Physics.Raycast(r, out RaycastHit hit)) {
            lineRenderer?.SetPositions(new Vector3[] { viewport.transform.position, hit.point });
            Transform objhit = hit.transform;

            objName = hit.transform.name;
            relativePos = computeLocalPostion(objhit, hit); ;
            objHitPos = objhit.position;
            gazePoint = hit.point;

            return false;

        }
        else {
            objName = null;
            relativePos = Vector2.zero;
            gazePoint = Vector3.zero;
            objHitPos = Vector3.zero;

            //clear the line renderer
            lineRenderer?.SetPositions(new Vector3[0]);

            return false;
        }
    }

    /// <summary>
    /// Checks if there are any objects hit in th UI Canvas by the eye sample data
    /// </summary>
    /// <param name="sample">Fsample from eyelink</param>
    /// <param name="gRaycaster">GraphicRaycaster of UI seen by the subject</param>
    /// <param name="objName">Name of object the gazed object</param>
    /// <param name="relativePos">Local 2D offset of from the center of the object gazed</param>
    /// <param name="objHitPos">World position of the object in the scene</param>
    /// <param name="gazePoint">World position of the point where the gaze fixates the object</param>
    /// <returns>True if an object was in the path of the gaze</returns>
    private bool RaycastToUI(Fsample sample,
                             GraphicRaycaster gRaycaster,
                             out string objName,
                             out Vector2 relativePos,
                             out Vector3 objHitPos,
                             out Vector3 gazePoint) {
        List<RaycastResult> results = new List<RaycastResult>(0);

        PointerEventData data = new PointerEventData(EventSystem.current) {
            position = sample.rightGaze
        };
        gRaycaster?.Raycast(data, results);

        if (results.Count > 0) {
            if (results.Count > 1) {
                Debug.LogWarning($"There are overlapping graphics at time = {sample.time}");
            }

            Vector2 objPosition = RectTransformUtility.WorldToScreenPoint(viewport, results[0].gameObject.transform.position);

            // checking only the first element since the canvas is assumed to have no overlapping image objects.
            objName = results[0].gameObject.name;
            RectTransform t = results[0].gameObject.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToWorldPointInRectangle(t, sample.rightGaze, viewport, out gazePoint);
            Vector3 imgWorldPos = t.TransformPoint(t.rect.center);
            relativePos = gazePoint - imgWorldPos;
            objHitPos = results[0].gameObject.transform.position;

            return true;
        }
        else {
            objName = null;
            relativePos = Vector2.zero;
            gazePoint = Vector3.zero;
            objHitPos = Vector3.zero;

            //clear the line renderer
            lineRenderer?.SetPositions(new Vector3[0]);

            return false;
        }
    }

    /// <summary>
    /// Gets the local position with reference to the center of the object
    /// 
    /// Note: Naive implementation of computation becasue this code only considers the case that the
    /// normal is pointing in solely one of the x, y or z axes.
    /// </summary>
    /// <param name="objHit">Transsform of the object hit by the raycast</param>
    /// <param name="hit"></param>
    /// <returns>2D location of the point fixated by that gaze relative to the center of the image</returns>
    private Vector2 computeLocalPostion(Transform objHit, RaycastHit hit) {
        Vector3 normal = hit.normal;
        Vector3 dist = hit.point - objHit.position;

        Vector2 result = Vector2.zero;

        if (normal.x != 0) {
            result.y = dist.y;
            result.x = dist.z * normal.x;
        }
        else if (normal.y != 0) {
            result.y = dist.z;
            result.x = dist.x * normal.y;
        }
        else if (normal.z != 0) {
            result.y = dist.z;
            result.x = dist.x * normal.z;
        }

        return result;
    }

    private AllFloatData PrepareFiles(SessionReader sessionReader, EyeDataReader eyeReader, SessionTrigger firstOccurance) {
        FindNextSessionTrigger(sessionReader, firstOccurance);
        return FindNextEdfTrigger(eyeReader, firstOccurance);
    }

    /// <summary>
    /// Moves session Reader to point to the session reader.
    /// </summary>
    /// <param name="sessionReader">Session reader to move</param>
    /// <param name="trigger">SessionTrigger to move to</param>
    private void FindNextSessionTrigger(SessionReader sessionReader, SessionTrigger trigger) {
        //move sessionReader to point to first trial
        while (sessionReader.NextData()) {
            if (sessionReader.currData.trigger == trigger) {
                MoveRobotTo(robot, sessionReader.currData);
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
                FEvent ev = (FEvent)data;
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
    private float LoadToNextTriggerSession(SessionReader reader, Queue<SessionData> frames, out SessionTrigger trigger) {
        float totalTime = 0;

        trigger = SessionTrigger.NoTrigger;
        bool isNextEventFound = false;

        // Conditon evaluation is Left to Right and it short circuits.
        // Please do not change the order of this if conditon.
        while (!isNextEventFound && reader.NextData()) {
            SessionData data = reader.currData;
            frames.Enqueue(data);
            totalTime += data.timeDelta;
            trigger = data.trigger;

            isNextEventFound = trigger != SessionTrigger.NoTrigger;
        }

        return totalTime;
    }


    /// <summary>
    /// Loads data from the next data point to the next trigger (inclusive) and returns the total taken from the current 
    /// position top the next Trigger
    /// 
    /// </summary>
    /// <param name="reader">EyeDataReader to process</param>
    /// <param name="fixations">Queue to fill the data</param>
    /// <param name="trigger">The trigger where the loading stops at</param>
    /// <param name="timestamp">The timestamp of the trigger where the loading stops at</param>
    /// <param name="latestType">The Datatype of the last data point</param>
    /// <returns>Time taken from next data point to the last data point</returns>
    private uint LoadToNextTriggerEdf(EyeDataReader reader,
                                      Queue<AllFloatData> fixations,
                                      out SessionTrigger trigger,
                                      out uint timestamp,
                                      out DataTypes latestType) {

        bool isNextEventFound = false;

        while (!isNextEventFound) {
            AllFloatData data = reader.GetNextData();
            DataTypes type = data.dataType;

            fixations.Enqueue(data);

            if (type == DataTypes.MESSAGEEVENT) {
                isNextEventFound = true;
                FEvent ev = (FEvent)data;
                trigger = ev.trigger;
                timestamp = ev.time;
                isNextEventFound = trigger != SessionTrigger.NoTrigger;
                latestType = type;
                return ev.time - fixations.Peek().time;
            }
            else if (type == DataTypes.NO_PENDING_ITEMS) {
                break;
            }
        }

        trigger = SessionTrigger.NoTrigger;
        timestamp = 0;
        latestType = DataTypes.NO_PENDING_ITEMS;
        return 0;
    }

    /// <summary>
    /// Positions the robot as stated in the Session file.
    /// </summary>
    /// <param name="robot">Transfrom of the robot to move</param>
    /// <param name="reader">Session data of the Object</param>
    private void MoveRobotTo(Transform robot, SessionData reader) {
        Vector3 pos = robot.position;
        // Y is unchanged
        pos.x = reader.posX;
        pos.z = reader.posZ;

        // Rotate around Y axis
        Vector3 orientation = robot.rotation.eulerAngles;
        orientation.y = reader.rotY;

        //convert back to quaterion
        Quaternion newrot = Quaternion.Euler(orientation);

        robot.SetPositionAndRotation(pos, newrot);
    }

    private void SaveScreen(Camera cam, string filename) {
        Texture2D tex = new Texture2D(cam.pixelWidth, cam.pixelHeight, TextureFormat.RGB24, false);
        tex.ReadPixels(cam.pixelRect, 0, 0);
        tex.Apply();
        byte[] bytes = tex.EncodeToJPG();
        Destroy(tex);

        File.WriteAllBytes(filename, bytes);
    }
}
