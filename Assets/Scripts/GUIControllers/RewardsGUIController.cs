using UnityEngine.UI;

public class RewardsGUIController : DataGUIController {
    private const string Text_OnState = "Valve On";
    private const string Text_OffState = "Valve Off";

    //Drag in from Unity GUI
    public InputField portNumField;
    public InputField rewardDurationField;
    public Toggle rewardDurationValid;
    public Text valveStateText;
    public InputField requiredViewAngleInput;
    public InputField requiredDistanceInput;
    public InputField proximityDistanceInput;
    public InputField directionErrorDistanceInput;
    public RewardsController rewardsController;

    private void Awake() {
        portNumField.onEndEdit.AddListener(OnPortNumFieldEndEdit);
        rewardDurationField.onEndEdit.AddListener(OnDurationFieldEndEdit);

        requiredViewAngleInput.onEndEdit.AddListener(OnRequiredViewAngleChanged);
        requiredDistanceInput.onEndEdit.AddListener(OnRequiredDistanceChanged);
        proximityDistanceInput.onEndEdit.AddListener(OnProximityDistanceChanged);
        directionErrorDistanceInput.onEndEdit.AddListener(OnDirectionErrorDistanceChanged);
    }

    private void OnRequiredViewAngleChanged(string value) {
        if (float.TryParse(value, out float angle)) {
            RewardArea.RequiredViewAngle = angle;
        }
        else {
            Console.WriteError("Invaild view angle");
        }

        requiredViewAngleInput.text = RewardArea.RequiredViewAngle.ToString();
    }

    public void ToggleValveState() {
        if (!rewardsController.IsPortOpen) {
            if (rewardsController.RewardValveOn()) {
                valveStateText.text = Text_OffState;
                SetInputFieldValid(portNumField, true);
            }
            else {
                Console.Write("cant open reward serial");
                SetInputFieldValid(portNumField, false);
            }
        }
        else {
            rewardsController.RewardValveOff();
            valveStateText.text = Text_OnState;
        }
    }

    public void OnPortNumFieldEndEdit(string input) {
        //check input and place update RewardsController
        rewardsController.portNum = input;
    }

    public void OnDurationFieldEndEdit(string input) {
        if (int.TryParse(input, out int duration)) {
            rewardsController.rewardDurationMilliSecs = duration;
        }
    }

    public void OnRequiredDistanceChanged(string value) {
        if (float.TryParse(value, out float distance)) {
            RewardArea.RequiredDistance = distance;
        }
        else {
            Console.WriteError("Invaild minimum distance");
        }
        requiredDistanceInput.text = RewardArea.RequiredDistance.ToString();
    }

    public void OnProximityDistanceChanged(string value) {
        if (float.TryParse(value, out float distance)) {
            RewardArea.ProximityDistance = distance;
        }
        else {
            Console.WriteError("Invaild proximity distance");
        }
        proximityDistanceInput.text = RewardArea.ProximityDistance.ToString();
    }

    public void OnDirectionErrorDistanceChanged(string value)
    {
        if (float.TryParse(value, out float distance))
        {
            DirectionError.distanceRange = distance;
        }
        else
        {
            Console.WriteError("Invaild direction error distance");
        }
        proximityDistanceInput.text = DirectionError.distanceRange.ToString();
    }

    public override void UpdateSettingsGUI() {
        portNumField.text = rewardsController.portNum;
        SetInputFieldNeutral(portNumField);

        string millis = rewardsController.rewardDurationMilliSecs.ToString();
        rewardDurationField.text = millis;
        rewardDurationValid.isOn = IsDurationInputValid(millis);

        requiredViewAngleInput.text = RewardArea.RequiredViewAngle.ToString();
        requiredDistanceInput.text = RewardArea.RequiredDistance.ToString();
        proximityDistanceInput.text = RewardArea.ProximityDistance.ToString();
        directionErrorDistanceInput.text = DirectionError.distanceRange.ToString();
    }

    private bool IsDurationInputValid(string duration) {
        if (string.IsNullOrEmpty(duration)) {
            return false;
        }
        if (int.TryParse(duration, out int milliseconds)) {
            return IsDurationInputValid(milliseconds);
        }
        return false;
    }

    private bool IsDurationInputValid(int duration) {
        if (duration >= 0) {
            return true;
        }
        return false;
    }
}
