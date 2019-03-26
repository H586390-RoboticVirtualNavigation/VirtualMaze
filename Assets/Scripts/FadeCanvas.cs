using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadeCanvas : MonoBehaviour {

	private CanvasGroup fade;
    private WaitForSecondsRealtime timeIncrements = new WaitForSecondsRealtime(0.01f);
	public bool fadeOutDone { get; private set; }
	public bool fadeInDone { get; private set; }
    public bool isFadedOut { get; private set; }

    void Awake() {
		fade = GetComponent<CanvasGroup> ();
		fadeInDone = true;
		fadeOutDone = true;
        isFadedOut = true;
	}

	public Coroutine FadeIn(){
		return StartCoroutine (_FadeIn());
	}

	public Coroutine FadeOut(){
		return StartCoroutine (_FadeOut());
	}

	IEnumerator _FadeOut(){
        if (isFadedOut) {
            yield break;
        }
		fadeOutDone = false;
        isFadedOut = true;
		//wait for any fades to be done
		while (fadeInDone == false) {
			yield return timeIncrements;
		}
		while(fade.alpha < 1.0f){
			fade.alpha += 0.02f;
			yield return timeIncrements;
	
		}
		fadeOutDone = true;
	}
	
	IEnumerator _FadeIn(){
        if (!isFadedOut) {
            yield break;
        }
        isFadedOut = false;
        fadeInDone = false;
		while (fadeOutDone == false) {
			yield return timeIncrements;
		}
		while(fade.alpha > 0f){
			fade.alpha -= 0.02f;
			yield return timeIncrements;
		}
		fadeInDone = true;
	}
}
