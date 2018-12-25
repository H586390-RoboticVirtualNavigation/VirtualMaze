using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Timers;
using System;
using UnityEngine.UI;

//NOTE: static class cannot derive, but singleton can

public class SerialController : MonoBehaviour {

	public int baudRate;

    /// <summary>
    ///  more than 0 is right,
    ///  less than 0 is left
    /// </summary>
	public static float horizontal { get; private set;}

	public static float vertical { get; private set;}

	private static byte[] buffer = new byte[2]; //serial must read byte (uint8), however must convert to sbyte (int8) later
	private static Timer timer;
	private static SerialPort serial;
	private static SerialPort rewardSerial;

	private Slider joystickDeadzoneSlider;

	private static SerialController _instance;
	public static SerialController instance {
		get {
			if(_instance == null){
				_instance = FindObjectOfType<SerialController>();
				if(_instance == null){
					Debug.LogError("Need one SerialController Object in Scene");
				}else{
					DontDestroyOnLoad(_instance.gameObject);
				}
			}
			return _instance;
		}
	}

	void Awake() {

		// get slider
		//joystickDeadzoneSlider = GameObject.Find ("JoystickDeadzoneSlider").GetComponent <Slider> ();

		if (_instance == null) {
			_instance = this;
		} else if (this.gameObject != _instance.gameObject) {
			Destroy(this.gameObject);
		}
	}

	public bool RewardValveOn(string serialPort){
		//dispose if exists
		if (rewardSerial != null) {
			rewardSerial.Close();
			rewardSerial.Dispose();
		}

		//open port
		try{
			rewardSerial = new SerialPort (serialPort, 9600);
			rewardSerial.Open ();
			rewardSerial.ReadTimeout = 60;
			rewardSerial.DtrEnable = true;
			rewardSerial.RtsEnable = true;

		} catch {
			return false;
		}
		return true;
	}

	public void RewardValveOff(){
		if (rewardSerial != null) {
			rewardSerial.Close();
			rewardSerial.Dispose();
			rewardSerial = null;
		}
	}

	public bool JoystickOpen(string serialPort){
		if(serial == null){
			Debug.Log ("Opening serial on " + serialPort + " at " + baudRate);

			try{
				serial = new SerialPort (serialPort, baudRate);
				serial.Open ();
				serial.ReadTimeout = 60;
			} catch {
				Debug.LogError ("cannot open serial port");
				JoystickClose();
				return false;
			}
		}
		if (timer == null) {
			Debug.Log("Listening for serial joystick events every 60 ms");
			Debug.Log("created new timer");
			timer = new Timer ();
			timer.Elapsed += TimerEvent;
			timer.Interval = 60;
			timer.Enabled = true;
		}
		return true;
	}

	public void JoystickClose(){
		if (serial != null) {
			Debug.Log("Closing serial");
			serial.Close();
			serial.Dispose();
			serial = null;
		}
		if (timer != null) {
			Debug.Log("Stop listening for serial joystick events");
			timer.Elapsed -= TimerEvent;
			timer.Enabled = false;
			timer.Dispose();
			timer = null;
		}
	}

	void OnApplicationQuit() {
		if (serial != null) {
			Debug.Log("Closing serial");
			serial.Close();
			serial.Dispose();
			serial = null;
		}
		if (timer != null) {
			Debug.Log("Stop listening for serial joystick events");
			timer.Elapsed -= TimerEvent;
			timer.Enabled = false;
			timer.Dispose();
			timer = null;
		}
	}

	void TimerEvent(object sender, ElapsedEventArgs e){

		if (serial != null) {
			serial.Write ("r");
			serial.Read (buffer, 0, 2);

			// get joystick axis readings
			horizontal = (sbyte)buffer [0] / 128.0f;
			vertical = (sbyte)buffer [1] / 128.0f;
			horizontal = Math.Abs(horizontal) < joystickDeadzoneSlider.value ? 0 : horizontal;
			vertical = Math.Abs (vertical) < joystickDeadzoneSlider.value ? 0 : vertical;
		}
	}

	public static void Write(string str){
		if (serial != null) {
			serial.Write (str);
		}
	}
}











