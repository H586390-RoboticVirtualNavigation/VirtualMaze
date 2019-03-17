using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScreenSaver : BasicGUIController {
    private int framePerBatch = 5;
    private bool levelLoaded;

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
    public Fading fadeController;

    private void OnsceneLoaded(Scene s, LoadSceneMode mode) {
        BasicLevelController levelcontroller = FindObjectOfType<BasicLevelController>();
        levelcontroller.gameObject.SetActive(false);


        print(levelcontroller.name);
        SceneManager.sceneLoaded -= OnsceneLoaded;

        StartCoroutine(ProcessSessionData(sessionInput.text, folderInput.text));
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
            SetInputFieldValid(eyeLinkFileInput);
        }
        else {
            SetInputFieldInvalid(sessionInput);
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

        SceneManager.sceneLoaded += OnSceneLoadCallback;
    }

    private void OnApplicationQuit() {
        levelLoaded = false;
        SceneManager.sceneLoaded -= this.OnSceneLoadCallback;
    }

    void OnSceneLoadCallback(Scene scene, LoadSceneMode sceneMode) {
        levelLoaded = true;
    }

    private IEnumerator ProcessSessionData(string sessionPath, string toFolderPath) {
        // check if file exists
        if (!File.Exists(sessionPath)) {
            Debug.LogError(sessionPath + " does not exist");
            yield break;
        }
        //check if directory exists
        if (!Directory.Exists(toFolderPath)) {
            Debug.LogError(toFolderPath + " does not exist");
            yield break;
        }

        SessionReader sessionReader = new SessionReader(sessionPath);
        int counter = 0;

        do {
            Ray r;

            MoveRobotTo(robot, sessionReader);

            for (int i = 0; i < 1; i++) {
                r = c.ScreenPointToRay(new Vector3(540, 960)); //960
                //r = new Ray(robot.position, robot.forward);
                if (Physics.Raycast(r, out RaycastHit hit)) {
                    lineRenderer?.SetPositions(new Vector3[] { c.transform.position, hit.point });
                }
                else {
                    print("nothing");
                }
            }

            //process more frames before returning control to main game logic.
            counter++;
            counter %= framePerBatch;
            if (counter == framePerBatch - 1) {
                yield return null;
            }

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
