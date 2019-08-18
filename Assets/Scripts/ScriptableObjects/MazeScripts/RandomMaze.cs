using UnityEngine;

[CreateAssetMenu(menuName = "Mazes/Random Maze")]
public class RandomMaze : AbstractMaze {

    private int idx = -1;

    [SerializeField]
    private AbstractMaze[] scenePool = null;

    /// <summary>
    /// Randomly selects a scene from the pool.
    /// </summary>
    public override string Scene {
        get {
            if (idx == -1) {
                // In the editor, the first random value does not seem to change across runs
                // but it does change as expected when the game is built.
                idx = Random.Range(0, scenePool.Length);
            }

            return scenePool[idx].Scene;
        }
    }

    /// <summary>
    /// Returns null if random scene not selected yet.
    /// </summary>
    public override MazeLogic Logic {
        get {
            if (idx == -1) {
                return null;
            }

            return scenePool[idx].Logic;
        }
    }

    public AbstractMaze[] ScenePool { get => scenePool; }
}
