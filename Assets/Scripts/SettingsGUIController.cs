using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SettingsGUIController : MonoBehaviour
{
    private void Start() {
        UpdateSettingsGUI();
    }

    public abstract void UpdateSettingsGUI();
}
