using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mazes/MazeList")]
public class MazeList : ScriptableObject {
    [SerializeField]
    private AbstractMaze[] _mazes = null;

    //First Maze is the default maze
    public AbstractMaze DefaultMaze { get => _mazes[0]; }

    public AbstractMaze this[int idx] {
        get => Instantiate(_mazes[idx]);
    }

    public int Length { get => _mazes.Length; }

    private Dictionary<string, AbstractMaze> nameMazeMap = new Dictionary<string, AbstractMaze>();

    private void PopulateMap() {
        foreach (AbstractMaze m in _mazes) {
            nameMazeMap.Add(m.MazeName, m);
        }
    }

    public bool TryGetMaze(string mazeName, out AbstractMaze maze) {
        if (nameMazeMap.Count == 0) {
            PopulateMap();
        }

        bool success = nameMazeMap.TryGetValue(mazeName, out AbstractMaze m);
        if (success) {
            // return a new copy so that data will not be shared across all places that use this object.
            maze = Instantiate(m);
        }
        else {
            maze = null;
        }

        return success;
    }
}
