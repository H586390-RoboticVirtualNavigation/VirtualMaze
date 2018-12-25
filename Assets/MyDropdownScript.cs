using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyDropdownScript : Dropdown {
    //drag and drop in Unity GUI
    public InputField inputField;

    private readonly List<string> empty = new List<string> { "empty" };

    public string text {
        get => inputField.text;
        set => inputField.text = value;
    }

    private bool isShown = false;

    protected override void Awake() {
        base.Awake();
        onValueChanged.AddListener(new UnityAction<int>(UpdateInputField));
        
    }

    public void UpdateInputField(int selected) {
        if (-1 == selected) {
            inputField.text = "";
            return;
        }

        string optionTitle = options[selected].text;
        inputField.text = optionTitle;

        //prevents UI bug
        Hide();
    }

    public void UpdateOptions(List<string> options, int selectItem) {
        ClearOptions();
        if (options.Count > 0) {
            AddOptions(options);
        }
        else {
            AddOptions(empty);
        }

        if(selectItem > 0) {
            value = selectItem;
        }
        
        RefreshShownValue();
    }
    
    private void ToggleDropdown() {
        if (!isShown) {
            Show();
        }
        else {
            Hide();
        }
    }

    public override void OnPointerClick(PointerEventData eventData) {
        ToggleDropdown();
    }

    //Wrapper Methods to implement custom UI behaviours
    public new void Show() {
        base.Show();
        isShown = true;
    }

    public new void Hide() {
        base.Hide();
        isShown = false;
    }

}
