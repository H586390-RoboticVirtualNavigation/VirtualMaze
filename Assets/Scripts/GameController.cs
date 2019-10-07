using UnityEngine;

/// <summary>
/// MonoBehaviour that affects VirtualMaze globally.
/// </summary>
public class GameController : MonoBehaviour {
    //UPDATE THESE WITH EACH COMPILATION
    public static readonly int versionNum = 4;
    public static readonly string versionInfo = "20171221 Taxi Continuous v" + versionNum;
    public static readonly string pportInfo = "v" + versionNum;

    private static GameController _instance;
    public static GameController instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType(typeof(GameController)) as GameController;
                if (_instance == null) {
                    Debug.LogError("need at least one GameController");
                }
            }
            return _instance;
        }
    }

    private void Update() {
        ProcessKeyPress();
    }

    //framerate dependent
    private readonly int pressDelay = 10;
    private int counter = 10;

    private void ProcessKeyPress() {
        if (!Application.isBatchMode && Input.GetKey(KeyCode.Escape)) {
            if (counter > 0) {
                counter--;
            }
            else {
                Application.Quit();
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape)) {
            counter = pressDelay;
        }
    }

    private void Start() {
        //Online sources says that if vSyncCount != 0, targetFrameRate will be ignored.
        Application.targetFrameRate = 30;

        //if display is 60hz, Unity will run at 30hz
        QualitySettings.vSyncCount = 2;

        if (Application.isBatchMode) {
            Debug.Log("Batchmode ready");
            int counter = 0;
            while (counter < 100) {
                counter++;
                print(counter);
            }

            Application.Quit();
        }
    }
}
