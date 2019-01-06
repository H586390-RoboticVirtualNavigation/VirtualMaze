using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// Wrapper Class for storing SerializableSettings.
/// 
/// Components with their individual configurations can be easily added
/// and removed from the settings.
/// </summary>
[Serializable]
public class ExperimentSettings : Dictionary<String, ComponentSettings> {
    public ExperimentSettings() : base() { }
    //required constructor to serialize properly
    protected ExperimentSettings(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public void TryGetComponentSetting(Type settingType, out ComponentSettings settings) {
        TryGetValue(settingType.FullName, out settings);
    }

    public void TryGetComponentSetting<T>(out ComponentSettings settings) where T : ComponentSettings {
        TryGetValue(typeof(T).FullName, out settings);
    }
}
