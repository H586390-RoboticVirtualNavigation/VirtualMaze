using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CueController : MonoBehaviour {
    private Sprite hint;

    //Drag in Unity Editor
    public Image cueImage;
    public Image hintImage;

    private void Awake() {
        ShowImage(cueImage, false);
        ShowImage(hintImage, false);

        Vector3[] corners = new Vector3[4];

        cueImage.rectTransform.GetWorldCorners(corners);

        print($"{cueImage.name}|{corners[0]}{corners[1]}{corners[2]}{corners[3]}");

        hintImage.rectTransform.GetWorldCorners(corners);

        print($"{hintImage.name}|{corners[0]}{corners[1]}{corners[2]}{corners[3]}");
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
