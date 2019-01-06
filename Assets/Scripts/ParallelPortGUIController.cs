using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ParallelPortGUIController : SettingsGUIController {
    //drag in Unity Editor
    public Button photodiodeButton;
    public InputField portNumField;
    public Button testConnectionButton;
    public ParallelPort parallelPortController;

    private Image stateImage;
    private bool _state;
    private bool state {
        get => _state;

        set {
            if (value) {
                stateImage.color = Color.black;
            }
            else {
                stateImage.color = Color.white;
            }
            _state = value;
        }
    }

    private void Awake() {
        stateImage = photodiodeButton.image;

        photodiodeButton.onClick.AddListener(StartPhotoDiode);
        testConnectionButton.onClick.AddListener(OnParallelTestClick);

        portNumField.onEndEdit.AddListener(onPortNumFieldEndEdit);
        portNumField.onValueChanged.AddListener(onPortNumFieldEdit);
    }

    private void onPortNumFieldEdit(string text) {
        portNumField.image.color = Color.white;
    }

    private void onPortNumFieldEndEdit(string text) {
        if (int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int address)) {
            portNumField.image.color = Color.white;
            parallelPortController.portHexAddress = address;
        }
        else {
            portNumField.image.color = Color.red;
        }
    }

    private void Start() {
        state = false;
    }

    public override void UpdateSettingsGUI() {
        portNumField.image.color = Color.white;
        portNumField.text = parallelPortController.portHexAddress.ToString();
    }

    public void OnParallelTestClick() {
        try {
            parallelPortController.SimpleTest();
            portNumField.image.color = Color.green;
        }
        catch (System.Exception e) {
            //experimentStatus = e.ToString();
            Debug.LogException(e);
            portNumField.image.color = Color.red;
        }
    }

    int numSyncs = 2000;
    float timeBetweenSyncs = 0.06f;
    float accTime = 0;
    bool startSync = false;

    public void StartPhotoDiode() {
        startSync = true;
        numSyncs = 2000;
    }

    void FixedUpdate() {
        if (startSync && numSyncs > 0) {
            accTime += Time.unscaledDeltaTime;
            if (accTime >= timeBetweenSyncs) {
                accTime = 0;
                state = !state;
                numSyncs--;
                OnParallelTestClick();
            }
        }
        else if (startSync && numSyncs <= 0) {
            startSync = false;
            numSyncs = 2000;
            accTime = 0;
        }
    }
}
