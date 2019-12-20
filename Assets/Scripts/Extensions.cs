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

    /// <summary>
    /// Unity uses the bottom left as origin (0,0) but eyelink uses top right as origin.
    /// This property returns converted gaze data.
    /// 
    /// Height of Unity screen is fixed as 1080.
    /// </summary>
    public static Vector2 ConvertToUnityOriginCoordinate(this Vector2 gazeVector) {
        return new Vector2(gazeVector.x, 1080 - gazeVector.y);
    }

    public static bool ContainsNumbers(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return false;
        }
        foreach(char c in str)
        {
            if (char.IsDigit(c))
            {
                return true;
            }
        }
        return false;
    }
}
