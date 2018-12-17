using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LinearLevelController : MonoBehaviour {

	private GameController gameController;
	private GameObject robot;
	private RobotMovement robotMovement;
	private Fading fade;
	private int numTrials;
	private int trialCounter;
	private bool trigger;
	private int triggerValue;
	private float elapsedTime;
	private float completionTime;
	private bool inTrial;
	public Reward[] rewards;
	public Transform startWaypoint;
	private GameObject posters;
	private Toggle showPosters;

	void OnEnable(){
		EventManager.StartListening ("Entered Reward Area", EnteredReward);
	}
	
	void OnDisable(){
		EventManager.StopListening ("Entered Reward Area", EnteredReward);
	}

	void Awake(){
		showPosters = GameObject.Find ("ShowPosters").GetComponent<Toggle>();
		posters = GameObject.Find ("Poster");
		gameController = GameObject.Find ("GameController").GetComponent<GameController>();
		fade = GameObject.Find ("FadeCanvas").GetComponent<Fading>();
		robot = GameObject.Find ("Robot");
		robotMovement = robot.GetComponent<RobotMovement> ();
	}

	void Start(){

		inTrial = false;

		//disable robot movement
		robotMovement.enabled = false;

		//get completiontime
		completionTime = (float)GuiController.completionWindowTime / 1000.0f;

		//set numTrials
		numTrials = gameController.numTrials;
		trialCounter = 1;	//start with first trial
		trigger = false;

		//check if need to show posters

		if (posters != null) {
			foreach (Transform child in posters.transform) {
				child.gameObject.GetComponent<MeshRenderer>().enabled = showPosters.isOn;
			}
		}


		StartCoroutine ("FadeInAndStart");
	}

	private Vector3 gazepos;

	void Update(){

		//increment elapsedTime for Timeout
		elapsedTime += Time.deltaTime;

		//save position data
		if (gameController.fs != null) {

			if(trigger){

				// send parallel port
				Debug.Log(triggerValue);
				if(GameController.instance.parallelPortAddr != -1) {
					ParallelPort.TryOut32 (GameController.instance.parallelPortAddr, triggerValue);	
					ParallelPort.TryOut32 (GameController.instance.parallelPortAddr, 0);	
				}
				gameController.fs.WriteLine("{0} {1:F8} {2:F2} {3:F2} {4:F2} {5:F6} {6:F6}", 
				                            triggerValue, 
				                            Time.deltaTime, 
				                            robot.transform.position.x, 
				                            robot.transform.position.z,
				                            robot.transform.eulerAngles.y);
				trigger = false;
			}else{
				gameController.fs.WriteLine("     {0:F8} {1:F2} {2:F2} {3:F2} {4:F6} {5:F6} ", 
				                            Time.deltaTime, 
				                            robot.transform.position.x, 
				                            robot.transform.position.z,
				                            robot.transform.eulerAngles.y);
			}
		}
	}

	void EnteredReward(){
		Reward entered = Reward.rewardTriggered;

		if (entered.enableReward) {

			StopCoroutine("Timeout");

			//reward
			EventManager.TriggerEvent("Reward");
			trigger = true;
			triggerValue = 2;

			//disable entered reward
			entered.enableReward = false;

			//enable other rewards
			foreach(Reward reward in rewards){
				if(reward != entered){
					reward.enableReward = true;
				}
			}

			//session ends
			if(trialCounter >= numTrials){
				
				//disable robot movement
				robotMovement.enabled = false;
				
				StartCoroutine("FadeOutBeforeLevelEnd");
			}else{
				//increment trial
				trialCounter++;
				
				//disable robot movement
				robotMovement.enabled = false;
				
				//new trial
				StartCoroutine("InterTrial");
			}
		}
	}

	void SetPositionToStart(){
		//set robot's position and rotation to start
		Vector3 startpos = robot.transform.position;
		startpos.x = startWaypoint.position.x;
		startpos.z = startWaypoint.position.z;
		robot.transform.position = startpos;
		
		Quaternion startrot = robot.transform.rotation;
		startrot.y = startWaypoint.rotation.y;
		robot.transform.rotation = startrot;
	}
	
	IEnumerator FadeInAndStart(){

		//go to start
		SetPositionToStart ();
		
		//fade in
		fade.FadeIn ();
		while (fade.fadeInDone == false) {
			yield return new WaitForSeconds(0.05f);
		}

		//enable robot movement
		robotMovement.enabled = true;

		//trigger - start trial
		trigger = true;
		triggerValue = 1;

		//play start clip
		PlayerAudio.instance.PlayStartClip ();

		//update experiment status
		GuiController.experimentStatus = string.Format ("session {0} trial {1}", gameController.sessionCounter, trialCounter);

		//reset elapsed time
		elapsedTime = 0;
		StartCoroutine ("Timeout");
		
		inTrial = true;
	}

	
	IEnumerator FadeOutBeforeLevelEnd(){

		inTrial = false;

		//fade out when end
		fade.FadeOut ();
		while (fade.fadeOutDone == false) {
			yield return new WaitForSeconds(0.05f);
		}
		EventManager.TriggerEvent("Level Ended");
	}

	IEnumerator InterTrial(){

		inTrial = false;

		//delay for inter trial window
		float countDownTime = (float)GuiController.interTrialTime / 1000.0f;
		while (countDownTime > 0) {
			GuiController.experimentStatus = string.Format("Inter-trial time {0:F2}", countDownTime);
			yield return new WaitForSeconds (0.1f);
			countDownTime -= 0.1f;
		}

		//disable robot movement
		robotMovement.enabled = true;

		//trigger - new trial
		trigger = true;
		triggerValue = 1;

		//reset elapsed time
		elapsedTime = 0;
		StartCoroutine ("Timeout");

		//play audio
		PlayerAudio.instance.PlayStartClip ();

		//update experiment status
		GuiController.experimentStatus = string.Format ("session {0} trial {1}", gameController.sessionCounter, trialCounter);

		inTrial = true;
	}

	IEnumerator Timeout(){

		while (true) {

			//time out
			if (elapsedTime > completionTime) {

				inTrial = false;

				//trigger - timeout
				trigger = true;
				triggerValue = 4;

				//play audio
				PlayerAudio.instance.PlayErrorClip();

				//disable robot movement
				robotMovement.enabled = false;

				//delay for timeout
				float countDownTime = (float)GuiController.timoutTime / 1000.0f;
				while (countDownTime > 0) {
					GuiController.experimentStatus = string.Format("timeout {0:F2}", countDownTime);
					yield return new WaitForSeconds (0.1f);
					countDownTime -= 0.1f;
				}

				//play audio
				PlayerAudio.instance.PlayStartClip ();

				//update experiment status, considered the same trial
				GuiController.experimentStatus = string.Format ("session {0} trial {1}", 
				                                                gameController.sessionCounter, 
				                                                trialCounter);

				//trigger - start trial
				trigger = true;
				triggerValue = 1;

				//disable robot movement
				robotMovement.enabled = true;

				//reset elapsed time
				elapsedTime = 0;

				inTrial = true;

			} else if (inTrial) {
				//update experiment status, considered the same trial
				GuiController.experimentStatus = string.Format ("session {0} trial {1}\ntimeout in {2:F2}", 
				                                                gameController.sessionCounter, 
				                                                trialCounter, 
				                                                completionTime - elapsedTime);
			}

			yield return new WaitForSeconds (0.1f);
		}
	}

}



















