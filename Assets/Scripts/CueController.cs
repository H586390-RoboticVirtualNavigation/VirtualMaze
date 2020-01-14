using UnityEngine;
using UnityEngine.UI;

public class CueController : MonoBehaviour {
    private Sprite hint;

    //Drag in Unity Editor
    [SerializeField]
    private Image cueImage = null;
    private BoxCollider cueBoxCollider = null;

    [SerializeField]
    private Image hintImage = null;
    private BoxCollider hintBoxCollider = null;

    [SerializeField]
    private CharacterController controller = null;

    [SerializeField]
    private RobotMovement robot = null;

    //offset with respect to the robot position.
    private Vector3 forwardOffset = new Vector3(0, 0, 0.5f);
    private Vector3 heightOffset = new Vector3(0, 1.35f, 0);

    //follows only the X rotation of the player camera
    private Quaternion rotationOffset = Quaternion.Euler(12f, 0, 0);

    public enum Mode {
        Recording,
        Experiment
    }

    private Mode mode = Mode.Experiment;

    public void SetMode(Mode mode) {
        switch (mode) {
            case Mode.Recording:
                cueBoxCollider.enabled = true;
                hintBoxCollider.enabled = true;
                controller.enabled = false;
                break;
            case Mode.Experiment:
                cueBoxCollider.enabled = false;
                hintBoxCollider.enabled = false;
                controller.enabled = true;
                break;
            default:
                break;
        }
        this.mode = mode;
    }

    private void Awake() {
        ShowImage(cueImage, false);
        ShowImage(hintImage, false);

        Vector3[] corners = new Vector3[4];

        cueImage.rectTransform.GetWorldCorners(corners);

        print($"{cueImage.name}|{corners[0]}{corners[1]}{corners[2]}{corners[3]}");

        hintImage.rectTransform.GetWorldCorners(corners);

        print($"{hintImage.name}|{corners[0]}{corners[1]}{corners[2]}{corners[3]}");

        cueBoxCollider = cueImage.GetComponent<BoxCollider>();
        hintBoxCollider = hintImage.GetComponent<BoxCollider>();

        robot.OnRobotMoved += UpdatePosition;

        SetMode(Mode.Experiment);
    }

    public void UpdatePosition(Transform robot) {
        Vector3 a = rotationOffset * forwardOffset; // apply robot's current rotation to position
        a = robot.rotation * a;
        transform.position = robot.position + a + heightOffset; //set canvas location

        transform.rotation = robot.rotation * rotationOffset; //apply camera X rotation to canvas rotation
    }

    public Sprite GetHint() {
        return hint;
    }

    public void SetTargetImage(Sprite value) {
        hint = value;
        cueImage.sprite = value;
        hintImage.sprite = value;
    }

    private void ShowImage(Image image, bool show) {
        //show or hide if not image is not null
        image?.gameObject.SetActive(show);
    }

    public void ShowCue() {
        ShowImage(cueImage, true);
    }

    public void HideCue() {
        ShowImage(cueImage, false);
    }

    public void ShowHint() {
        ShowImage(hintImage, true);
    }

    public void HideHint() {
        ShowImage(hintImage, false);
    }

    public void HideAll() {
        HideHint();
        HideCue();
    }

    public static void ProcessTrigger(SessionTrigger trigger, CueController cueController, ITriggerActions actions = null) {
        switch (trigger) {
            case SessionTrigger.CueOffsetTrigger:
                cueController.HideCue();
                cueController.ShowHint();
                SessionStatusDisplay.DisplaySessionStatus("Trial Running");
                actions?.CueOffsetTriggerAction();
                break;

            case SessionTrigger.TrialStartedTrigger:
                cueController.HideHint();
                cueController.ShowCue();
                SessionStatusDisplay.DisplaySessionStatus("Showing Cue");
                actions?.TrialStartedTriggerAction();
                break;

            case SessionTrigger.TimeoutTrigger:
                SessionStatusDisplay.DisplaySessionStatus("Time out");
                cueController.HideAll();
                actions?.TimeoutTriggerAction();
                break;

            case SessionTrigger.TrialEndedTrigger:
                cueController.HideAll();
                SessionStatusDisplay.DisplaySessionStatus("Trial Ended");
                actions?.TrialEndedTriggerAction();
                break;

            case SessionTrigger.ExperimentVersionTrigger:
                SessionStatusDisplay.DisplaySessionStatus("Next Session");
                actions?.ExperimentVersionTriggerAction();
                break;

            case SessionTrigger.NoTrigger:
                actions?.NoTriggerAction();
                break;

            default:
                Debug.LogError($"Unidentified Session Trigger: {trigger}");
                actions?.DefaultAction();
                break;
        }
    }

    /// <summary>
    /// Implement this interface to add custom actions when processing triggers
    /// </summary>
    public interface ITriggerActions {
        void TrialStartedTriggerAction();
        void CueOffsetTriggerAction();
        void TrialEndedTriggerAction();
        void TimeoutTriggerAction();
        void ExperimentVersionTriggerAction();
        void NoTriggerAction();
        void DefaultAction();
    }
}
