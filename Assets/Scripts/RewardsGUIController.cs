using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardsGUIController : SettingsGUIController {
    private const string Text_OnState = "Valve On";
    private const string Text_OffState = "Valve Off";

    //Drag in from Unity GUI
    public InputField portNumField;
    public InputField rewardDurationField;
    public Toggle rewardDurationValid;
    public Text valveStateText;
    public MySliderScript requiredViewAngleSlider;
    public MySliderScript requiredDistanceSlider;
    public RewardsController rewardsController;

    private void Awake() {
        portNumField.onEndEdit.AddListener(OnPortNumFieldEndEdit);
        rewardDurationField.onEndEdit.AddListener(OnDurationFieldEndEdit);

        requiredViewAngleSlider.OnValueChanged.AddListener(OnRequiredViewAngleChanged);
        requiredDistanceSlider.OnValueChanged.AddListener(OnRequiredDistanceChanged);
    }

    private void OnRequiredViewAngleChanged(float value) {
        RewardArea.requiredViewAngle = value;
    }

    public void ToggleValveState() {
        if (!rewardsController.IsPortOpen) {
            if (rewardsController.RewardValveOn()) {
                valveStateText.text = Text_OffState;
                SetInputFieldValid(portNumField);
            }
            else {
                Console.Write("cant open reward serial");
                SetInputFieldInvalid(portNumField);
            }
        }
        else {
            rewardsController.RewardValveOff();
            valveStateText.text = Text_OnState;
        }
    }

    public void OnPortNumFieldEndEdit(string input) {
        //check input and place update RewardsController
        rewardsController.portNum = input;
    }

    public void OnDurationFieldEndEdit(string input) {
        if (int.TryParse(input, out int duration)) {
            rewardsController.rewardDurationMilliSecs = duration;
        }
    }

    public void OnRequiredDistanceChanged(float value) {
        RewardArea.requiredDistance = value;
    }

    public override void UpdateSettingsGUI() {
        portNumField.text = rewardsController.portNum;
        SetInputFieldNeutral(portNumField);

        string millis = rewardsController.rewardDurationMilliSecs.ToString();
        rewardDurationField.text = millis;
        rewardDurationValid.isOn = IsDurationInputValid(millis);

        requiredViewAngleSlider.value = RewardArea.requiredViewAngle;
        requiredDistanceSlider.value = RewardArea.requiredDistance;
    }

    private bool IsDurationInputValid(string duration) {
        if (string.IsNullOrEmpty(duration)) {
            return false;
        }
        if (int.TryParse(duration, out int milliseconds)) {
            return IsDurationInputValid(milliseconds);
        }
        return false;
    }

    private bool IsDurationInputValid(int duration) {
        if (duration >= 0) {
            return true;
        }
        return false;
    }
}
