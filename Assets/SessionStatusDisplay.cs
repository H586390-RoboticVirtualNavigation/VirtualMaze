using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SessionStatusDisplay : MonoBehaviour {
    public const float TickAmount = 0.1f;

    public Text SessionNumberText;
    public Text TrialNumberText;
    public Text SessionStatusText;
    public Text TimeLeftText;

    public static SessionStatusDisplay Instance { get; private set; }

    public static WaitForSeconds waitForTick = new WaitForSeconds(TickAmount);

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }
    }

    private void Start() {
        ResetStatus();
    }

    public static void ResetStatus() {
        DisplaySessionNumber(0);
        DisplaySessionStatus("");
        DisplayTrialNumber(0);
        DisplayTimeLeft(0);
    }

    public static void DisplaySessionNumber(int sessionNum) {
        Instance.SessionNumberText.text = sessionNum.ToString();
    }

    public static void DisplayTrialNumber(int trialNum) {
        Instance.TrialNumberText.text = trialNum.ToString();
    }

    public static void DisplaySessionStatus(String status) {
        Instance.SessionStatusText.text = status;
    }

    public static void DisplayTimeLeft(float seconds) {
        Instance.TimeLeftText.text = seconds.ToString("0.00");
    }

    /// <summary>
    /// Helper method to wait and display the time left in the GUI.
    /// 
    /// Simple Countdown for a defined number of seconds
    /// </summary>
    /// <param name="status">String to represent the status</param>
    /// <param name="seconds">Number of Seconds to count down from</param>
    /// <returns></returns>
    public static IEnumerator Countdown(string status, float seconds) {
        float timeLeft = seconds;
        DisplaySessionStatus(status);
        while (timeLeft > 0) {
            DisplayTimeLeft(timeLeft);
            yield return waitForTick;
            Tick(timeLeft, out timeLeft);
        }
    }

    /// <summary>
    /// Helper method to countdown values based on the tick defined here.
    /// 
    /// This method is used to support a more complex countdown timer.
    /// </summary>
    /// <param name="timeLeft">Time left for the countdown</param>
    /// <param name="nextTimeLeft">Remainding time after this Tick</param>
    /// <returns>WaitForSeconds for caller to yield</returns>
    public static WaitForSeconds Tick(float timeLeft, out float nextTimeLeft) {
        DisplayTimeLeft(timeLeft);
        nextTimeLeft = timeLeft - TickAmount;
        return waitForTick;
    }
}
