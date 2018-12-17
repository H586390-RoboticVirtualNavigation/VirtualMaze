using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RobotMovement : MonoBehaviour {

	private Slider rotationSpeedSlider;
	private Slider translationSpeedSlider;
	private Toggle useJoystickToggle;
	private Rigidbody rigidBody;

	private static Toggle enableReverseToggle;
	private static Toggle enableForwardToggle;
	private static Toggle enableRightToggle;
	private static Toggle enableLeftToggle;

	void Awake () {
		rigidBody = GetComponent<Rigidbody> ();	
		rotationSpeedSlider = GameObject.Find ("RotationSpeedSlider").GetComponent <Slider> ();
		translationSpeedSlider = GameObject.Find ("TranslationSpeedSlider").GetComponent <Slider> ();
		enableReverseToggle = GameObject.Find ("Reverse").GetComponent <Toggle> ();
		enableForwardToggle = GameObject.Find ("Forward").GetComponent <Toggle> ();
		enableRightToggle = GameObject.Find ("Right").GetComponent <Toggle> ();
		enableLeftToggle = GameObject.Find ("Left").GetComponent <Toggle> ();
		useJoystickToggle = GameObject.Find ("UseJoystick").GetComponent <Toggle> ();
	}

	// Update is called once per frame
	void Update () {

		// using joy stick
		if (useJoystickToggle.isOn) {

			// rotate using horizontal axis
			if((SerialController.horizontal < 0) && !enableLeftToggle.isOn){
				
			} else if((SerialController.horizontal > 0) && !enableRightToggle.isOn){
				
			} else {
				Quaternion rotateBy = Quaternion.Euler (0, SerialController.horizontal * rotationSpeedSlider.value * Time.deltaTime, 0);
				rigidBody.MoveRotation (transform.rotation * rotateBy);
			}

			// translate
			if((SerialController.vertical < 0) && !enableReverseToggle.isOn){
				
			} else if((SerialController.vertical > 0) && !enableForwardToggle.isOn){

			} else {
				Vector3 moveBy = transform.forward * SerialController.vertical * translationSpeedSlider.value * Time.deltaTime;
				rigidBody.MovePosition (transform.position + moveBy);
			}
		} 

		// using keyboard
		else {
			float vertical = Input.GetAxis ("Vertical");
			float horizontal = Input.GetAxis ("Horizontal");

			// rotate
			if((horizontal < 0) && !enableLeftToggle.isOn){
				
			} else if((horizontal > 0) && !enableRightToggle.isOn){
				
			} else {
				Quaternion rotateBy = Quaternion.Euler (0, horizontal * rotationSpeedSlider.value * Time.deltaTime, 0);
				rigidBody.MoveRotation (transform.rotation * rotateBy);
			}

			// translate
			if((vertical < 0) && !enableReverseToggle.isOn){
				
			} else if((vertical > 0) && !enableForwardToggle.isOn){
				
			} else {
				Vector3 moveBy = transform.forward * vertical * translationSpeedSlider.value * Time.deltaTime;
				rigidBody.MovePosition (transform.position + moveBy);
			}
		}
	}
}


















