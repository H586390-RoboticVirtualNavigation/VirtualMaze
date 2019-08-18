using System.Collections;
using UnityEngine;

public class FadeCanvas : MonoBehaviour {

    public static FadeCanvas fadeCanvas;

    public float Alpha { get => fade.alpha; set => fade.alpha = value; }
    private CanvasGroup fade;
    private WaitForSecondsRealtime timeIncrements = new WaitForSecondsRealtime(0.01f);
    public bool fadeOutDone { get; private set; }
    public bool fadeInDone { get; private set; }
    public bool isFadedOut { get; private set; }

    public bool isTransiting { get => !fadeInDone || !fadeOutDone; }

    void Awake() {
        fadeCanvas = this;
        fade = GetComponent<CanvasGroup>();
        fadeInDone = true;
        fadeOutDone = true;
        isFadedOut = true;
    }

    public Coroutine AutoFadeIn() {
        return StartCoroutine(FadeIn());
    }

    public Coroutine AutoFadeOut() {
        return StartCoroutine(FadeOut());
    }

    public IEnumerator FadeOut() {
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

    public IEnumerator FadeIn() {
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
