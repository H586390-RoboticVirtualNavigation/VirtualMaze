using Eyelink.EdfAccess;
using Eyelink.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenSaver : BasicGUIController {
    private int framePerBatch = 100;

    public InputField eyeLinkFileInput;
    public InputField sessionInput;
    public InputField folderInput;
    public FileBrowser fb;
    public InputField from;
    public InputField to;
    public Text sessionInfo;

    private GameObject gazeIndicator;

    public Camera viewport;

    public Transform robot;

    public LineRenderer lineRenderer;
    public CueController cueController;
    public FadeCanvas fadeController;

    public GraphicRaycaster cueCaster;

    private void Awake() {
        eyeLinkFileInput.onEndEdit.AddListener(ChooseEyelinkFile);
        sessionInput.onEndEdit.AddListener(ChooseSession);
        folderInput.onEndEdit.AddListener(ChooseFolder);

        if (true) { //for testing purposes.
            ChooseEyelinkFile(@"D:\Desktop\NUS\FYP\181105.edf");
            ChooseSession(@"D:\Desktop\NUS\FYP\session_1_5112018105323.txt");
            ChooseFolder(@"D:\Documents\GitHub\VirtualMaze\out");
        }
    }

    private void OnsceneLoaded(Scene s, LoadSceneMode mode) {
        BasicLevelController levelcontroller = FindObjectOfType<BasicLevelController>();
        levelcontroller.gameObject.SetActive(false);

        SceneManager.sceneLoaded -= OnsceneLoaded;

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


        //long st = long.Parse(from.text);
        //long end = long.Parse(to.text);
        //if((end >= st) && (end <= (allLines.Length-1))){
        //everything okay to render
        //StartCoroutine(ProcessSessionData(sessionInput.text, folderInput.text));
        //}
        SessionReader.ExtractInfo(sessionPath, out SessionContext context, out int numframes);

        SceneManager.sceneLoaded += OnsceneLoaded;
        SceneManager.LoadScene(context.trialName);
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

    void ChooseSession(string file) {
        fb.OnFileBrowserExit -= ChooseSession;
        if (string.IsNullOrEmpty(file)) return;

        sessionInput.text = file;
        if (File.Exists(file)) {
            SetInputFieldValid(sessionInput);

            SessionReader.ExtractInfo(file, out SessionContext context, out int numFrames);
            //from.text = "1";
            //to.text = lineCount.ToString();
            sessionInfo.text = $"{numFrames} frames";
            Console.Write(context.ToJsonString(true));
        }
        else {
            SetInputFieldInvalid(sessionInput);
        }
    }

    void ChooseEyelinkFile(string file) {
        fb.OnFileBrowserExit -= ChooseEyelinkFile;
        if (string.IsNullOrEmpty(file)) return;

        eyeLinkFileInput.text = file;

        if (File.Exists(file)) {
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

        gazeIndicator = GameObject.Find("gazeIndicator");

    }

    /// <summary>
    /// 
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

    private float GetWeightedOffset(float timeToNextTrigger, float currentPeriod, float excessTime) {
        return (currentPeriod / (timeToNextTrigger * 1000)) * excessTime;
    }


    private uint Round(float value) {
        //default Math.Round uses ToEven
        return (uint)Math.Round(value, MidpointRounding.AwayFromZero);
    }

    private IEnumerator ProcessSessionDataTask(string sessionPath, string edfPath, string toFolderPath) {
        //Setup
        fadeController.gameObject.SetActive(false);
        int frameCounter = 0;
        EdfFilePointer pointer = EdfAccessWrapper.EdfOpenFile(edfPath, 0, 1, 1, out int errVal);

        if (errVal != 0) { //check if file can be parsed by library
            String error = $"Unable to open .edf file";
            Debug.LogError(error);
            Console.WriteError(error);
            yield break;
        }
        string filename = $"{Path.GetFileNameWithoutExtension(sessionPath)}_{Path.GetFileNameWithoutExtension(edfPath)}.csv";
        RayCastRecorder recorder = new RayCastRecorder(toFolderPath, filename);

        SessionReader sessionReader = new SessionReader(sessionPath);

        //Move to first Trial Trigger
        DataTypes currType = PrepareFiles(sessionReader, pointer, SessionTrigger.TrialStartedTrigger);

        print($"prepared {EdfAccessWrapper.EdfGetFloatData(pointer).fe.GetSessionTrigger()}");

        Queue<SessionData> sessionFrames = new Queue<SessionData>();
        Queue<AllFloatData> fixations = new Queue<AllFloatData>();

        DataTypes latestType = DataTypes.NULL;
        //int useSessionEvents = 0;

        //feed in current Data
        fixations.Enqueue(EdfAccessWrapper.EdfGetFloatData(pointer).ConvertToAllFloatData(currType));

        int debugMaxMissedOffset = 0;

        while (sessionReader.HasNext() && latestType != DataTypes.NO_PENDING_ITEMS) {
            //buffer since sessionData.timeDelta is the time difference from the previous frame.
            sessionFrames.Enqueue(sessionReader.currData);

            float sessionEventPeriod = LoadToNextTriggerSession(sessionReader, sessionFrames, out SessionTrigger sessionTrigger);
            uint edfEventPeriod = LoadToNextTriggerEdf(pointer, fixations, out SessionTrigger edfTrigger, out uint timestamp, out latestType);

            bool missingTriggerDetected = false;
            bool isMissingEdfTrigger = false;
            SessionTrigger missingTrigger;

            while (sessionTrigger != edfTrigger && sessionReader.HasNext() && latestType != DataTypes.NO_PENDING_ITEMS) {
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
                    edfEventPeriod += LoadToNextTriggerEdf(pointer, fixations, out edfTrigger, out timestamp, out latestType);
                    isMissingEdfTrigger = false;
                    missingTrigger = sessionTrigger;
                }
            }

            float excessTime = (sessionEventPeriod * 1000 - edfEventPeriod);
            float timepassed = fixations.Peek().Time;
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
                        recorder.WriteEvent(DataTypes.MESSAGEEVENT, currData.Time, $"Approximated Trigger {sessionData.flag}");
                    }
                }

                debugtimeOffset += timeOffset;

                timepassed += period;

                MoveRobotTo(robot, sessionData);

                while (gazeTime <= Round(timepassed) && fixations.Count > 0) {
                    currData = fixations.Dequeue();
                    gazeTime = currData.Time;
                    ProcessData(currData, recorder);
                }

                frameCounter++;
                frameCounter %= framePerBatch;
                if (frameCounter == 0) {
                    yield return null;
                }
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

                ProcessData(fixations.Dequeue(), recorder);
            }
        }

        Debug.LogError(debugMaxMissedOffset);

        recorder.Close();
        sessionReader.Close();
        SceneManager.LoadScene("Start");
        fadeController.gameObject.SetActive(true);
    }

    private void ProcessData(AllFloatData data, RayCastRecorder recorder) {
        switch (data.dataType) {
            case DataTypes.SAMPLE_TYPE:
                //print($"type:{type}, flags:{Convert.ToString(data.fs.flags, 16)}, time:{data.fs.time}");
                Fsample fs = (Fsample)data;

                RaycastGazeData(fs, out string objName, out Vector2 relativePos, out Vector3 objHitPos, out Vector3 gazePoint);
                recorder.WriteSample(data.dataType, data.Time, objName, relativePos, objHitPos, gazePoint, fs.rightGaze, robot.position, robot.rotation.eulerAngles.y);

                break;
            case DataTypes.MESSAGEEVENT:
                FEvent fe = (FEvent)data;
                ProcessTrigger(fe.trigger);

                //print($"type:{type}, flags:{Convert.ToString(data.fs.flags, 16)}, time:{gazeTime}");
                recorder.WriteEvent(fe.dataType, fe.Time, fe.message);

                break;
            default:
                //ignore others for now
                //Debug.LogWarning($"Unsupported EDF DataType Found! ({type})");
                break;
        }
    }

    private void IgnoreData(Queue<AllFloatData> fixations, RayCastRecorder recorder) {
        while (fixations.Count > 0) {
            AllFloatData data = fixations.Dequeue();

            switch (data.dataType) {
                case DataTypes.SAMPLE_TYPE:
                    Fsample fs = (Fsample)data;
                    recorder.IgnoreEvent(fs.dataType, fs.Time, fs.rightGaze);

                    break;
                case DataTypes.MESSAGEEVENT:
                    FEvent fe = (FEvent)data;
                    ProcessTrigger(fe.trigger);
                    recorder.WriteEvent(fe.dataType, fe.Time, $"Data ignored {fe.message}");

                    break;
                default:
                    //ignore others for now
                    //Debug.LogWarning($"Unsupported EDF DataType Found! ({type})");
                    break;
            }
        }
    }

    private void ProcessTrigger(AllFloatData data) {
        if (data.dataType == DataTypes.MESSAGEEVENT) {
            ProcessTrigger(((FEvent)data).trigger);
        }
    }

    TestTrigger expected = TestTrigger.TrialStartedTrigger;

    [Flags]
    enum TestTrigger {
        NoTrigger = 0x0,
        TrialStartedTrigger = 0x1,
        CueShownTrigger = 0x2,
        TrialEndedTrigger = 0x4,
        TimeoutTrigger = 0x8,
        ExperimentVersionTrigger = 0xF
    }

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
                break;

            case SessionTrigger.TrialStartedTrigger:
                cueController.HideHint();
                cueController.ShowCue();

                expected = TestTrigger.CueShownTrigger;
                break;

            case SessionTrigger.TimeoutTrigger:
            case SessionTrigger.TrialEndedTrigger:
                cueController.HideAll();
                expected = TestTrigger.TrialStartedTrigger;
                break;

            case SessionTrigger.NoTrigger:
                //do nothing
                break;

            case SessionTrigger.ExperimentVersionTrigger:
                expected = TestTrigger.TrialStartedTrigger;
                break;
            default:
                Debug.LogError($"Unidentified Session Trigger: {trigger}");
                break;
        }
    }

    /// <summary>
    /// Fires a raycast into the scene bases on the sample data.
    /// </summary>
    /// <param name="sample">Data sameple from edf file</param>
    /// <param name="objName">Name of object the gazed object</param>
    /// <param name="relativePos">Local 2D offset of from the center of the object gazed</param>
    /// <param name="objHitPos">World position of the object in the scene</param>
    /// <param name="gazePoint">World position of the point where the gaze fixates the object</param>
    /// <returns>True if an object was in the path of the gaze</returns>
    private bool RaycastGazeData(Fsample sample, out string objName, out Vector2 relativePos, out Vector3 objHitPos, out Vector3 gazePoint) {
        //check for UI raycasts first
        List<RaycastResult> results = new List<RaycastResult>();

        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = sample.rightGaze;
        cueCaster.Raycast(data, results);

        if (results.Count > 0) {
            if (results.Count > 1) {
                Debug.LogWarning($"There are overlapping graphics at time = {sample.Time}");
            }

            Vector2 objPosition = RectTransformUtility.WorldToScreenPoint(viewport, results[0].gameObject.transform.position);


            objName = results[0].gameObject.name;

            RectTransform t = results[0].gameObject.GetComponent<RectTransform>();

            RectTransformUtility.ScreenPointToWorldPointInRectangle(t, sample.rightGaze, viewport, out gazePoint);

            Vector3 imgWorldPos = t.TransformPoint(t.rect.center);

            relativePos = gazePoint - imgWorldPos;

            objHitPos = results[0].gameObject.transform.position;

            return true;
        }

        //check for scene raycast
        Ray r = viewport.ScreenPointToRay(sample.rightGaze);

        if (Physics.Raycast(r, out RaycastHit hit)) {
            lineRenderer?.SetPositions(new Vector3[] { viewport.transform.position, hit.point });
            Transform objhit = hit.transform;

            objName = hit.transform.name;
            //not accurate did not consider the direction the object is facing
            relativePos = computeLocalPostion(objhit, hit); ;
            objHitPos = objhit.position;
            //check this
            gazePoint = hit.point;

            return false;

        }
        else {
            objName = null;
            //not accurate did not consider the direction the object is facing
            relativePos = Vector2.zero;
            //check this
            gazePoint = Vector3.zero;
            objHitPos = Vector3.zero;

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

    private DataTypes PrepareFiles(SessionReader sessionReader, EdfFilePointer pointer, SessionTrigger firstOccurance) {
        //move sessionReader to point to first trial
        while (sessionReader.NextData()) {
            if (sessionReader.currData.trigger == firstOccurance) {
                Debug.Log($"LOOK AT ME First {sessionReader.currData.rawData}");
                MoveRobotTo(robot, sessionReader.currData);
                break;
            }
        }

        DataTypes currType = DataTypes.NULL;

        //move edfFile to point to first trial
        bool foundFirstTrial = false;
        while (!foundFirstTrial) {
            currType = EdfAccessWrapper.EdfGetNextData(pointer);
            if (currType == DataTypes.MESSAGEEVENT) {
                ALLF_DATA data = EdfAccessWrapper.EdfGetFloatData(pointer);
                FEVENT ev = data.fe;
                if (ev.GetSessionTrigger() == firstOccurance) {
                    Debug.Log($"LOOK AT ME TOO {ev.sttime} {currType}");
                    foundFirstTrial = true;
                }
            }
        }

        return currType;
    }

    //returns total time to 
    private float LoadToNextTriggerSession(SessionReader reader, Queue<SessionData> frames, out SessionTrigger trigger) {
        float totalTime = 0;// reader.currData.timeDelta;

        trigger = SessionTrigger.NoTrigger;
        bool isNextEventFound = false;

        //bool checks is left to right do not rearrange this while condition
        while (!isNextEventFound && reader.NextData()) {
            SessionData data = reader.currData;
            frames.Enqueue(data);
            totalTime += data.timeDelta;
            trigger = data.trigger;

            isNextEventFound = trigger != SessionTrigger.NoTrigger;
        }

        return totalTime;
    }

    private uint LoadToNextTriggerEdf(EdfFilePointer file,
                                      Queue<AllFloatData> fixations,
                                      out SessionTrigger trigger,
                                      out uint timestamp,
                                      out DataTypes latestType) {

        bool isNextEventFound = false;

        while (!isNextEventFound) {
            DataTypes type = EdfAccessWrapper.EdfGetNextData(file);
            AllFloatData data = EdfAccessWrapper.EdfGetFloatData(file).ConvertToAllFloatData(type);

            fixations.Enqueue(data);

            if (type == DataTypes.MESSAGEEVENT) {
                isNextEventFound = true;
                FEvent ev = (FEvent)data;
                trigger = ev.trigger;
                timestamp = ev.Time;
                isNextEventFound = trigger != SessionTrigger.NoTrigger;
                latestType = type;
                return ev.Time - fixations.Peek().Time;
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

    void MoveRobotTo(Transform robot, SessionData reader) {
        Vector3 pos = robot.position;
        pos.x = reader.posX;
        pos.z = reader.posZ;

        Vector3 test = robot.rotation.eulerAngles;
        test.y = reader.rotY;

        Quaternion newrot = Quaternion.Euler(test);

        robot.SetPositionAndRotation(pos, newrot);
    }

    void SaveScreen(Camera cam, string filename) {
        Texture2D tex = new Texture2D(cam.pixelWidth, cam.pixelHeight, TextureFormat.RGB24, false);
        tex.ReadPixels(cam.pixelRect, 0, 0);
        tex.Apply();
        byte[] bytes = tex.EncodeToJPG();
        Destroy(tex);

        File.WriteAllBytes(filename, bytes);
    }
}
