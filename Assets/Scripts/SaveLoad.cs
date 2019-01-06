using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveLoad : MonoBehaviour {


    /// <summary>
    /// SettingNotFoundException should be thrown when the setting is not found in the _settingsDictionary.
    /// </summary>
    public class SettingNotFoundException : Exception {
        public SettingNotFoundException() { }
        public SettingNotFoundException(string message) : base(message) { }
        public SettingNotFoundException(string message, Exception inner) : base(message, inner) { }
    };

    public static List<ConfigurableComponent> configurableComponentList =
        new List<ConfigurableComponent>();

    public List<string> SettingsList {
        get {
            if (_settingsDictionary == null) {
                return new List<string>();
            }
            return new List<string>(_settingsDictionary.Keys);
        }
    }

    private const string SettingsFileName = "/ExperiemntSettings.gd";
    private const string Msg_DeserializationWarning =
        "Settings unable to Deserialize. May be due to changing _settingsList's type." +
        "Try finding and deleting the file " + SettingsFileName + " to reset. " +
        "An empty _settingsList is created instead";
    private const string Msg_ComponentNotConfigurable =
        " is not configurable! Did you implement IConfigurableComponent?";
    private const string Msg_DefaultValuesUsed =
        " used default Values.";

    //A simple way of knowing where the settings file is saved it to Debug.Log(SaveFileLocation)
    //SaveFileLocation declared in Awake();
    private static string SaveFileLocation;

    //key: settingName, value: Settings
    private Dictionary<string, ExperimentSettings> _settingsDictionary;
    private BinaryFormatter bf = new BinaryFormatter();

    private void Awake() {
        SaveFileLocation = Application.persistentDataPath + SettingsFileName;

        //load saved settings (if any)
        if (File.Exists(SaveFileLocation)) {
            FileStream file = File.Open(SaveFileLocation, FileMode.Open);
            try {
                _settingsDictionary = bf.Deserialize(file) as Dictionary<string, ExperimentSettings>;
                if (_settingsDictionary == null) {
                    Debug.LogWarning(Msg_DeserializationWarning);
                    _settingsDictionary = new Dictionary<string, ExperimentSettings>();
                }
            }
            catch (Exception e) {
                // Fail with logging
                Debug.LogException(e);
                _settingsDictionary = new Dictionary<string, ExperimentSettings>();
            }
            finally {
                file.Close();
            }
        }
        else {
            _settingsDictionary = new Dictionary<string, ExperimentSettings>();
        }
    }

    public static void RegisterConfigurableComponent(ConfigurableComponent component) {
        configurableComponentList.Add(component);
    }

    public static ExperimentSettings getCurrentConfig() {
        ExperimentSettings s = new ExperimentSettings();

        foreach (ConfigurableComponent c in configurableComponentList) {
            s.Add(c.GetSettingsID(), c.GetCurrentSettings());
        }

        return s;
    }

    /// <summary>
    /// Saves or replaces current setting. This will also trigger saving all known settings into the 
    /// external save file.
    /// </summary>
    /// <returns>true if existing settings replaced, false if is new setting</returns>
    public bool SaveSetting(string settingsName) {
        ExperimentSettings settings = getCurrentConfig();

        //remove and add to update settings
        bool isReplaced = _settingsDictionary.Remove(settingsName);
        _settingsDictionary.Add(settingsName, settings);

        WriteFile();

        return isReplaced;
    }

    /// <summary>
    /// Removes current setting. This will also trigger saving all remaining settings into the 
    /// external save file.
    /// </summary>
    /// <param name="settingsName">Name of Setting to be removed</param>
    /// <returns>true if deleted sucessfully</returns>
    public bool DeleteSetting(string settingsName) {
        bool success = _settingsDictionary.Remove(settingsName);
        if (success) {
            WriteFile();
        }
        return success;
    }

    public void ApplySettings(String settingName) {
        if (!_settingsDictionary.TryGetValue(settingName, out ExperimentSettings savedSetting)) {
            throw new SettingNotFoundException();
        }

        foreach (ConfigurableComponent component in configurableComponentList) {
            //if apply ComponentConfig if exist, else use default.
            if (savedSetting.TryGetValue(component.GetSettingsID(), out ComponentSettings componentConfig)) {
                component.LoadSavableSettings(componentConfig);
            }
            else {
                Debug.LogWarning(component.GetSettingsID() + Msg_DefaultValuesUsed);
                component.LoadSavableSettings(component.GetDefaultSettings());
            }
        }
    }

    /// <summary>
    /// Creates and serialize settings file from _settingsDictionary.
    /// </summary>
    private void WriteFile() {
        //Creates or overrides file
        FileStream file = File.Create(SaveFileLocation);

        bf.Serialize(file, _settingsDictionary);
        file.Close();
    }
}
