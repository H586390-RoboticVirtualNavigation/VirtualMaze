using UnityEngine;
using System.Collections.Generic;

public class SaveLoadGUIController : MonoBehaviour {
    //drag and drop in Unity GUI
    public MyDropdownScript settingListDropdown;
    public SaveLoad saveloadController;

    public void Start() {
        settingListDropdown.UpdateOptions(saveloadController.SettingsList, -1);
    }

    public void OnSaveButtonClicked() {
        //prevents UI Bug
        settingListDropdown.Hide();

        string settingName = settingListDropdown.text;

        if (string.IsNullOrEmpty(settingName)) {
            Debug.LogWarning("settingName is empty");
            return;
        }

        bool replaced = saveloadController.SaveSetting(settingName);

        List<string> settingList = saveloadController.SettingsList;
        settingListDropdown.UpdateOptions(settingList, -1);
    }

    public void OnDeleteButtonClicked() {
        //prevents UI bug
        settingListDropdown.Hide();

        string settingName = settingListDropdown.text;
        if (string.IsNullOrEmpty(settingName)) {
            Debug.LogWarning("settingName is empty");
            return;
        }

        bool success = saveloadController.DeleteSetting(settingName);

        if (success) {
            settingListDropdown.UpdateOptions(saveloadController.SettingsList, -1);
            settingListDropdown.text = "";
            settingListDropdown.RefreshShownValue();
        }
    }

    public void OnOptionSelectedAction(int index) {
        if (index == -1) {
            return;
        }
        string settingName = settingListDropdown.options[index].text;
        saveloadController.ApplySettings(settingName);
    }
}
