using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class Reward : MonoBehaviour {

	public bool enableReward;
	public Light blinkLight;
    public GameObject poster;
	public float intensity;
	public static Reward rewardTriggered;
    public bool mainReward;
    public bool onScreenFx;
    private bool blink;
	private bool toggle;
	private Camera cam;
	//private Slider rewardViewCriteriaSlider;
	private bool inTriggerArea;
	private bool teleportedToStart;     

	void OnEnable(){
		EventManager.StartListening ("Teleported To Start", SetTeleFlag);
	}
	
	void OnDisable(){
		EventManager.StopListening ("Teleported To Start", SetTeleFlag);
	}

	void SetTeleFlag(){
		Debug.Log ("resetting");
		// robot might be teleported into the trigger area
		teleportedToStart = true;
    }

    void Start(){
		teleportedToStart = false;
		inTriggerArea = false;
        if (enableReward)
        {
            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
        //StartCoroutine("StartBlinking");
    }

	void Awake() {
		cam = GameObject.Find ("Left Camera").GetComponent<Camera> ();
		//rewardViewCriteriaSlider = GameObject.Find ("RewardViewCriteriaSlider").GetComponent<Slider> ();
	}

	IEnumerator StartBlinking(){
		while (true) {
			if (enableReward) {
				ToggleBlink();
			} else {
				blinkLight.intensity = 0;
			}
			yield return new WaitForSeconds(1.0f);
		}
	}

	private void ToggleBlink(){
		if (toggle) {
			blinkLight.intensity = 0;
		} else {
			blinkLight.intensity = intensity;
		}
		toggle = !toggle;
	}

	void Update () {
		if (inTriggerArea) {
			// possibility to trigger if light is in view
			Vector3 screenPoint = cam.WorldToViewportPoint(poster.transform.position); //poster position
            //Debug.Log("x:" + screenPoint.x + ";z:" + screenPoint.z);
            //float restrict = (1.0f - rewardViewCriteriaSlider.value) / 2.0f;
            //bool onScreen = screenPoint.z > 0 && screenPoint.x > restrict && screenPoint.x < (1.0f - restrict);
            float restrict = 0.2f;
            
            bool onScreen = screenPoint.z > 0 && screenPoint.x > restrict && screenPoint.x < (1.0f - restrict); //poster centered
            if (onScreenFx && enableReward)
            {
                Debug.Log("here1");
                triggerReward();
            }
            else if (onScreen && enableReward)
                {
                //Debug.Log("x:" + screenPoint.x + ";z:" + screenPoint.z);
                triggerReward();
                }
		}
        if (enableReward)
        {
            if (!blink)
            {
                StartCoroutine("StartBlinking");
                blink = true;
            }
        }
        else
        {
            blink = false;
        }
    }

    void triggerReward()
    {
        rewardTriggered = this;
        EventManager.TriggerEvent("Entered Reward Area");
        inTriggerArea = false;
    }

	void OnTriggerStay(Collider other){
		if (teleportedToStart) {
			teleportedToStart = false;
			inTriggerArea = true;
		}
	}

	void OnTriggerEnter(Collider other){
		Debug.Log ("entered");
        inTriggerArea = true;
    }

	void OnTriggerExit(Collider other){
		Debug.Log ("exit");
        inTriggerArea = false;
        //GetComponent<Collider>().enabled = false; // disable collider
    }



}
