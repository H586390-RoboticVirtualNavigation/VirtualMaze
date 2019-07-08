using UnityEngine;

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
}
