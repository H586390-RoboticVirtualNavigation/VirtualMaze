using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// C# class to encapsulate the data required to define a session
/// </summary>
/// 
[Serializable]
public class Session {
    /// <summary>
    /// Special level to signify a small random range of other levels where 
    /// levels are designed for restricted movement.
    /// </summary>
    public const String RandLRFLevel = "RandLRF";

    /// <summary>
    /// Special level to signify a small random range of other levels
    /// </summary>
    public const String RandomLevel = "Random";

    /// <summary>
    /// Array of all possible Levels to be used.
    /// 
    /// If a level exists here, make sure Unity has the scene created and 
    /// added into the build settings of VirtualMaze or the option is properly mapped
    /// to other levels in SessionController.
    /// </summary>
    [NonSerialized]
    public static readonly string[] AllLevels = {
        RandomLevel,
        RandLRFLevel,
        "Linear",
        "Tee",
        "Four-Arm",
        "TrainForward",
        "TrainRight",
        "TrainLeft",
        "Double Tee",
        "Tee Left",
        "Tee Left Block",
        "Tee Right",
        "Tee Right Block",
        "Back and Forth 3"
    };

    /// <summary>
    /// Pool of levels to be randomised.
    /// 
    /// If a level exists here, make sure Unity has the scene created and 
    /// added into the build settings of VirtualMaze or the option is properly mapped
    /// to other levels in SessionController.
    /// </summary>
    [NonSerialized]
    public static readonly string[] RandomLevels = {
        "Linear",
        "Tee",
        "Four-Arm",
        "TrainForward",
        "TrainRight",
        "TrainLeft",
        "Double Tee",
        "Tee Left",
        "Tee Left Block",
        "Tee Right",
        "Tee Right Block",
        "Back and Forth 3"
    };

    /// <summary>
    /// Pool of levels to train restricted directional mazes.
    /// 
    /// If a level exists here, make sure Unity has the scene created and 
    /// added into the build settings of VirtualMaze or the option is properly mapped
    /// to other levels in SessionController.
    /// </summary>
    [NonSerialized]
    private static readonly string[] RandomLRFLevels = {
        "TrainForward",
        "TrainRight",
        "TrainLeft",
    };

    /// <summary>
    /// number of trails to run in this session
    /// </summary>
    public int numTrial;

    
    private string _level;
    /// <summary>
    /// Name or Identifier for this Session. Name must exists in the AllLevels
    /// </summary>
    public string level {
        get { return _level; }
        set {
            if (Array.IndexOf(AllLevels, value) != -1) {
                _level = value;
            }
            else {
                throw new LevelDoesNotExistException();
            }
        }
    }

    /// <summary>
    /// True if session is generated randomly
    /// </summary>
    public bool isRandom { get; private set; } = true; //true since default value is random.

    //Constructors
    //default trails is the first on in the All trials array.
    public Session() : this(AllLevels[0]){ }

    //default number of trials in a session is 1.
    public Session(string level) : this(1, level) { }

    public Session(int numTrial, string level) {
        this.numTrial = numTrial;
        this.level = level;
    }


    public static string GetRandomLRFLevel() {
        int random = UnityEngine.Random.Range(0, RandomLRFLevels.Length);
        
        return RandomLRFLevels[random];
    }

    public static string GetRandomLevel() {
        int random = UnityEngine.Random.Range(0, AllLevels.Length);

        return AllLevels[random];
    }

    public class LevelDoesNotExistException : Exception {
        public LevelDoesNotExistException() { }
        public LevelDoesNotExistException(string message) : base(message) { }
        public LevelDoesNotExistException(string message, Exception inner) : base(message, inner) { }
    }

    public override string ToString() {
        return "numtrials: " + numTrial + "\nlevel: " + level;
    }
}
