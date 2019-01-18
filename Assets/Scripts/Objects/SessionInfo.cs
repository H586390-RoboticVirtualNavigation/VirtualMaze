using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Since Unity is unable to send parameters across Scenes, 
/// this static class will be used to hold the required parameters
/// to pass to BasicLevelController
/// </summary>
public static class SessionInfo {
    private static int trialTimeLimit; // time to complete each trial.
    private static Session session; // current Session that is running

    private static float timeoutDuration;

    public static void SetSessionInfo(
            int trialTimeLimit, 
            Session session, 
            float timeoutDuration
        ) {
        SessionInfo.trialTimeLimit = trialTimeLimit;
        SessionInfo.session = session;
        SessionInfo.timeoutDuration = timeoutDuration;
    }

    public static void GetSessionInfo(
            out int trialTimeLimit, 
            out Session session,
            out float timeoutDuration
        ) {
        trialTimeLimit = SessionInfo.trialTimeLimit;
        session = SessionInfo.session;
        timeoutDuration = SessionInfo.timeoutDuration;
    }
}
