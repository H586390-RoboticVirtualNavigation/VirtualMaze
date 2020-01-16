using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Unused class, kept as reference
/// 
/// YY: Most likely a 2 reward trial which needs the subject to reach reward1 then reward2
/// </summary>
[Obsolete]
public class TeeLevelController : MonoBehaviour {
	
	private GameController gameController;
	private GameObject robot;
	private RobotMovement robotMovement;
	private FadeCanvas fade;
	private bool gotReward001;
	private int numTrials = 0;
	private int trialCounter;
	
	public Transform startWaypoint;
	
	void OnEnable(){
		EventManager.StartListening ("Reward_001", WentIntoRewardArea001);
		EventManager.StartListening ("Reward_002", WentIntoRewardArea002); 
	}
	
	void OnDisable(){
		EventManager.StopListening ("Reward_001", WentIntoRewardArea001);
		EventManager.StopListening ("Reward_002", WentIntoRewardArea002); 
	}
	
	void Awake(){
		gameController = GameObject.Find ("GameController").GetComponent<GameController>();
		fade = GameObject.Find ("FadeCanvas").GetComponent<FadeCanvas>();
		robot = GameObject.Find ("Robot");
		robotMovement = robot.GetComponent<RobotMovement> ();
	}
	
	void Start(){
		
		//set numTrials
		//numTrials = gameController.numTrials;
		trialCounter = 1;	//start with first trial
		
		StartCoroutine (FadeInDelayStart ());
	}
	
	void Update(){
		
		//save position data
		//if (gameController.fs != null) {
		//	gameController.fs.WriteLine("{0:F3} {1:F2} {2:F2}", Time.fixedDeltaTime, robot.transform.position.x, robot.transform.position.z);
		//}
	}
	
	void WentIntoRewardArea001(){
		
		//went into reward001 again without going to reward002
		if (gotReward001) {
			
		} 
		
		//first time going into reward001
		else {
			gotReward001 = true;
			EventManager.TriggerEvent("Reward");
		}
	}
	
	void WentIntoRewardArea002(){
		
		//went into reward001 before coming to reward002
		if (gotReward001) {
			EventManager.TriggerEvent("Reward");	
			gotReward001 = false;
			
			//session ends
			if(trialCounter >= numTrials){
				StartCoroutine("FadeOutBeforeLevelEnd");
			}else{
				//new trial
				StartCoroutine("InterTrialFade");
				
				//increment trial
				trialCounter++;
			}
		} 
		
		//have not gone to reward001 yet
		else {
			
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
	
	IEnumerator FadeInDelayStart(){
		//disable robot movement
		robotMovement.enabled = false;
		
		//delay for intertrial time
		yield return new WaitForSeconds ((float)GuiController.interTrialTime / 1000.0f);
		
		//go to start
		SetPositionToStart ();
		
		//GuiController.experimentStatus = string.Format ("session {0} trial {1}", gameController.sessionCounter, trialCounter);
		
		//fade in
		fade.FadeToScreen ();
		while (fade.fadeInDone == false) {
			yield return new WaitForSeconds(0.05f);
		}
		
		//enable robot movement
		robotMovement.enabled = true;
	}
	
	
	IEnumerator FadeOutBeforeLevelEnd(){
		//fade out when end
		fade.AutoFadeOut ();
		while (fade.fadeOutDone == false) {
			yield return new WaitForSeconds(0.05f);
		}
		EventManager.TriggerEvent("Level Ended");
	}
	
	IEnumerator InterTrialFade(){
		//disable robot movement
		robotMovement.enabled = false;
		
		//fade out
		fade.AutoFadeOut ();
		while (fade.fadeOutDone == false) {
			yield return new WaitForSeconds(0.2f);
		}
		
		//reset position
		SetPositionToStart ();
		
		//delay for intertrial time
		yield return new WaitForSeconds ((float)GuiController.interTrialTime / 1000.0f);
		
		//GuiController.experimentStatus = string.Format ("session {0} trial {1}", gameController.sessionCounter, trialCounter);
		
		//fade in
		fade.FadeToScreen ();
		while (fade.fadeInDone == false) {
			yield return new WaitForSeconds(0.05f);
		}
		
		//enable robot movement
		robotMovement.enabled = true;
	}
}
