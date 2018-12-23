using UnityEngine;

/// <summary>
/// Interface to encapsulate configuration data that needs to be saved
/// 
/// A serializable subclass must be created in the desired class with savable settings
/// and this will be the generic type that must be passed in. Then, update 
/// </summary>
public interface IConfigurableComponent {
    SerializableSettings GetSavableSettings();
    SerializableSettings GetDefaultSettings();
    /// <summary>
    /// Identifies the subcomponentConfigs in the Settings
    /// 
    /// Recommended return statement: 
    /// <code> return typeof(ConfigurableComponent.SerializableSettings).Fullname </code>
    /// 
    /// Important: Developers must make sure this returns a unique string or bugs will occur
    /// (data not saved or wrong data overwritten.)
    /// </summary>
    /// <returns>Identifier of Congfig in Settings</returns>
    string GetConfigID();
    void ApplySavableSettings(SerializableSettings loadedSettings);
}
