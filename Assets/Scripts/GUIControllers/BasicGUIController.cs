using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// This class contains helper methods to help with manipulating the GUI.
/// </summary>
public abstract class BasicGUIController : MonoBehaviour {
    protected void SetInputFieldValid(InputField field) {
        field.image.color = Color.green;
    }

    protected void SetInputFieldInvalid(InputField field) {
        field.image.color = Color.red;
    }

    protected void SetInputFieldNeutral(InputField field) {
        field.image.color = Color.white;
    }
}

/// <summary>
/// Abstract class to automatically provide a function to update the GUI.
/// </summary>
public abstract class DataGUIController : BasicGUIController, IDataChangedListener {
    protected virtual void Start() {
        UpdateSettingsGUI();
    }

    public abstract void UpdateSettingsGUI();
}

/// <summary>
/// Interface to notify class of any data changes.
/// </summary>
public interface IDataChangedListener {
    void UpdateSettingsGUI();
}
