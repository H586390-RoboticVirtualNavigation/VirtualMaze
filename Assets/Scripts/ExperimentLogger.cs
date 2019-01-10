using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class ExperimentLogger {
    private const string Format_DefaultExperimentId = "dMyyyyHHms";
    private const string Format_Filename = "session_{0}_{1}.txt";
    private const string Msg_StreamNotClosed = "Previous Stream not closed. Closing Previous Stream";
    private const string Msg_StreamNotOpened = "Unable to Open Stream";

    /// <summary>
    /// Default ID generator
    /// </summary>
    /// <returns></returns>
    public static string GenerateDefaultExperimentID() {
        return DateTime.Now.ToString(Format_DefaultExperimentId);
    }

    public string SaveLocation { get; set; }
    public string ExperimentID { get; set; }

    private StreamWriter fs;

    public ExperimentLogger(string saveLocation, string experimentID) {
        SaveLocation = saveLocation;
        ExperimentID = experimentID;
    }

    public bool OpenLog(int sessionNum, Session session, ExperimentSettings currentConfig) {
        //help close fs if not previously closed
        if (fs != null) {
            Debug.LogWarning(Msg_StreamNotClosed);
            CloseLog();
        }

        fs = new StreamWriter(Path.Combine(SaveLocation, FileName(sessionNum)));
        //fs = FileWriter.CreateFileInFolder(SaveLocation, FileName(sessionNum));
        if (fs == null) {
            Debug.LogError(Msg_StreamNotOpened);
            return false;
        }

        WriteHeader(fs, session, currentConfig);

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

    private string FileName(int sessionNum) {
        return string.Format(Format_Filename, sessionNum, ExperimentID);
    }

    private void WriteHeader(StreamWriter fs, Session session, ExperimentSettings settings) {
        fs.WriteLine("Version: {0}", GameController.versionInfo);
        fs.WriteLine("Trigger: {0}", GameController.pportInfo);
        fs.WriteLine("TaskType: Continuous");
        fs.WriteLine("PosterLocations: P1(-5,1.5,-7.55) P2(-7.55,1.5,5) P3(7.55,1.5,-5) P4(5,1.5,7.55) P5(5,1.5,2.45) P6(-5,1.5,-2.45)");
        fs.WriteLine("TrialType: {0}", session.level);
        fs.WriteLine("SpecifiedRewardNo: {0}", session.numTrial);

        LogExperimentSettings(fs, settings);
        LogRewardSettings(fs, settings);
        LogRobotMovementSettings(fs, settings);
        LogJoystickSettings(fs, settings);

        fs.Flush();//call flush to write to file
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
}
