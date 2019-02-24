using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A serializable subclass must be created and filled with the parameters to save.
/// 
/// Create a subclass which extends SerializableSettings and add the [System.Serializable] attribute.
/// 
/// Update the implemented methods to return the subclass.
/// 
/// To add your ConfigurableComponent to SaveLoad, use SaveLoad.registerConfigurableComponent
/// and the Configs will be saved.
/// </summary>
public abstract class ConfigurableComponent : MonoBehaviour {
    public UnityEvent OnConfigChanged = new UnityEvent();

    /// <summary>
    /// Return the current Settings of this Component.
    /// </summary>
    /// <returns>Returns a SerializableSettings subclass which contains this components's current settings</returns>
    public abstract ComponentSettings GetCurrentSettings();

    /// <summary>
    /// Return the default Settings of this Component.
    /// </summary>
    /// <returns>Returns a SerializableSettings subclass which contains this components's default settings</returns>
    public abstract ComponentSettings GetDefaultSettings();

    /// <summary>
    /// Identifies the ComponentSettings in the Settings
    /// 
    /// Recommended return statement: 
    /// <code> return typeof(ConfigurableComponent.SerializableSettings) </code>
    /// 
    /// Type.Fullname is used to identify the componentSettings as Type is not serializable.
    /// 
    /// </summary>
    /// <returns>Type of the inner class containing the Settings to save.</returns>
    public abstract Type GetSettingsType();
    
    /// <summary>
    /// Applies the settings to the component.
    /// </summary>
    /// <param name="loadedSettings">Settings to load</param>
    protected abstract void ApplySettings(ComponentSettings loadedSettings);

    public void LoadSavableSettings(ComponentSettings loadedSettings) {
        ApplySettings(loadedSettings);
        OnConfigChanged.Invoke();
    }

    /// <summary>
    /// If Awake is overriden, remember to call the base.Awake().
    /// </summary>
    protected virtual void Awake() {
        ApplySettings(GetDefaultSettings());
        SaveLoad.RegisterConfigurableComponent(this);
    }
}
