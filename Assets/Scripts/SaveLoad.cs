using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveLoad : MonoBehaviour {
    /// <summary>
    /// Dictioanry is not serializable in Unity or C#. Therefore ISerializationCallbackReceiver is 
    /// used to translate the dictionary to Lists.
    /// </summary>
    [Serializable]
    public class Settings : Dictionary<string, SerializableSettings>, ISerializationCallbackReceiver {
        public Settings() { }
        public Settings(SerializationInfo info, StreamingContext context) {

        }

        [SerializeField]
        private List<string> keys = new List<string>();

        [SerializeField]
        private List<SerializableSettings> values = new List<SerializableSettings>();

        [NonSerialized]
        private string MsgSettingsDeserializeError = "there are {0} keys and {1} values" +
            " after deserialization. Make sure that both key and value types are serializable.";

        public class SettingsDeserializationException : Exception {
            public SettingsDeserializationException() { }
            public SettingsDeserializationException(string message) : base(message) { }
            public SettingsDeserializationException(string message, Exception inner) : base(message, inner) { }
        }

        public void OnAfterDeserialize() {
            if (keys.Count != values.Count)
                throw new SettingsDeserializationException(string.Format(MsgSettingsDeserializeError, 
                    keys.Count, values.Count));

            this.Clear();
            for(int i = 0; i < keys.Count; i++) {
                this.Add(keys[i], values[i]);
            }
        }

        public void OnBeforeSerialize() {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<string, SerializableSettings> kvp in this) {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }
    }

    /// <summary>
    /// SettingNotFoundException should be thrown when the setting is not found in the _settingsDictionary.
    /// </summary>
    public class SettingNotFoundException : Exception {
        public SettingNotFoundException() { }
        public SettingNotFoundException(string message) : base(message) { }
        public SettingNotFoundException(string message, Exception inner) : base(message, inner) { }
    };

    /// <summary>
    /// Could not make this show up in Unity GUI. Therefore used a list of Monobehaviours to poplutae this list
    /// in Awake()
    /// </summary>
    public List<IConfigurableComponent> configurableComponentList =
        new List<IConfigurableComponent>();

    public List<MonoBehaviour> components = new List<MonoBehaviour>();

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

    //A simple way of knowing where the settings file is saved it to Debug.Log(SaveFileLocation)
    private static string SaveFileLocation;
    private Dictionary<string, Settings> _settingsDictionary;
    private BinaryFormatter bf = new BinaryFormatter();

    private void Awake() {
        SaveFileLocation = Application.persistentDataPath + SettingsFileName;

        foreach (MonoBehaviour script in components) {
            IConfigurableComponent configurableComponent =
                script as IConfigurableComponent;

            if (configurableComponent != null) {
                configurableComponentList.Add(configurableComponent);
            }
            else {
                Debug.LogWarning(configurableComponent.ToString() + Msg_ComponentNotConfigurable);
            }
        }

        //load saved settings (if any)
        if (File.Exists(SaveFileLocation)) {
            FileStream file = File.Open(SaveFileLocation, FileMode.Open);
            try {
                _settingsDictionary = bf.Deserialize(file) as Dictionary<string, Settings>;
                if (_settingsDictionary == null) {
                    Debug.LogWarning(Msg_DeserializationWarning);
                    _settingsDictionary = new Dictionary<string, Settings>();
                }
            }
            catch (Exception e) {
                // Fail with logging
                Debug.LogException(e);
                _settingsDictionary = new Dictionary<string, Settings>();
            }
            finally {
                file.Close();
            }
        }
        else {
            _settingsDictionary = new Dictionary<string, Settings>();
        }
    }

    /// <summary>
    /// Saves or replaces current settings. This will also trigger saving all known settings into the 
    /// external save file.
    /// </summary>
    /// <returns>true if existing settings replaced, false if is new setting</returns>
    public bool SaveSetting(string settingsName) {
        Settings settings = new Settings();

        foreach (IConfigurableComponent savable in configurableComponentList) {
            settings.Add(savable.GetConfigID(), savable.GetSavableSettings());
        }

        Debug.Log(_settingsDictionary.Count);

        //remove and add to update settings
        bool isReplaced = _settingsDictionary.Remove(settingsName);
        _settingsDictionary.Add(settingsName, settings);

        WriteFile();

        return isReplaced;
    }

    /// <summary>
    /// Removes current settings from memory. This will also trigger saving all remaining settings into the 
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

        if (!_settingsDictionary.TryGetValue(settingName, out Settings savedSetting)) {
            throw new SettingNotFoundException();
        }

        foreach (IConfigurableComponent component in configurableComponentList) {
            //if apply ComponentConfig if exist, else use default.
            if (savedSetting.TryGetValue(component.GetConfigID(), out SerializableSettings componentConfig)) {
                component.ApplySavableSettings(componentConfig);
            }
            else {
                component.ApplySavableSettings(component.GetDefaultSettings());
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
