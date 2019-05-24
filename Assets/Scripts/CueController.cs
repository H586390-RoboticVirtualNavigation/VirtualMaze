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
}
