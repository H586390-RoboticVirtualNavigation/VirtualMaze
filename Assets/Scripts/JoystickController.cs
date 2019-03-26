using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Timers;
using System;
using UnityEngine.UI;

//NOTE: static class cannot derive, but singleton can
// TODO convert to Co routine?
public class JoystickController : ConfigurableComponent {
    [Serializable]
    public class Settings : ComponentSettings {
        [Range(0, 1)]
        public float deadzoneAmount = 0;
        public string portNum = "";
    }

    public int baudRate = 115200;

    /// <summary>
    ///  more than 0 is right,
    ///  less than 0 is left
    /// </summary>
	public static float horizontal { get; private set; }
    public static float vertical { get; private set; }

    [SerializeField]
    private Settings settings;

    public float DeadzoneAmount { get => settings.deadzoneAmount; set => settings.deadzoneAmount = value; }
    public string PortNum { get => settings.portNum; set => settings.portNum = value; }

    public bool isOpen { get; private set; }

    private static byte[] buffer = new byte[2]; //serial must read byte (uint8), however must convert to sbyte (int8) later
    private static Timer timer;
    private static SerialPort serial;

    private static JoystickController _instance;
    public static JoystickController instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<JoystickController>();
                if (_instance == null) {
                    Debug.LogError("Need one SerialController Object in Scene");
                }
                else {
                    DontDestroyOnLoad(_instance.gameObject);
                }
            }
            return _instance;
        }
    }

    protected override void Awake() {
        base.Awake();

        if (_instance == null) {
            _instance = this;
        }
        else if (this.gameObject != _instance.gameObject) {
            Destroy(this.gameObject);
        }


    }

    public bool JoystickOpen() {
        if (serial == null) {
            Debug.Log($"Opening serial on {settings.portNum} at {baudRate}");

            try {
                serial = new SerialPort(settings.portNum, baudRate);
                serial.ReadTimeout = 16;
                serial.Open();
            }
            catch {
                Debug.LogError("cannot open serial port");
                JoystickClose();
                return false;
            }
        }
        if (timer == null) {
            Debug.Log("Listening for serial joystick events every 60 ms");
            Debug.Log("created new timer");
            timer = new Timer();
            timer.Elapsed += TimerEvent;
            timer.AutoReset = true;
            timer.Interval = 60;
            timer.Enabled = true;
        }
        isOpen = true;
        return true;
    }

    public void JoystickClose() {
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
            timer.Stop();
            timer.Dispose();
            timer = null;
        }
        isOpen = false;
    }

    void OnApplicationQuit() {
        JoystickClose();
    }

    void TimerEvent(object sender, ElapsedEventArgs e) {

        if (serial != null) {
            serial.Write("r");
            serial.Read(buffer, 0, 2);

            // get joystick axis readings
            horizontal = ((sbyte)buffer[0]) / 128f;
            vertical = ((sbyte)buffer[1]) / 128f;

            //apply deadzone
            horizontal = ApplyDeadzone(horizontal);
            vertical = ApplyDeadzone(vertical);
        }
    }

    private float ApplyDeadzone(float value) {
        if (Math.Abs(value) > settings.deadzoneAmount) {
            return value;
        }
        return 0;
    }

    public override ComponentSettings GetCurrentSettings() {
        return settings;
    }

    public override ComponentSettings GetDefaultSettings() {
        return new Settings();
    }

    public override Type GetSettingsType() {
        return typeof(Settings);
    }

    protected override void ApplySettings(ComponentSettings loadedSettings) {
        settings = (Settings)loadedSettings;
    }
}
