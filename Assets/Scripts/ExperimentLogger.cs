using System;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Logs information regarding the Experiment in to a text file.
/// Each experiment should have its own ExperimentLogger.
/// Each Session will be logged into its individual file.
/// </summary>
/// <example>
/// <code>
///     //cache the reference
///     private ExperimentLogger logger = new ExperimentLogger;
///
///     private void OnExperimentStart() {
///         //creates a new ExperimentLogger
///         logger.SetSaveLocation("SaveLocation");
///         logger.SetExperimentId(ExperimentLogger.GenerateDefaultExperimentID());
///     }
///
///     private void BeforeSessionStart() {
///         //creates and opens the log file for the current session
///         logger.OpenSessionLog(sessionNum, context);
///     }
///
///     private void WithinSession() {
///         //or any helper methods to write to the logs
///         logger.WriteLine("string to log");
///     }
///
///     private void AfterSessionEnd() {
///         //close log to prevent unwanted writes to the session log
///         logger.CloseLog();
///     }
///
///     private void OnExperimentEnd() {
///         //close log again for cleanup
///         logger.CloseLog();
///     }
/// </code>
/// </example>

public class ExperimentLogger {
    private const string Format_DefaultExperimentId = "dMyyyyHHms";
    //session number, experimentID
    private const string Format_Filename = "session_{0}_{1}.txt";
    //triggerValue, deltaTime, position x, position z, rotation y
    private const string Format_LogRobotMovement = " {0} {1:F8} {2:F4} {3:F4} {4:F4}";
    private const string Msg_StreamNotClosed = "Previous Stream not closed. Closing Previous Stream";
    private const string Msg_NotInitialized = "ExperiementLogger not initialize!";
    private const string Msg_StreamNotOpened = "Unable to Open Stream";

    /// <summary>
    /// Default ID generator
    /// </summary>
    /// <returns>Default ID for the session logs</returns>
    public static string GenerateDefaultExperimentID() {
        return DateTime.Now.ToString(Format_DefaultExperimentId);
    }

    private string saveLocation;
    private string experimentID;

    private StreamWriter fs;

    public void SetSaveLocation(string saveLocation) {
        this.saveLocation = saveLocation;
    }

    public void SetExperimentIdDefault() {
        SetExperimentId(GenerateDefaultExperimentID());
    }

    public void SetExperimentId(string experimentID) {
        this.experimentID = experimentID;
    }

    /// <summary>
    /// Opens a file to store the logs of the current session
    /// </summary>
    /// <param name="sessionNum">Index of the current session that is currently being logged</param>
    /// <param name="context">Configuration of the current session</param>
    /// <returns>True if log is opened successfully</returns>
    public bool OpenSessionLog(int sessionNum, SessionContext context) {
        //check if properly instantiated
        if (string.IsNullOrEmpty(saveLocation) || string.IsNullOrEmpty(experimentID)) {
            Debug.LogWarning(Msg_NotInitialized);
            return false;
        }

        //help close fs if not previously closed
        if (fs != null) {
            Debug.LogWarning(Msg_StreamNotClosed);
            CloseLog();
        }

        fs = new StreamWriter(Path.Combine(saveLocation, FileName(sessionNum)));
        //fs = FileWriter.CreateFileInFolder(SaveLocation, FileName(sessionNum));
        if (fs == null) {
            Debug.LogError(Msg_StreamNotOpened);
            return false;
        }

        WriteHeader(fs, context);

        return true;
    }

    public void CloseLog() {
        if (fs != null) {
            fs.Flush(); //write any lingering text to file
            fs.Dispose();
        }
        fs = null;
    }

    public void WriteLine(string data) {
        fs.WriteLine(data);
        fs.Flush();
    }

    /// <summary>
    /// Log movement with no trigger
    /// </summary>
    /// <param name="robot">transform of the robot GameObject</param>
    public void LogMovement(Transform robot) {
        LogMovement(SessionTrigger.NoTrigger, 0, robot);
    }

    public void LogMovement(SessionTrigger trigger, int rewardIndex, Transform robot) {
        Profiler.BeginSample("Log Trigger");
        WriteLine(
            string.Format(
                Format_LogRobotMovement,
                (int)trigger + rewardIndex,
                Time.deltaTime,
                robot.position.x,
                robot.position.z,
                robot.rotation.eulerAngles.y
            )
        );
        Profiler.EndSample();
    }

    private void WriteHeader(StreamWriter fs, SessionContext context) {
        fs.WriteLine(context.ToJsonString());
        fs.Flush();//call flush to write to file
    }

    private string FileName(int sessionNum) {
        return string.Format(Format_Filename, sessionNum, experimentID);
    }
}
