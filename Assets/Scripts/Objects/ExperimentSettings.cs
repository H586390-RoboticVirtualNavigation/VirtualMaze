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

    public bool TryGetComponentSetting<T>(out T settings) where T : ComponentSettings {
        ComponentSettings s;
        if (TryGetValue(typeof(T).FullName, out s)) {
            settings = (T)s;
            return true;
        }
        settings = null;
        return false;
    }
}
