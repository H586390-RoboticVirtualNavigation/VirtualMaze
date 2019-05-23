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
    private int framePerBatch = 5;

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
            ChooseEyelinkFile("D:\\Program Files (D)\\SR Research\\EyeLink\\EDF_Access_API\\Example\\181026.edf");
            ChooseSession("D:\\Desktop\\FYP Init\\session01\\RawData_T1-400\\ShortVer.txt");
            ChooseFolder(@"D:\Documents\GitHub\VirtualMaze\out");
        }
    }

    private void OnsceneLoaded(Scene s, LoadSceneMode mode) {
        BasicLevelController levelcontroller = FindObjectOfType<BasicLevelController>();
        levelcontroller.gameObject.SetActive(false);


        print(levelcontroller.name);
        SceneManager.sceneLoaded -= OnsceneLoaded;

        StartCoroutine(ProcessSessionData(sessionInput.text, eyeLinkFileInput.text, folderInput.text));
        print("Started");
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

    private IEnumerator ProcessSessionData(string sessionPath, string edfPath, string toFolderPath) {
        fadeController.gameObject.SetActive(false);
        int frameCounter = 0;
        EdfFilePointer pointer = EdfAccessWrapper.EdfOpenFile(edfPath, 0, 1, 1, out int errVal);

        cueController.ShowCue();

        if (errVal != 0) { //check if file can be parsed by library
            Debug.LogError($"Unable to open .edf file");
            yield break;
        }

        SessionReader sessionReader = new SessionReader(sessionPath);

        //assume all files the first msg is 84
        uint timeToNextFrame;
        uint time = 0;

        //move sessionReader to point to first trial
        while (sessionReader.NextData()) {
            if (sessionReader.trigger == SessionTrigger.TrialStartedTrigger) {
                MoveRobotTo(robot, sessionReader);
                break;
            }
        }

        //move edfFile to point to first trial
        bool foundFirstTrial = false;
        while (!foundFirstTrial) {
            if (EdfAccessWrapper.EdfGetNextData(pointer) == DataTypes.MESSAGEEVENT) {
                ALLF_DATA data = EdfAccessWrapper.EdfGetFloatData(pointer);
                FEVENT ev = data.fe;
                if (ev.GetSessionTrigger() == SessionTrigger.TrialStartedTrigger) {
                    time = ev.sttime;
                    Debug.LogError($"LOOK AT ME TOO {ev.sttime}");
                    foundFirstTrial = true;
                    cueController.ShowCue();
                    break;
                }
            }
        }

        print("ended");
        bool synced = true;
        uint desyncTime = 0;

        string filename = $"{Path.GetFileNameWithoutExtension(sessionPath)}{Path.GetFileNameWithoutExtension(edfPath)}.csv";

        RayCastRecorder recorder = new RayCastRecorder(toFolderPath, filename);

        while (sessionReader.NextData()) {
            timeToNextFrame = time + (uint)(sessionReader.timeDelta * 1000);
            print($"timetoNext:{timeToNextFrame}");
            MoveRobotTo(robot, sessionReader);

            if (sessionReader.trigger != SessionTrigger.NoTrigger) {
                synced = false;
            }

            while (time < timeToNextFrame || !synced) {
                print($"{time} | {timeToNextFrame}");

                DataTypes type = EdfAccessWrapper.EdfGetNextData(pointer);
                ALLF_DATA data = EdfAccessWrapper.EdfGetFloatData(pointer);

                switch (type) {
                    case DataTypes.SAMPLE_TYPE:
                        print($"type:{type}, flags:{Convert.ToString(data.fs.flags, 16)}, time:{data.fs.time}, {synced}");
                        RaycastGazeData(data.fs, out string objName, out Vector2 relativePos, out Vector3 objHitPos, out Vector3 gazePoint);

                        recorder.WriteSample(type, data.fs.time, objName, relativePos, objHitPos, gazePoint, data.fs.RightGaze);

                        time = data.fs.time;
                        break;
                    case DataTypes.MESSAGEEVENT:
                        SessionTrigger trigger = ProcessMessageEvent(data.fe);
                        time = data.fe.sttime;

                        print($"type:{type}, flags:{Convert.ToString(data.fs.flags, 16)}, time:{time}, {synced}");

                        recorder.WriteEvent(type, data.fe.sttime, data.fe.GetMessage());

                        if (!synced) {
                            synced = sessionReader.trigger == trigger;
                            Debug.LogError($"error of {time - desyncTime}ms, {synced}, sessionT: {data.fe.GetMessage()}|{sessionReader.flag}, @desynced at{desyncTime}");
                            desyncTime = 0;
                        }
                        else {
                            synced = false;
                            float frameError = sessionReader.timeDelta;//1 for this current frame
                            uint timestart = data.fe.sttime;
                            while (sessionReader.NextData() && !synced) {
                                if (sessionReader.trigger == trigger) {
                                    synced = true;
                                    Debug.LogError($"{frameError}|{sessionReader.flag}|at:{timestart}|{data.fe.GetMessage()},{sessionReader.flag}");
                                }
                                else {
                                    frameError += sessionReader.timeDelta;
                                    //Debug.Break();
                                }
                            }
                        }
                        break;
                    default:
                        //ignore others for now
                        Debug.LogWarning($"Unsupported EDF DataType Found! ({type})");
                        break;
                }

                if (time > timeToNextFrame && !synced && desyncTime == 0) {
                    desyncTime = time;
                }
            }
            frameCounter++;
            frameCounter %= framePerBatch;
            if (frameCounter == 0) {
                yield return null;
            }
        }

        recorder.Close();
        sessionReader.Close();
        SceneManager.UnloadSceneAsync("Double Tee");
    }

    private SessionTrigger ProcessMessageEvent(FEVENT ev) {
        //print($", @time:{ev.sttime}");

        SessionTrigger trigger = ev.GetSessionTrigger();

        switch (trigger) {
            case SessionTrigger.CueShownTrigger:
                cueController.HideCue();
                cueController.ShowHint();
                break;

            case SessionTrigger.TrialStartedTrigger:
                cueController.ShowCue();
                break;

            case SessionTrigger.TimeoutTrigger:
            case SessionTrigger.TrialEndedTrigger:
                cueController.HideAll();
                break;

            case SessionTrigger.NoTrigger:
            default:
                Debug.LogError($"Unidentified Session Trigger: {trigger}");
                break;
        }
        return trigger;
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
    private bool RaycastGazeData(FSAMPLE sample, out string objName, out Vector2 relativePos, out Vector3 objHitPos, out Vector3 gazePoint) {
        //check for UI raycasts first
        List<RaycastResult> results = new List<RaycastResult>();

        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = sample.RightGaze;
        cueCaster.Raycast(data, results);

        if (results.Count > 0) {
            print($"results count: {results.Count}, gaze:{data.position}");

            if (results.Count > 1) {
                Debug.LogWarning($"There are overlapping graphics when graphics at time = {sample.time}");
            }

            Vector2 objPosition = RectTransformUtility.WorldToScreenPoint(viewport, results[0].gameObject.transform.position);


            objName = results[0].gameObject.name;

            RectTransform t = results[0].gameObject.GetComponent<RectTransform>();


            //check this
            RectTransformUtility.ScreenPointToWorldPointInRectangle(t, sample.RightGaze, viewport, out gazePoint);

            Vector3 imgWorldPos = t.TransformPoint(t.rect.center);

            relativePos = gazePoint - imgWorldPos;

            objHitPos = results[0].gameObject.transform.position;

            return true;
        }

        //check for scene raycast
        Ray r = viewport.ScreenPointToRay(sample.RightGaze);

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


    private Vector2 computeLocalPostion(Transform objHit, RaycastHit hit) {
        Debug.LogWarning($"{hit.normal}, {objHit.name}");

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

    void MoveRobotTo(Transform robot, SessionReader reader) {
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
