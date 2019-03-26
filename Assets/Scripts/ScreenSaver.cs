using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using EDFUtil;
using EdfAccess;
using System;
using UnityEngine.SceneManagement;

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

    public Camera c;

    public Transform robot;

    public LineRenderer lineRenderer;
    public CueController cueController;
    public FadeCanvas fadeController;

    private void Awake() {
        eyeLinkFileInput.onEndEdit.AddListener(ChooseEyelinkFile);
        sessionInput.onEndEdit.AddListener(ChooseSession);
        folderInput.onEndEdit.AddListener(ChooseFolder);
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
        if (sessionInput.GetComponent<Image>().color == Color.green) {
            if (folderInput.GetComponent<Image>().color == Color.green) {
                //long st = long.Parse(from.text);
                //long end = long.Parse(to.text);
                //if((end >= st) && (end <= (allLines.Length-1))){
                //everything okay to render
                //StartCoroutine(ProcessSessionData(sessionInput.text, folderInput.text));
                //}
                SceneManager.sceneLoaded += OnsceneLoaded;
                SceneManager.LoadScene("Double Tee");
            }
        }
    }

    public void onBrowseSession() {
        fb.OnFileBrowserExit += ChooseSession;
        fb.Show(Application.dataPath);
    }

    public void onBrowseEyelink() {
        fb.OnFileBrowserExit += ChooseEyelinkFile;
        fb.Show(Application.dataPath);
    }

    public void onBrowseFolder() {
        fb.OnFileBrowserExit += ChooseFolder;
        fb.Show(Application.dataPath);
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
            EdfFilePointer filePointer = EdfAccessWrapper.EdfOpenFile(eyeLinkFileInput.text, 0, 1, 1, out int errVal);
            if (errVal == 0) {
                print($"success {errVal}");
                uint test = EdfAccessWrapper.EdfGetElementCount(filePointer);
                print($"element count:{test}");
                int dataType = EdfAccessWrapper.EdfGetNextData(filePointer);
                ALLF_DATA data = EdfAccessWrapper.EdfGetFloatData(filePointer);
                print($"success data :\n{data}");
                EdfAccessWrapper.EdfCloseFile(filePointer);
            }
            else {
                print("fail");
            }

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
        // check if file exists
        if (!File.Exists(sessionPath)) {
            Debug.LogError($"{sessionPath} does not exist");
            yield break;
        }

        if (!File.Exists(edfPath)) {
            Debug.LogError($"{edfPath} does not exist");
            yield break;
        }

        //check if directory exists
        if (!Directory.Exists(toFolderPath)) {
            Debug.LogError($"{toFolderPath} does not exist");
            yield break;
        }

        SessionReader sessionReader = new SessionReader(sessionPath);
        EDFReader edfReader = new EDFReader(edfPath);

        int counter = 0;

        Vector3 screenPos = Vector3.zero;
        EdfData data;
        do {
            Ray r;

            data = edfReader.getNextData();

            switch (data) {
                case EdfSampleData sample:

                    screenPos.Set(sample.gazeX, sample.gazeY, 0);

                    r = c.ScreenPointToRay(screenPos);
                    //r = new Ray(robot.position, robot.forward);
                    if (Physics.Raycast(r, out RaycastHit hit)) {
                        lineRenderer?.SetPositions(new Vector3[] { c.transform.position, hit.point });
                        yield return null;//for debug
                    }
                    else {
                        print("nothing");
                    }

                    break;
                case EdfMessage message:
                    break;
                default:
                    break;
            }

            data = edfReader.getNextData();

            MoveRobotTo(robot, sessionReader);

            //process the triggers
            switch (sessionReader.flag / 10) {
                case 0: //no trigger
                    break; //do nothing
                case 1: //cue shown
                    cueController.ShowCue();
                    break;
                case 2: //cue hidden, hint shown, trial start
                    cueController.HideCue();
                    cueController.ShowHint();
                    break;
                case 3: //Trial Ended (Trial Success)
                    cueController.HideAll();
                    break;
                case 4: //Timeout Trigger (TrialFailed)
                    cueController.HideAll();
                    break;
                case 8: //Experiment Version
                    fadeController.FadeIn();
                    break;
                default:
                    Console.WriteError($"Unkown Flag {sessionReader.flag}");
                    break;
            }



            //process more frames before returning control to main game logic.
            counter++;
            counter %= framePerBatch;
            if (counter == framePerBatch - 1) {
                yield return null;
            }
        } while (sessionReader.NextData());

        sessionReader.Close();
        SceneManager.UnloadSceneAsync("Double Tee");
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
