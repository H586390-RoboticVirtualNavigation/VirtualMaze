using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Class for encapsulation the data required to parse and unparse settings to be logged into
/// the session Logs.
/// </summary>
[Serializable]
public class SessionContext {
    public string version;
    public string triggerVersion;
    public string taskType;
    public List<PosterLocation> posterLocations = new List<PosterLocation>();
    public string trialName;
    public int rewardsNumber; //number of trials
    public int completionWindow;
    public int timeoutDuration;
    public int intersessionInterval;
    public int rewardTime;
    public int rewardDistance;
    public float rewardViewCriteria;
    public float rotationSpeed;
    public float movementSpeed;
    public float joystickDeadzone;

    public SessionContext(Session session, ExperimentSettings settings, RewardArea[] rewards) {
        version = GameController.versionInfo;
        triggerVersion = GameController.pportInfo;
        taskType = "Continuous";
        trialName = session.level;
        rewardsNumber = session.numTrial;

        foreach (RewardArea reward in rewards) {
            posterLocations.Add(new PosterLocation(reward.target.position, reward.target.name));
        }

        GetJoystickSettings(settings);
        GetRobotMovementSettings(settings);
        GetRewardSettings(settings);
        GetExperimentSettings(settings);
    }

    //allow creation of empty object
    public SessionContext() {}

    public string ToJsonString() {
        return JsonUtility.ToJson(this, false);
    }

    public string ToJsonString(bool prettyPrint) {
        return JsonUtility.ToJson(this, prettyPrint);
    }

    //helper methods to log required settings
    private void GetJoystickSettings(ExperimentSettings settings) {
        if (settings.TryGetComponentSetting(out JoystickController.Settings joystickSettings)) {
            joystickDeadzone = joystickSettings.deadzoneAmount;
        }
        else {
            //this values are a must to be logged. Therefore an exception is thrown.
            throw new SaveLoad.SettingNotFoundException("JoystickController.Settings not found");
        }
    }

    private void GetRobotMovementSettings(ExperimentSettings settings) {
        if (settings.TryGetComponentSetting(out RobotMovement.Settings movementSettings)) {
            rotationSpeed = movementSettings.rotationSpeed;
            movementSpeed = movementSettings.movementSpeed;
        }
        else {
            //this values are a must to be logged. Therefore an exception is thrown.
            throw new SaveLoad.SettingNotFoundException("RobotMovement.Settings not found");
        }
    }

    private void GetRewardSettings(ExperimentSettings settings) {
        if (settings.TryGetComponentSetting(out RewardsController.Settings rewardSettings)) {
            rewardTime = rewardSettings.rewardDurationMilliSecs;
            rewardViewCriteria = rewardSettings.requiredViewAngle;
        }
        else {
            //this values are a must to be logged. Therefore an exception is thrown.
            throw new SaveLoad.SettingNotFoundException("RewardsController.Settings not found");
        }
    }

    private void GetExperimentSettings(ExperimentSettings settings) {
        if (settings.TryGetComponentSetting(out ExperimentController.Settings experimentSettings)) {
            completionWindow = experimentSettings.timeLimitDuration;
            timeoutDuration = experimentSettings.timeoutDuration;
            intersessionInterval = experimentSettings.sessionIntermissionDuration;
        }
        else {
            //this values are a must to have. Therefore an exception is thrown
            throw new SaveLoad.SettingNotFoundException("ExperimentController.Settings not found");
        }
    }

    [Serializable]
    public struct PosterLocation {
        public string name;
        public Vector3 position;
        public PosterLocation(Vector3 position, string name) {
            this.name = name;
            this.position = position;
        }
    }
}
