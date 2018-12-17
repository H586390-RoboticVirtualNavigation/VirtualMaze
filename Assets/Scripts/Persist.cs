using UnityEngine;
using System.Collections;

public class Persist : MonoBehaviour {

	private static Persist _instance;

	public static Persist instance {
		get {
			if(!_instance){
				_instance = FindObjectOfType<Persist>();
				if(!_instance){
					Debug.LogError("Needs one Persist GameObject in scene.");
				} else {
					DontDestroyOnLoad(_instance.gameObject);
				}
			}
			return _instance;
		}
	}

	void Awake () {
		if (!_instance) {
			_instance = this;
			DontDestroyOnLoad(this.gameObject);
		} else if(this.gameObject != _instance.gameObject) {
			Destroy(this.gameObject);
		}
	}
}
