using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Since Unity is unable to send parameters across Scenes, 
/// this static class will be used to hold the required parameters
/// to pass to BasicLevelController and any inherited classes.
/// </summary>
public static class SessionInfo {
    private static Session session; // current Session that is running

    public static void SetSessionInfo(Session session) {
        SessionInfo.session = session;
    }

    public static void GetSessionInfo(out Session session) {
        session = SessionInfo.session;
    }
}
