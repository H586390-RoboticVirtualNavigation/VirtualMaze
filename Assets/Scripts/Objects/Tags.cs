﻿/// <summary>
/// Static class to hold all default tags and user defined tags.
/// 
/// User defined tags can be added via Edit > Project Settings > Tags and Layers
/// 
/// Any changes to the user defined tags must be reflected in "Tags and Layers" and
/// vice versa.
/// </summary>
public static class Tags {
    //default tags
    public static readonly string Respawn = "Respawn";
    public static readonly string EditorOnly = "EditorOnly";
    public static readonly string Finish = "Finish";
    public static readonly string MainCamera = "MainCamera";
    public static readonly string Player = "Player";
    public static readonly string GameController = "GameController";

    // user defined tags
    public static readonly string Reward = "Reward";
    public static readonly string CalibPoint = "CalibPoint";
    public static readonly string Waypoint = "Waypoint";

    // All rewardAreas Gameobjects should be tagged as a RewardArea so that BasicLevelControllers
    //and their extended classes can find them easily.
    public static readonly string RewardArea = "RewardArea";

    // BasicLevelController Gameobjects and their extended classes should be tagged with this
    public static readonly string LevelController = "LevelController";

    // Experiement Console Tag. GameObjects with a Text Component to be used as an Experiment Console should
    // have this tag.
    public  static readonly string Console = "Console";
}
