using UnityEngine;

public class MazeList : MonoBehaviour {
    private static MazeList _instance;

    [SerializeField]
    private AbstractMaze[] _mazes = null;

    public AbstractMaze[] Mazes { get => _mazes; }
    public static MazeList Instance { get => _instance; }

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject); // destroy self
        }
        else {
            _instance = this;
        }

        DontDestroyOnLoad(this);
    }

    public AbstractMaze this[int idx] {
        get => _mazes[idx];
    }

    private void Start() {

        foreach (AbstractMaze m in MazeList.Instance.Mazes) {
            print(m.MazeName);
            print(m.Scene);
            print(m.Logic.IsTrialCompleteAfterReward());
        }
    }
}
