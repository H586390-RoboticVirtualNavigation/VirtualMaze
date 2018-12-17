using UnityEngine;
using System.Collections;

public class SliderValue : MonoBehaviour {

	public UnityEngine.UI.Text text;
	public UnityEngine.UI.Slider slider;

	void Start () {
		text.text = slider.value.ToString ();
	}

	public void ChangeTextValue (float value) {
		text.text = value.ToString ();
	}
}
