using UnityEngine;
using System.Collections;

public class StartLevelController : MonoBehaviour {

	void Awake(){
		foreach (GameObject item in GameController.instance.listToDisable) {

			CanvasGroup cg = item.GetComponent<CanvasGroup>();
			if(cg != null){
				cg.alpha = 1f;
				cg.interactable = true;
				cg.blocksRaycasts = true;
			}else{
				item.SetActive(true);
			}
		}
	}

}
