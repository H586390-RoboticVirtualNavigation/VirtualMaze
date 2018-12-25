using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Interface to encapsulate configuration data that needs to be saved
/// 
/// A serializable subclass must be created in the desired class with savable settings
/// and this will be the generic type that must be passed in. Then, update 
/// </summary>
public abstract class ConfigurableComponent : MonoBehaviour {
    public UnityEvent OnConfigChanged = new UnityEvent();

    public abstract SerializableSettings GetSavableSettings();

    public abstract SerializableSettings GetDefaultSettings();

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
    public abstract string GetConfigID();

    protected abstract void ApplySavableSettings(SerializableSettings loadedSettings);

    public void LoadSavableSettings(SerializableSettings loadedSettings) {
        ApplySavableSettings(loadedSettings);
        OnConfigChanged.Invoke();
    }
}
