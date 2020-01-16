using System.Collections;
using UnityEngine;

public class FadeCanvas : MonoBehaviour {

    public static FadeCanvas fadeCanvas;

    public float Alpha { get => fade.alpha; set => fade.alpha = value; }

    [SerializeField]
    private CanvasGroup fade = null;

    private WaitForSecondsRealtime timeIncrements = new WaitForSecondsRealtime(0.01f);
    public bool fadeOutDone { get; private set; }
    public bool fadeInDone { get; private set; }
    public bool isFadedOut { get; private set; }

    public bool isTransiting { get => !fadeInDone || !fadeOutDone; }

    void Awake() {
        if (fadeCanvas != null && fadeCanvas != this) {
            Destroy(this);
        }
        else {
            fadeCanvas = this;
        }
        fadeInDone = true;
        fadeOutDone = true;
        isFadedOut = true;
    }

    public Coroutine AutoFadeIn() {
        return StartCoroutine(FadeToScreen());
    }

    public Coroutine AutoFadeOut() {
        return StartCoroutine(FadetoBlack());
    }

    public IEnumerator FadetoBlack() {
        if (isFadedOut) {
            yield break;
        }
        fadeOutDone = false;
        isFadedOut = true;
        //wait for any fades to be done
        while (fadeInDone == false) {
            yield return timeIncrements;
        }
        while (fade.alpha < 1.0f) {
            fade.alpha += 0.02f;
            yield return timeIncrements;

        }
        fadeOutDone = true;
    }

    public IEnumerator FadeToScreen() {
        if (!isFadedOut) {
            yield break;
        }
        isFadedOut = false;
        fadeInDone = false;
        while (fadeOutDone == false) {
            yield return timeIncrements;
        }
        while (fade.alpha > 0f) {
            fade.alpha -= 0.02f;
            yield return timeIncrements;
        }
        fadeInDone = true;
    }
}
