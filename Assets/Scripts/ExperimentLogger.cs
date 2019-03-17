using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Logs information regarding the Experiment in to a text file.
/// Each experiment should have its own ExperimentLogger.
/// Each Session will be logged into its individual file.
/// </summary>
/// <example>
/// <code>
///     //cache the reference
///     private ExperimentLogger logger;
///
///     private void OnExperimentStart() {
///         //creates a new ExperimentLogger
///         logger = new ExperimentLogger(SaveLocation, ExperimentLogger.GenerateDefaultExperimentID());
///     }
///
///     private void BeforeSessionStart() {
///         //creates and opens the log file for the current session
///         logger.OpenSessionLog(sessionNum, session, currentSettings);
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

    public ExperimentLogger(string saveLocation, string experimentID) {
        SetSaveLocation(saveLocation);
        SetExperimentId(experimentID);
    }

    public void SetSaveLocation(string saveLocation) {
        this.saveLocation = saveLocation;
    }

    public void SetExperimentId(string experimentID) {
        this.experimentID = experimentID;
    }

    /// <summary>
    /// Opens a file to store the logs of the current session
    /// </summary>
    /// <param name="sessionNum">Index of the current session that is currently being logged</param>
    /// <param name="session">Session object describing the current Session</param>
    /// <param name="currentSettings">Current Settings of the Experiment</param>
    /// <param name="rewards">Array of rewards in the level</param>
    /// <returns>True if log is opened successfully</returns>
    public bool OpenSessionLog(int sessionNum, Session session, ExperimentSettings currentSettings, RewardArea[] rewards) {
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

        WriteHeader(fs, session, currentSettings, rewards);

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
    }

    private void WriteHeader(StreamWriter fs, Session session, ExperimentSettings settings, RewardArea[] rewards) {
        SessionContext context = new SessionContext(session, settings, rewards);
        fs.WriteLine(context.ToJsonString());
        
        //fs.WriteLine("Version: {0}", GameController.versionInfo);
        //fs.WriteLine("Trigger: {0}", GameController.pportInfo);
        //fs.WriteLine("TaskType: Continuous");
        //LogRewardPositions(fs, rewards);
        //fs.WriteLine("TrialType: {0}", session.level);
        //fs.WriteLine("SpecifiedRewardNo: {0}", session.numTrial);

        //LogExperimentSettings(fs, settings);
        //LogRewardSettings(fs, settings);
        //LogRobotMovementSettings(fs, settings);
        //LogJoystickSettings(fs, settings);

        fs.Flush();//call flush to write to file
    }

    private string FileName(int sessionNum) {
        return string.Format(Format_Filename, sessionNum, experimentID);
    }

    //helper methods to log required settings
    private void LogJoystickSettings(StreamWriter fs, ExperimentSettings settings) {
        if (settings.TryGetComponentSetting(out JoystickController.Settings joystickSettings)) {
            fs.WriteLine("JoystickDeadzone: {0}", joystickSettings.deadzoneAmount);
        }
        else {
            //this values are a must to be logged. Therefore an exception is thrown.
            throw new SaveLoad.SettingNotFoundException("JoystickController.Settings not found");
        }
    }

    private void LogRobotMovementSettings(StreamWriter fs, ExperimentSettings settings) {
        if (settings.TryGetComponentSetting(out RobotMovement.Settings movementSettings)) {
            fs.WriteLine("RotationSpeed: {0}", movementSettings.rotationSpeed); // robotMovement
            fs.WriteLine("TranslationSpeed: {0}", movementSettings.movementSpeed); // robotMovement
        }
        else {
            //this values are a must to be logged. Therefore an exception is thrown.
            throw new SaveLoad.SettingNotFoundException("RobotMovement.Settings not found");
        }
    }

    private void LogRewardSettings(StreamWriter writer, ExperimentSettings settings) {
        if (settings.TryGetComponentSetting(out RewardsController.Settings rewardSettings)) {
            fs.WriteLine("RewardTime: {0}", rewardSettings.rewardDurationMilliSecs);
            fs.WriteLine("RewardViewCriteria: {0}", rewardSettings.requiredViewAngle);
        }
        else {
            //this values are a must to be logged. Therefore an exception is thrown.
            throw new SaveLoad.SettingNotFoundException("RewardsController.Settings not found");
        }
    }

    private void LogExperimentSettings(StreamWriter writer, ExperimentSettings settings) {
        if (settings.TryGetComponentSetting(out ExperimentController.Settings experimentSettings)) {
            fs.WriteLine("CompletionWindow: {0}", experimentSettings.timeLimitDuration);
            fs.WriteLine("TimeoutDuration: {0}", experimentSettings.timeoutDuration);
            fs.WriteLine("IntersessionInterval: {0}", experimentSettings.sessionIntermissionDuration);
        }
        else {
            //this values are a must to have. Therefore an exception is thrown
            throw new SaveLoad.SettingNotFoundException("ExperimentController.Settings not found");
        }
    }

    private void LogRewardPositions(StreamWriter writer, RewardArea[] rewards) {
        fs.Write("PosterLocations:");
        Vector3 posterPosition;
        foreach(RewardArea reward in rewards) {
            posterPosition = reward.target.transform.position;
            //format: 'name(x,y,z) '. Maximum 2 decimal places for float positions
            fs.Write("{0}({1:0.##},{2:0.##},{3:0.##}) ", reward.target.name, posterPosition.x, posterPosition.y, posterPosition.z);
        }
        fs.WriteLine();
    }
}
