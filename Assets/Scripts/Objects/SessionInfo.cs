using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Since Unity is unable to send parameters across Scenes, 
/// this static class will be used to hold the required parameters
/// to pass to BasicLevelController
/// </summary>
public static class SessionInfo {
    public static int trialTimeLimit; // time to complete each trial.
    public static Session session; // current Session that is running

    public static float timeoutDuration;
}
