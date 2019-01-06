using UnityEngine;
using System.Collections;

public class ProgressCalibPoint : MonoBehaviour {

	private MeshRenderer calibrender;

	public float progress = 0.0f;

	void Awake(){
		calibrender = GetComponent<MeshRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		calibrender.material.SetFloat("_Cutoff", 1.0f - (float)Mathf.Lerp(0.01f, 0.99f, progress)); 
	}
}
