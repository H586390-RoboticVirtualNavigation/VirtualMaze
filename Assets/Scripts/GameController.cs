using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// MonoBehaviour that affects VirtualMaze globally.
/// </summary>
public class GameController : MonoBehaviour {
    //UPDATE THESE WITH EACH COMPILATION
    public static readonly int versionNum = 4;
    public static readonly string versionInfo = "20171221 Taxi Continuous v" + versionNum;
    public static readonly string pportInfo = "v" + versionNum;

    [SerializeField]
    private ScreenSaver saver = null;

    private bool generationComplete = false;

    private string SessionPattern = "session[0-9]{2}";
    private string DayPattern = "[0-9]{8}";

    private string eyelinkMatFile = $"{Path.DirectorySeparatorChar}eyelink.mat";
    private string unityfileMatFile = $"{Path.DirectorySeparatorChar}unityfile.mat";
    private string resultFile = $"{Path.DirectorySeparatorChar}unityfile_eyelink.csv";

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
        //QualitySettings.vSyncCount = 2;
        SetPaths();

        if (Application.isBatchMode) {
            BatchModeLogger logger = new BatchModeLogger(PresentWorkingDirectory);
            Queue<string> sessionQ = new Queue<string>();

            DirectoryInfo pwd = new DirectoryInfo(PresentWorkingDirectory);

            if (IsDayDir(pwd)) {
                IEnumerable<string> subDirs = Directory.EnumerateDirectories(pwd.FullName, "*", SearchOption.TopDirectoryOnly);
                foreach (string subDir in subDirs) {
                    if (IsSessionDir(new DirectoryInfo(subDir))) {
                        logger.Print($"Queuing {subDir}");
                        sessionQ.Enqueue(subDir);
                    }
                }
            }
            else if (IsSessionDir(pwd)) {
                logger.Print($"Queuing {pwd}");
                sessionQ.Enqueue(pwd.FullName);
            }

            if (sessionQ.Count > 0) {
                logger.Print($"{sessionQ.Count} sessions to be processed");
                ProcessSession(sessionQ, logger);
            }
            else {
                logger.Print("No Session directories found! Exiting");
                logger.Dispose();
                Application.Quit();
            }
        }

    }

    private async void ProcessSession(Queue<string> sessions, BatchModeLogger logger) {
        string path;
        int total = sessions.Count;
        int count = 1, notifyAliveCount = 0;

        while (sessions.Count > 0) {
            path = sessions.Dequeue();
            logger.Print($"Starting({count}/{total}): {path}");

            StartCoroutine(ProcessWrapper(path + unityfileMatFile, path + eyelinkMatFile, path));
            while (!generationComplete) {
                await Task.Delay(10000); //10 second notify-alive message

                notifyAliveCount++;
                notifyAliveCount %= 6; //only print message every 60 seconds
                if (notifyAliveCount == 0) {
                    logger.Print($"{saver.progressBar.value * 100}%: Data Generation is still running. {DateTime.Now.ToString()}");
                }
            }
            if (File.Exists(path + resultFile)) {
                logger.Print($"Success: {path + resultFile}");
            }
            else {
                logger.Print($"Failed: {path + resultFile}. Add to the command \"-logfile <log file location>.txt\" to debug");
            }
            count++;
        }
        logger.Print("BatchMode Complete! Exiting VirtualMaze.");
        logger.Dispose();
        Application.Quit();
    }

    private string PresentWorkingDirectory { get => Path.GetFullPath("."); }

    private bool IsDayDir(DirectoryInfo dirInfo) {
        return Regex.IsMatch(dirInfo.Name, DayPattern);
    }

    private bool IsSessionDir(DirectoryInfo dirInfo) {
        return Regex.IsMatch(dirInfo.Name, SessionPattern);
    }

    private IEnumerator ProcessWrapper(string sessionPath, string edfPath, string toFolderPath) {
        print($"session: {sessionPath}");
        print($"edf: {edfPath}");
        print($"toFolder: {toFolderPath}");

        generationComplete = false;
        try {
            yield return saver.ProcessSessionDataTask(sessionPath, edfPath, toFolderPath);
        }
        finally { //so that the batchmode app will quit or move on the the next session
            generationComplete = true;
        }
    }



    private void SetPaths() {
        //Debug.LogError("dll ready?");
        //string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
        //Debug.LogError(currentPath);
        //StringBuilder dllPathB = new StringBuilder(Environment.CurrentDirectory);
        //dllPathB.Append(Path.DirectorySeparatorChar);
        //dllPathB.Append("Assets");
        //dllPathB.Append(Path.DirectorySeparatorChar);
        //dllPathB.Append("Plugins");
        //dllPathB.Append(Path.DirectorySeparatorChar);
        //dllPathB.Append("SharpHDF");
        //dllPathB.Append(Path.DirectorySeparatorChar);
        //dllPathB.Append("bin64");
        //dllPathB.Append(Path.DirectorySeparatorChar);


        //String dllPath = dllPathB.ToString();
        //Debug.LogError(dllPath);
        //if (currentPath.Contains(dllPath) == false) {
        //    Environment.SetEnvironmentVariable("PATH", currentPath + Path.PathSeparator + dllPath, EnvironmentVariableTarget.Process);
        //}
    }
}
