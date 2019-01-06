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
    /// Identifies the subcomponentConfigs in the Settings
    /// 
    /// Recommended return statement: 
    /// <code> return typeof(ConfigurableComponent.SerializableSettings).Fullname </code>
    /// 
    /// Important: Developers must make sure this returns a unique string or bugs will occur
    /// (data not saved or wrong data overwritten.)
    /// 
    /// </summary>
    /// <returns>Identifier of this component's settings</returns>
    public abstract string GetSettingsID();
    
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
    /// Override this method to check if the current configuration values are valid.
    /// </summary>
    /// <returns>True if all configurations are valid</returns>
    public virtual bool IsValid() {
        return true;
    }



    /// <summary>
    /// If Awake is overriden, remember to call the base.Awake().
    /// </summary>
    protected virtual void Awake() {
        ApplySettings(GetDefaultSettings());
        SaveLoad.RegisterConfigurableComponent(this);
    }
}
