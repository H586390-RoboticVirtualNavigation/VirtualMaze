using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class to contain all extension methods for faster development
/// </summary>
public static class Extensions {
    /// <summary>
    /// Helper method to hide or show a canvas group.
    /// </summary>
    /// <param name="canvasGroup">canvas group to hide or show</param>
    /// <param name="shown">true to show, false to hide</param>
    public static void SetVisibility(this CanvasGroup canvasGroup, bool shown) {
        if (shown) {
            canvasGroup.alpha = 1;
        }
        else {
            canvasGroup.alpha = 0;
        }
        canvasGroup.blocksRaycasts = shown;
        canvasGroup.interactable = shown;
    }

    public static SessionTrigger NextTrigger(this SessionTrigger trigger, bool clearedTrial = true) {
        switch (trigger) {
            case SessionTrigger.NoTrigger:
                return SessionTrigger.NoTrigger;
            case SessionTrigger.TrialStartedTrigger:
                return SessionTrigger.CueOffsetTrigger;
            case SessionTrigger.CueOffsetTrigger:
                if (clearedTrial) {
                    return SessionTrigger.TrialEndedTrigger;
                }
                else {
                    return SessionTrigger.TimeoutTrigger;
                }
            case SessionTrigger.TrialEndedTrigger:
                return SessionTrigger.TrialStartedTrigger;
            case SessionTrigger.TimeoutTrigger:
                return SessionTrigger.TrialStartedTrigger;
            case SessionTrigger.ExperimentVersionTrigger:
                return SessionTrigger.ExperimentVersionTrigger;
            default:
                throw new NotImplementedException($"Trigger {trigger} not implemented.");
        }
    }

    public static Text GetText(this Button button) {
        return button.GetComponentInChildren<Text>();
    }
}
