using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Fading : MonoBehaviour {

	private CanvasGroup fade;
	public bool fadeOutDone;
	public bool fadeInDone;

	void Awake() {
		fade = this.gameObject.GetComponent<CanvasGroup> ();
		fadeInDone = true;
		fadeOutDone = true;
	}

	public Coroutine FadeIn(){
		return StartCoroutine (_FadeIn());
	}

	public Coroutine FadeOut(){
		return StartCoroutine (_FadeOut());
	}

	IEnumerator _FadeOut(){
		fadeOutDone = false;
		//wait for any fades to be done
		while (fadeInDone == false) {
			yield return new WaitForSeconds(.01f);
		}
		while(fade.alpha < 1.0f){
			fade.alpha += 0.02f;
			yield return new WaitForSeconds(.01f);
	
		}
		fadeOutDone = true;
	}
	
	IEnumerator _FadeIn(){
		fadeInDone = false;
		while (fadeOutDone == false) {
			yield return new WaitForSeconds(.01f);
		}
		while(fade.alpha > 0f){
			fade.alpha -= 0.02f;
			yield return new WaitForSeconds(.01f);
		}
		fadeInDone = true;
	}
}
