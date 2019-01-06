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

    public bool Open(int sessionNum, string sessionName, ExperimentSettings currentConfig) {
        //help close fs if not previously closed
        if (fs != null) {
            Debug.LogWarning(Msg_StreamNotClosed);
            Close();
        }

        fs = new StreamWriter(Path.Combine(SaveLocation, FileName(sessionNum)));

        if (fs == null) {
            Debug.LogError(Msg_StreamNotOpened);
            return false;
        }

        WriteHeader(fs, sessionName, currentConfig);

        return true;
    }

    public void Close() {
        fs.Dispose();
        fs = null;
    }

    public void Write(string data) {
        fs.Write(data);
    }

    private string FileName(int sessionNum) {
        return string.Format(Format_Filename, sessionNum, ExperimentID);
    }

    private void WriteHeader(StreamWriter fs, string sessionName, ExperimentSettings s) {
        fs.WriteLine("Version: {0}", GameController.versionInfo);
        fs.WriteLine("Trigger: {0}", GameController.pportInfo);
        fs.WriteLine("TaskType: Continuous");
        fs.WriteLine("PosterLocations: P1(-5,1.5,-7.55) P2(-7.55,1.5,5) P3(7.55,1.5,-5) P4(5,1.5,7.55) P5(5,1.5,2.45) P6(-5,1.5,-2.45)");
        fs.WriteLine("TrialType: {0}", sessionName);
        fs.WriteLine("SpecifiedRewardNo: {0}", InputRewardNo.inputrewardno); //session
        fs.WriteLine("CompletionWindow: {0}", GuiController.completionWindowTime); // experiment
        fs.WriteLine("TimeoutDuration: {0}", GuiController.timoutTime); // experiment
        fs.WriteLine("IntersessionInterval: {0}", GuiController.interSessionTime); // experiment
        fs.WriteLine("RewardTime: {0}", GuiController.rewardTime); // rewards

        //s.TryGetValue(typeof(RobotMovement.Settings).FullName, out RobotMovement.Settings t as RobotMovement.Settings);
        //fs.WriteLine("RotationSpeed: {0}", t.rotationSpeedSlider.value); // robotMovement
        //fs.WriteLine("TranslationSpeed: {0}", GuiController.translationSpeedSlider.value); // robotMovement
        //fs.WriteLine("JoystickDeadzone: {0}", GuiController.joystickDeadzoneSlider.value); // robotMovement
        fs.WriteLine("RewardViewCriteria: {0}", GuiController.rewardViewCriteriaSlider.value); //undecided
    }
}
