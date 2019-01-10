using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CueController : MonoBehaviour {
    public Sprite hintImage { get; set; }

    //Drag in Unity Editor
    public Image cue;
    public Image hint;

    private void Awake() {
        ShowImage(cue, false);
        ShowImage(hint, false);
    }

    private void ShowImage(Image image, bool show) {
        //show or hide if not image is not null
        image?.gameObject.SetActive(show);
    }

    public void ShowCue() {
        ShowImage(cue, true);
    }

    public void HideCue() {
        ShowImage(cue, false);
    }

    public void ShowHint() {
        ShowImage(hint, true);
    }

    public void HideHint() {
        ShowImage(hint, false);
    }
}
