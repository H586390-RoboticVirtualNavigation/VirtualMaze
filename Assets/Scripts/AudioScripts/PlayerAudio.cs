using UnityEngine;
using System.Collections;

public class PlayerAudio : MonoBehaviour {

	public AudioClip rewardClip;
	public AudioClip startClip;
	public AudioClip errorClip;
	public AudioSource audioSource;

	private static PlayerAudio _instance;
	public static PlayerAudio instance{
		get{
			_instance = GameObject.FindObjectOfType(typeof(PlayerAudio)) as PlayerAudio;
			if(_instance == null){
				Debug.Log ("need 1 instance of PlayerAudio");
			}
			return _instance;
		}
	}

	public void PlayRewardClip(){
		audioSource.clip = rewardClip;
		audioSource.Play ();
	}

	public void PlayStartClip(){
		audioSource.clip = startClip;
		audioSource.Play ();
	}

	public float PlayErrorClip(){
		audioSource.clip = errorClip;
		audioSource.Play();
        return errorClip.length;
	}
}
