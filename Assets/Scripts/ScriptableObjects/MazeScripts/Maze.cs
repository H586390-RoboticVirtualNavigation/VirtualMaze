using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Mazes/Maze")]
[Serializable]
public class Maze : AbstractMaze {
    [SerializeField]
    private string scene = null;

    [SerializeField]
    private MazeLogic logic = null;

    public override MazeLogic Logic => logic;

    public override string Scene => scene;
}