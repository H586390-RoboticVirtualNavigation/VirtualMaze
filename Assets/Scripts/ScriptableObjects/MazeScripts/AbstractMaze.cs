using UnityEngine;

public abstract class AbstractMaze : ScriptableObject {
    public int numTrials = 0;

    [SerializeField]
    private string mazeName = null;

    public string MazeName { get => mazeName; }

    public abstract MazeLogic Logic { get; }
    public abstract string Scene { get; }
}
