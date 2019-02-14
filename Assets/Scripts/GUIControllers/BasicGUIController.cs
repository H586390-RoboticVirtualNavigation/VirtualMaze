using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public abstract class BasicGUIController : MonoBehaviour {

    private void Start() {
        UpdateSettingsGUI();
    }

    protected void SetInputFieldValid(InputField field) {
        field.image.color = Color.green;
    }

    protected void SetInputFieldInvalid(InputField field) {
        field.image.color = Color.red;
    }

    protected void SetInputFieldNeutral(InputField field) {
        field.image.color = Color.white;
    }

    public abstract void UpdateSettingsGUI();
}
