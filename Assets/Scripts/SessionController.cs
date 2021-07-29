using System;
using System.Collections.Generic;

public class SessionController : ConfigurableComponent {
    [Serializable]
    public class Settings : ComponentSettings {
        public List<Tuple<int, string>> serializableSessions;

        public Settings() {
            serializableSessions = new List<Tuple<int, string>>();
        }

        public Settings(List<Session> sessions) : this() {
            foreach (Session s in sessions) {
                serializableSessions.Add(new Tuple<int, string>(s.numTrials, s.maze.MazeName));
            }
        }

        public void LoadSessions(List<Session> sessions, MazeList mazeList) {
            foreach (Tuple<int, string> data in serializableSessions) {
                if (mazeList.TryGetMaze(data.Item2, out AbstractMaze maze)) {
                    sessions.Add(new Session(data.Item1, maze));
                }
            }
        }
    }
    
    public MazeList masterList;
    public List<Session> sessions { get; private set; } = new List<Session>();
    public int index { get; private set; } = 0;
    
    public void RestartIndex() {
        index = 0;
    }

    public bool HasNextLevel() {
        return (index + 1) <= sessions.Count;
    }

    public Session NextLevel() {
        if (index < sessions.Count) {
            Session session = sessions[index];
            index++;

            return session;
        }
        else {
            return null;
        }
    }

    //updates the session Name at the given position
    public void UpdateSessionNameAt(int pos, AbstractMaze maze) {
        sessions[pos].maze = maze;
    }

    //updates the session Name at the given position
    public void UpdateSessionNumTrialAt(int pos, int numTrial) {
        sessions[pos].numTrials = numTrial;
    }

    public void RemoveSessionAt(int pos) {
        sessions.RemoveAt(pos);
    }

    public Session AddSession() {
        Session s = new Session(masterList.DefaultMaze);//masterList.DefaultMaze));
        sessions.Add(s);
        return s;
    }

    public override Type GetSettingsType() {
        return typeof(Settings);
    }

    public override ComponentSettings GetDefaultSettings() {
        return new Settings();
    }

    public override ComponentSettings GetCurrentSettings() {
        return new Settings(sessions);
    }

    protected override void ApplySettings(ComponentSettings loadedSettings) {
        Settings s = (Settings)loadedSettings;
        //fill sessions with the saved sessions.
        sessions.Clear();
        s.LoadSessions(sessions, masterList);
    }
}
