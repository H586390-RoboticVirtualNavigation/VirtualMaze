using UnityEngine;
using System.Collections;

public class TrainLeftLevelController : MonoBehaviour {

	private GameController gameController;
	private GameObject robot;
	private RobotMovement robotMovement;
	private FadeCanvas fade;
	private int numTrials;
	private int trialCounter;
	private bool trigger;
	private int triggerValue;
	private float elapsedTime;
	private float completionTime;
	private bool inTrial;
	public Reward[] rewards;
	public Transform startWaypoint;

    private ParallelPort parallelPortcontroller;

    void OnEnable(){
		EventManager.StartListening ("Entered Reward Area", EnteredReward);
	}
	
	void OnDisable(){
		EventManager.StopListening ("Entered Reward Area", EnteredReward);
	}
	
	void Awake(){
		
		gameController = GameObject.Find ("GameController").GetComponent<GameController>();
		fade = GameObject.Find ("FadeCanvas").GetComponent<FadeCanvas>();
		robot = GameObject.Find ("Robot");
		robotMovement = robot.GetComponent<RobotMovement> ();
        parallelPortcontroller = GameObject.Find("ParallelPortController").GetComponent<ParallelPort>();

        if (NetworkConnection.instance.replayMode == true) {
			this.gameObject.SetActive(false);
			fade.FadeIn ();
		}
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
		
		StartCoroutine ("FadeInAndStart");
	}
	
	void Update(){
		
		//increment elapsedTime for Timeout
		elapsedTime += Time.deltaTime;
		
		//determine if trail complete
		float angle = Vector3.Angle (robot.transform.forward, startWaypoint.forward);
		angle = Mathf.Abs (angle);
	
		if ((angle > 70) && inTrial) {
			inTrial = false;
			CompleteTrial();
		}
		
		//save position data
		if (gameController.fs != null) {

			if(trigger){
                // send parallel port
                parallelPortcontroller.WriteTrigger(triggerValue);
                parallelPortcontroller.WriteTrigger(0);

                //original code
                //if(GameController.instance.parallelPortAddr != -1) {
                //	ParallelPort.TryOut32 (GameController.instance.parallelPortAddr, triggerValue);	
                //	ParallelPort.TryOut32 (GameController.instance.parallelPortAddr, 0);	
                //}
                gameController.fs.WriteLine("{0} {1:F8} {2:F2} {3:F2} {4:F2} {5:F2} {6:F2}", 
				                            triggerValue, 
				                            Time.deltaTime, 
				                            robot.transform.position.x, 
				                            robot.transform.position.z,
				                            robot.transform.eulerAngles.y);
				trigger = false;
			}else{
				gameController.fs.WriteLine("     {0:F8} {1:F2} {2:F2} {3:F2} {4:F2} {5:F2} ", 
				                            Time.deltaTime, 
				                            robot.transform.position.x, 
				                            robot.transform.position.z,
				                            robot.transform.eulerAngles.y);
			}
		}
	}
	
	private void CompleteTrial(){
		
		StopCoroutine("Timeout");
		
		//reward
		EventManager.TriggerEvent("Reward");
		trigger = true;
		triggerValue = 2;
		
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
	
	void EnteredReward(){
		
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
		fade.AutoFadeOut ();
		while (fade.fadeOutDone == false) {
			yield return new WaitForSeconds(0.05f);
		}
		EventManager.TriggerEvent("Level Ended");
	}
	
	IEnumerator InterTrial(){
		
		inTrial = false;
		
		
		fade.AutoFadeOut ();
		while (fade.fadeOutDone == false) {
			yield return new WaitForSeconds(0.05f);
		}
		
		//teleport back to start
		SetPositionToStart();
		
		//delay for inter trial window
		float countDownTime = (float)GuiController.interTrialTime / 1000.0f;
		while (countDownTime > 0) {
			GuiController.experimentStatus = string.Format("Inter-trial time {0:F2}", countDownTime);
			yield return new WaitForSeconds (0.1f);
			countDownTime -= 0.1f;
		}
		
		
		fade.FadeIn ();
		while (fade.fadeInDone == false) {
			yield return new WaitForSeconds(0.05f);
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
