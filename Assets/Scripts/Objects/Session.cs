using System;
using Random = UnityEngine.Random;

/// <summary>
/// C# class to encapsulate the data required to define a session.
/// 
/// This Class represents an experiemnt session and dictates the duration
/// to between trial runs.
/// </summary>
[Serializable]
public class Session {
    //scriptable object this?
    // Configs for all sessions (Class variables)
    public static int timeoutDuration;
    public static int trialTimeLimit; // time to complete each trial.

    // flag to determine if trail Intermission Duration is randomised or not
    public static bool isTrailIntermissionRandom = false;
    public static int fixedTrialIntermissionDuration;

    //Values for random trialIntermissionDuration
    public static int maxTrialIntermissionDuration;
    public static int minTrialIntermissionDuration;

    /// <summary>
    /// number of trails to run in this maze
    /// 
    /// Note this should not be in the proposed scriptable object
    /// </summary>
    public int numTrials;


    public AbstractMaze maze = null;
    public string MazeScene { get => maze.Scene; }

    //Give a new instance of logic scriptable object
    public IMazeLogicProvider MazeLogic { get => UnityEngine.Object.Instantiate(maze.Logic); }

    //Constructors
    //default number of trials in a session is 1.
    public Session(AbstractMaze maze) : this(1, maze) { }

    public Session(int numTrials, AbstractMaze maze) {
        this.numTrials = numTrials;
        this.maze = maze;
    }


    /// <summary>
    /// Returns the amount of time to delay, Fixed or Randomised.
    /// </summary>
    /// <returns></returns>
    public static float getTrailIntermissionDuration() {
        if (isTrailIntermissionRandom) {
            return Random.Range(minTrialIntermissionDuration, maxTrialIntermissionDuration);
        }
        else {
            return fixedTrialIntermissionDuration;
        }
    }

    public override string ToString() {
        return "numtrials: " + numTrials + "\nlevel: " + maze.MazeName;
    }
}
