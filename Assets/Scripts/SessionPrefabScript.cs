using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SessionPrefabScript : MonoBehaviour {

	private int _index = 0;
	public int index{
		get{
			return _index;
		}
		set{
			_index = value;
			levelButton.GetComponentInChildren<Text> ().text = levels [_index];
		}
	}

	//drag from editor
	public string[] levels;
	public Button levelButton;
	public InputField numTrialsField;

	public string numTrials{
		get {
			return numTrialsField.text;
		}
		set{
			numTrialsField.text = value;
			CheckValidTrialNumber (value);
		}
	}

	public string level{
		get {
			string lv = levelButton.GetComponentInChildren<Text>().text;

			// Random level
			while(lv.Equals("Random")){
				lv = levels[Random.Range(0,levels.GetLength(0))];
			}

			//Random LRF level
			if(lv.Equals("RandLRF")){
				string[] lrf = new string[3];
				lrf[0] = "TrainLeft";
				lrf[1] = "TrainRight";
				lrf[2] = "TrainForward";
				lv = lrf[(int)Random.Range (0,3)];
			}

			return lv;
		}
	}

	public bool valid { 
		get{
			int trialnum;
			if (int.TryParse (numTrialsField.text, out trialnum)) {
				return true;
			}
			return false;
		} 
	}
	
	public void NextLevel(){
		index = (index + 1) % levels.Length;
	}

	public void PrevLevel(){
		int temp = index - 1;
		temp = temp < 0 ? levels.Length - 1 : temp;
		index = temp;
	}

	public void Remove() {
		Destroy (this.gameObject);
	}

	public void CheckValidTrialNumber(string str) {
		int value;
		if (int.TryParse (str, out value)) {
			numTrialsField.GetComponent<Image>().color = Color.green;
		} else {
			numTrialsField.GetComponent<Image>().color = Color.red;
		}
	}

	// Use this for initialization
	void Start () {
		CheckValidTrialNumber (numTrials);
	}
}












