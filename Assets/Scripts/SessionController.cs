using System;
using System.Collections.Generic;
using UnityEngine;

public class SessionController : ConfigurableComponent {
    [Serializable]
    public class Settings : ComponentSettings {
        public List<Session> sessions;

        public Settings() {
            sessions = new List<Session>();
        }

        public Settings(List<Session> sessions) {
            this.sessions = new List<Session>(sessions);
        }
    }

    public List<Session> Sessions { get; private set; } = new List<Session>();
    public int index { get; private set; } = 0;

    public void RestartIndex() {
        index = 0;
    }

    public bool HasNextLevel() {
        return (index + 1) <= Sessions.Count;
    }

    public Session NextLevel() {
        Session session = Sessions[index];

        switch (session.level) {
            case Session.RandLRFLevel:
                session = new Session(session.numTrials, Session.GetRandomLRFLevel());
                break;
            case Session.RandomLevel:
                session = new Session(session.numTrials, Session.GetRandomLevel());
                break;
            default:
                break;
        }

        index++;

        return session;
    }

    //updates the session Name at the given position
    public void UpdateSessionNameAt(int pos, string newName) {
        Sessions[pos].level = newName;
    }

    //updates the session Name at the given position
    public void UpdateSessionNumTrialAt(int pos, int numTrial) {
        Sessions[pos].numTrials = numTrial;
    }

    public void RemoveSessionAt(int pos) {
        Sessions.RemoveAt(pos);
    }

    public Session AddSession() {
        Session s = new Session();
        Sessions.Add(s);
        return s;
    }

    public override Type GetSettingsType() {
        return typeof(Settings);
    }

    public override ComponentSettings GetDefaultSettings() {
        return new Settings();
    }

    public override ComponentSettings GetCurrentSettings() {
        return new Settings(Sessions);
    }

    protected override void ApplySettings(ComponentSettings loadedSettings) {
        Settings s = (Settings)loadedSettings;
        //fill sessions with the saved sessions.
        Sessions.Clear();
        Sessions.AddRange(s.sessions);
    }
}
