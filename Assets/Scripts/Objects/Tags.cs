/// <summary>
/// Static class to hold all default tags and user defined tags.
/// 
/// User defined tags can be added via Edit > Project Settings > Tags and Layers
/// </summary>
public static class Tags {
    //default tags
    public static readonly string Respawn = "Respawn";
    public static readonly string EditorOnly = "EditorOnly";
    public static readonly string Finish = "Finish";
    public static readonly string MainCamera = "MainCamera";
    public static readonly string Player = "Player";
    public static readonly string GameController = "GameController";

    //user defined tags
    public static readonly string Reward = "Reward";
    public static readonly string CalibPoint = "CalibPoint";

    //All rewardAreas Gameobjects should be tagged as a RewardArea so that BasicLevelControllers
    //and their extended classes can find them easily.
    public static readonly string RewardArea = "RewardArea";
}
