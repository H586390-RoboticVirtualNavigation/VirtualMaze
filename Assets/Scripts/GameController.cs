using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        if (!Application.isEditor) {
            Application.targetFrameRate = 30;
            /* if display is 60hz, Unity will run at 30hz */
            QualitySettings.vSyncCount = 1;
        }

        if (Application.isBatchMode) {
            BatchModeLogger logger = new BatchModeLogger(PresentWorkingDirectory);

            string[] args = Environment.GetCommandLineArgs();
            bool isSessionList = false;

            int numofLengthBins = BinMapper.DEFAULT_NUM_BIN_LENGTH;
            int radius = BinWallManager.Default_Radius;
            int density = BinWallManager.Default_Density;
            string sessionListPath = null;

            for (int i = 0; i < args.Length; i++) {
                Debug.LogError($"ARG {i}: {args[i]}");
                switch (args[i].ToLower()) {
                    case "-sessionlist":
                        isSessionList = true;
                        logger.Print($"Session List detected!");
                        Debug.LogError($"{args[i + 1]}");
                        sessionListPath = args[i + 1];
                        break;

                    case "-numOfLengthBins":
                        if (int.TryParse(args[i + 1], out numofLengthBins)) {
                            logger.Print($"Setting number of length bins to : {numofLengthBins}");
                        }
                        else {
                            logger.Print($"Unable to parse {args[i + 1]} to integer, using  {numofLengthBins} as default");
                        }
                        break;
                    case "-density":
                        if (int.TryParse(args[i + 1], out density)) {
                            logger.Print($"Setting density to : {density}");
                        }
                        else {
                            logger.Print($"Unable to parse {args[i + 1]} to integer, using  {density} as default");
                        }
                        break;
                    case "-radius":
                        if (int.TryParse(args[i + 1], out radius)) {
                            logger.Print($"Setting radius to : {radius}");
                        }
                        else {
                            logger.Print($"Unable to parse {args[i + 1]} to integer, using  {radius} as default");
                        }
                        break;
                }
            }

            Queue<DirectoryInfo> dirQ = new Queue<DirectoryInfo>();

            if (!isSessionList) {
                PwdMode(logger, dirQ);
            }
            else {
                SessionListMode(logger, sessionListPath, dirQ);
            }
            BinWallManager.ReconfigureGazeOffsetCache(radius, density);
            ProcessExperimentQueue(dirQ, logger, numofLengthBins);
        }
    }

    private void SessionListMode(BatchModeLogger logger, string listPath, Queue<DirectoryInfo> dirQ) {
        using (StreamReader reader = new StreamReader(listPath)) {

            while (reader.Peek() > 0) {
                DirectoryInfo dir = new DirectoryInfo(reader.ReadLine());
                dirQ.Enqueue(dir);
            }
        }
    }

    private void PwdMode(BatchModeLogger logger, Queue<DirectoryInfo> dirQ) {
        DirectoryInfo pwd = new DirectoryInfo(PresentWorkingDirectory);
        dirQ.Enqueue(pwd);
    }

    private void ProcessExperimentQueue(Queue<DirectoryInfo> dirQ, BatchModeLogger logger, int numOfBinsForFloorLength) {
        Queue<string> sessionQ = new Queue<string>();

        while (dirQ.Count > 0) {
            DirectoryInfo dir = dirQ.Dequeue();
            if (IsDayDir(dir)) {
                IEnumerable<string> subDirs = Directory.EnumerateDirectories(dir.FullName, "*", SearchOption.TopDirectoryOnly);
                foreach (string subDir in subDirs) {
                    if (IsSessionDir(new DirectoryInfo(subDir))) {
                        logger.Print($"Queuing {subDir}");
                        sessionQ.Enqueue(subDir);
                    }
                }
            }
            else if (IsSessionDir(dir)) {
                logger.Print($"Queuing {dir}");
                sessionQ.Enqueue(dir.FullName);
            }
        }

        BinMapper mapper = new DoubleTeeBinMapper(numOfBinsForFloorLength);

        if (sessionQ.Count > 0) {
            logger.Print($"{sessionQ.Count} sessions to be processed");
            ProcessSession(sessionQ, logger, mapper);
        }
        else {
            logger.Print("No Session directories found! Exiting");
            logger.Dispose();
            Application.Quit();
        }
    }

    private async void ProcessSession(Queue<string> sessions, BatchModeLogger logger, BinMapper mapper) {
        string path;
        int total = sessions.Count;
        int count = 1, notifyAliveCount = 0;

        while (sessions.Count > 0) {
            path = sessions.Dequeue();
            logger.Print($"Starting({count}/{total}): {path}");

            StartCoroutine(ProcessWrapper(path + unityfileMatFile, path + eyelinkMatFile, path, mapper));
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

    private IEnumerator ProcessWrapper(string sessionPath, string edfPath, string toFolderPath, BinMapper mapper) {
        print($"session: {sessionPath}");
        print($"edf: {edfPath}");
        print($"toFolder: {toFolderPath}");

        generationComplete = false;
        try {
            yield return saver.ProcessSessionDataTask(sessionPath, edfPath, toFolderPath, mapper);
        }
        finally { //so that the batchmode app will quit or move on the the next session
            generationComplete = true;
        }
    }
}
