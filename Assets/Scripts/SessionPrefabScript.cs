using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.UI;

public class SessionPrefabScript : MonoBehaviour {
    /// <summary>
    /// when the level/scene is changed, the position of the item and 
    /// the new level name is returned
    /// </summary>
    [Serializable]
    public class OnValueChanged : UnityEvent<int, string> { }

    /// <summary>
    /// when the level/scene is changed, the position of the item and 
    /// the new number of trials is returned
    /// </summary>
    [Serializable]
    public class OnNumTrialsChanged : UnityEvent<int, int> { }

    /// <summary>
    /// When a session is deleted, the position of the deleted item will be 
    /// provided.
    /// </summary>
    [Serializable]
    public class OnItemRemove : UnityEvent<int> { }

    private int index = 0;

    //drag from editor
    public string[] levels; // could have placed Sessions.AllLevels here but it will increase the coupling of the 2 classes.
    public Button levelButton;
    public InputField numTrialsField;

    public OnValueChanged onValueChanged = new OnValueChanged();
    public OnItemRemove onItemRemove = new OnItemRemove();
    public OnNumTrialsChanged onNumTrialsChanged = new OnNumTrialsChanged();

    private Text buttonLabel;

    public string numTrials {
        get {
            return numTrialsField.text;
        }
        set {
            numTrialsField.text = value;
        }
    }

    //boolean representing the validity of this session
    public bool isValidSession { get; private set; } = false;

    private void Awake() {
        buttonLabel = levelButton.GetComponentInChildren<Text>();
        numTrialsField.onEndEdit.AddListener(OnNumTrialsFieldEndEdit);
    }

    private void OnNumTrialsFieldEndEdit(string text) {
        if (CheckValidTrialNumber(text, out int num)) {
            onNumTrialsChanged.Invoke(transform.GetSiblingIndex(), num);
        }
    }

    public void NextLevel() {
        if (levels == null || levels.Length == 0) return;

        //circular array
        index = (index + 1) % levels.Length;

        string level = levels[index];
        buttonLabel.text = level;

        //call onValueChanged after value is changed.
        onValueChanged.Invoke(transform.GetSiblingIndex(), level);
    }

    public void PrevLevel() {
        if (levels == null || levels.Length == 0) return;

        //circular array
        int temp = index - 1;
        temp = temp < 0 ? levels.Length - 1 : temp;
        index = temp;

        string level = levels[index];
        buttonLabel.text = level;
        
        //call onValueChanged after value is changed.
        onValueChanged.Invoke(transform.GetSiblingIndex(), level);
    }

    public void Remove() {
        //get the sibling index before the object is deleted.
        onItemRemove.Invoke(transform.GetSiblingIndex());
        Destroy(this.gameObject);
    }

    public bool CheckValidTrialNumber(string str, out int num) {
        if (int.TryParse(str, out int value)) {
            numTrialsField.GetComponent<Image>().color = Color.green;
            isValidSession = true;
            num = value;
            return true;
        }
        else {
            numTrialsField.GetComponent<Image>().color = Color.red;
            isValidSession = false;
            num = value;
            return false;
        }
    }

    public void SetSession(Session s) {
        buttonLabel.text = s.level;
        numTrialsField.text = s.numTrial.ToString();
    }

    // Use this for initialization
    void Start() {
        CheckValidTrialNumber(numTrials, out int num);
    }
}
