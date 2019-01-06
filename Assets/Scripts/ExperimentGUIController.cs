using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentGUIController : SettingsGUIController {

    //Drag from Unity Editor
    public InputField trialIntermissionFixedField;
    public InputField trialIntermissionMaxField;
    public InputField trialIntermissionMinField;

    public InputField saveLocationField;
    public InputField sessionIntermissionDurationField;
    public InputField timeoutDurationField;
    public InputField timeLimitField;

    public Toggle fixedTrailIntermissionToggle;
    public Toggle randomTrailIntermissionToggle;

    public Toggle posterEnableValid;
    public Toggle saveLocationValid;
    public Toggle sessionIntermissionValid;
    public Toggle timeoutDurationValid;
    public Toggle timeLimitValid;

    public ExperimentController experimentController;

    private void Awake() {
        trialIntermissionFixedField.onEndEdit.AddListener(OnTrialIntermissionFixedEndEdit);
        trialIntermissionMaxField.onEndEdit.AddListener(OnTrialIntermissionMaxEndEdit);
        trialIntermissionMinField.onEndEdit.AddListener(OnTrialIntermissionMinEndEdit);

        saveLocationField.onEndEdit.AddListener(OnSaveLocationFieldEndEdit);
        sessionIntermissionDurationField.onEndEdit.AddListener(OnSessionIntermissionFieldEndEdit);
        timeoutDurationField.onEndEdit.AddListener(OnTimeoutDurationFieldEndEdit);
        timeLimitField.onEndEdit.AddListener(OnTimeLimitFieldEndEdit);

        fixedTrailIntermissionToggle.onValueChanged.AddListener(OnFixedTrialIntermissionToggleChanged);
        randomTrailIntermissionToggle.onValueChanged.AddListener(OnRandomTrialIntermissionToggleChanged);
    }

    private void OnFixedTrialIntermissionToggleChanged(bool value) {
        experimentController.IsTrialIntermissionFixed = value;
    }
    private void OnRandomTrialIntermissionToggleChanged(bool value) {
        experimentController.IsTrialIntermissionFixed = !value;
    }

    private void OnTimeLimitFieldEndEdit(string text) {
        if (IsValidDuration(text, out int duration)) {
            experimentController.TimeLimitDuration = duration;
        }
        else {
            text = experimentController.TimeLimitDuration.ToString();
        }
    }

    private void OnSessionIntermissionFieldEndEdit(string text) {
        if (IsValidDuration(text, out int duration)) {
            experimentController.SessionIntermissionDuration = duration;
        }
        else {
            text = experimentController.SessionIntermissionDuration.ToString();
        }
    }

    private void OnTimeoutDurationFieldEndEdit(string text) {
        if (IsValidDuration(text, out int duration)) {
            experimentController.TimeoutDuration = duration;
        }
        else {
            text = experimentController.TimeoutDuration.ToString();
        }
    }

    private void OnTrialIntermissionFixedEndEdit(string text) {
        if (IsValidDuration(text, out int duration)) {
            experimentController.FixedTrialIntermissionDuration = duration;
        }
        else {
            text = experimentController.FixedTrialIntermissionDuration.ToString();
        }
    }

    private void OnTrialIntermissionMaxEndEdit(string text) {
        if (IsValidDuration(text, out int duration)) {
            experimentController.MaxTrialIntermissionDuration = duration;
        }
        else {
            text = experimentController.MaxTrialIntermissionDuration.ToString();
        }
    }

    private void OnTrialIntermissionMinEndEdit(string text) {
        if (IsValidDuration(text, out int duration)) {
            experimentController.MinTrialIntermissionDuration = duration;
        }
        else {
            text = experimentController.MinTrialIntermissionDuration.ToString();
        }
    }

    private void OnSaveLocationFieldEndEdit(string text) {
        if (IsValidSaveLocation(text)) {
            experimentController.SaveLocation = text;
            SetInputFieldValid(saveLocationField);
        }
        else {
            SetInputFieldInvalid(saveLocationField);
        }
    }

    private bool IsValidSaveLocation(string path) {
        return Directory.Exists(path);
    }

    private bool IsValidDuration(string text, out int duration) {
        int result = -1;
        if (int.TryParse(text, out result)) {
            duration = result;
            return IsValidDuration(duration);
        }
        duration = result;
        return false;
    }

    private bool IsValidDuration(int duration) {
        return duration > 0;
    }

    public override void UpdateSettingsGUI() {
        trialIntermissionFixedField.text = experimentController.FixedTrialIntermissionDuration.ToString();
        trialIntermissionMaxField.text = experimentController.MaxTrialIntermissionDuration.ToString();
        trialIntermissionMinField.text = experimentController.MinTrialIntermissionDuration.ToString();

        saveLocationField.text = experimentController.SaveLocation;
        sessionIntermissionDurationField.text = experimentController.SessionIntermissionDuration.ToString();
        timeoutDurationField.text = experimentController.TimeoutDuration.ToString();
        timeLimitField.text = experimentController.TimeLimitDuration.ToString();

        Debug.LogWarning(experimentController.IsTrialIntermissionFixed);

        if (experimentController.IsTrialIntermissionFixed) {
            fixedTrailIntermissionToggle.isOn = true;
        }
        else {
            randomTrailIntermissionToggle.isOn = true;
        }

        posterEnableValid.isOn = experimentController.PostersEnabled;
        saveLocationValid.isOn = IsValidSaveLocation(experimentController.SaveLocation);
        sessionIntermissionValid.isOn = IsValidDuration(experimentController.SessionIntermissionDuration);
        timeoutDurationValid.isOn = IsValidDuration(experimentController.TimeoutDuration);
        timeLimitValid.isOn = IsValidDuration(experimentController.TimeLimitDuration);
    }
}
