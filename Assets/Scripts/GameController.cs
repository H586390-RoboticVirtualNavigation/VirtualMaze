using HDF.PInvoke;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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

        if (Application.isBatchMode) {
            BatchModeLogger logger = new BatchModeLogger(PresentWorkingDirectory);

            string[] args = Environment.GetCommandLineArgs();
            bool isSessionList = false;
            for (int i = 0; i < args.Length; i++) {
                Debug.LogError($"ARG {i}: {args[i]}");
                if (args[i].ToLower().Equals("-sessionlist")) {
                    isSessionList = true;
                    logger.Print($"Session List detected!");
                    Debug.LogError($"{args[i + 1]}");
                    SessionListMode(logger, args[i + 1]);
                }
            }
            if (!isSessionList) {
                PwdMode(logger);
            }
        }

        long file = 0, g_id = 0, dset_id = 0;

        try {
            print(H5.open());
            string path = @"D:\Desktop\NUS\FYP\rawdata\20180824\unityfile.mat";
            UnityMazeMatFile f = new UnityMazeMatFile(path);
            file = H5F.open(path, H5F.ACC_RDWR);
            g_id = H5G.open(file, "/um/data");

            H5G.info_t t = new H5G.info_t();

            byte[] buffer = new byte[255];

            int counter = 0;

            GCHandle h1 = GCHandle.Alloc(buffer);


            while (0 < H5O.visit(g_id, H5.index_t.NAME, H5.iter_order_t.INC, asd, (IntPtr)h1) && counter < 100) {
                counter++;
                print($"looper {counter}");
            }

            h1.Free();


            dset_id = H5D.open(file, "/um/data/unityData");

            long space = H5D.get_space(dset_id);

            int a = H5S.get_simple_extent_ndims(space);
            ulong[] dimms = new ulong[a];

            print($"isSimple: {H5S.is_simple(space)}");

            H5S.get_simple_extent_dims(space, dimms, null);

            H5S.close(space);

            H5G.get_info(g_id, ref t);
            long type = H5D.get_type(dset_id);

            H5T.class_t clas = H5T.get_class(type);

            H5T.close(type);

            double[,] vs = new double[dimms[0], dimms[1]];
            print(vs.Length);

            GCHandle h2 = GCHandle.Alloc(vs, GCHandleType.Pinned);

            try {

                H5D.read(dset_id, H5T.NATIVE_DOUBLE, H5S.ALL, H5S.ALL, H5P.DEFAULT, h2.AddrOfPinnedObject());
            }
            catch (Exception e) {
                Debug.LogError("ERROR");
                Debug.LogException(e);
            }
            finally {
                h2.Free();
            }

            for (int i = 0; i < 5; i++) {

                print(vs[i, 0]);
                print(f.unityData[i, 0]);
            }



            print($"{file}, {g_id}, {dset_id}, {space} {a} {clas} {dimms[0]} {dimms[1]}");
        }
        finally {
            H5G.close(dset_id);
            H5G.close(g_id);
            H5F.close(file);
        }
    }

    H5O.iterate_t iterate = new H5O.iterate_t(asd);

    static int asd(long grp, IntPtr name, ref H5O.info_t info, IntPtr op_Data) {
        print($"whahahaha: {Marshal.PtrToStringAnsi(name)}");
        return 0;
    }

    private void SessionListMode(BatchModeLogger logger, string listPath) {
        using (StreamReader reader = new StreamReader(listPath)) {
            Queue<DirectoryInfo> dirQ = new Queue<DirectoryInfo>();
            while (reader.Peek() > 0) {
                DirectoryInfo dir = new DirectoryInfo(reader.ReadLine());
                dirQ.Enqueue(dir);
            }

            ProcessExperimentQueue(dirQ, logger);
        }
    }

    private void PwdMode(BatchModeLogger logger) {
        DirectoryInfo pwd = new DirectoryInfo(PresentWorkingDirectory);
        Queue<DirectoryInfo> q = new Queue<DirectoryInfo>();
        q.Enqueue(pwd);
        ProcessExperimentQueue(q, logger);
    }

    private void ProcessExperimentQueue(Queue<DirectoryInfo> dirQ, BatchModeLogger logger) {
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
}
