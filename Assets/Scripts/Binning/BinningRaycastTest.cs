using System.Collections.Generic;
using UnityEngine;

public class BinningRaycastTest : MonoBehaviour {
    [SerializeField]
    private int radius = BinWallManager.Default_Radius;
    [SerializeField]
    private int density = BinWallManager.Default_Density;
    [SerializeField]
    private int numBinForLength = BinMapper.DEFAULT_NUM_BIN_LENGTH;
    [SerializeField]
    private Vector2 gazepoint = new Vector2(1920 / 2f, 1080 / 2f);

    [SerializeField]
    private Camera c = null;

    [SerializeField]
    private GameObject binWallPrefab = null;

    private List<Vector2> gazes;
    private int prev_numBinForLength = -1;

    /// <summary>
    /// Change the mapper as required. If there is alot of mappers in the future,
    /// Consider making binmappers inherit ScriptableObject so that mappers can be
    /// easily tested by dragging and dropping.
    /// </summary>
    private DoubleTeeBinMapper mapper = null;

    private void Awake() {
        // this script should only run in development/editor mode
        if (!Debug.isDebugBuild) {
            gameObject.SetActive(false);
        }
    }

    private void Start() {
        gazes = new List<Vector2>() { gazepoint };
    }

    int count = 0;

    private void Update() {
        BinWallManager.ReconfigureGazeOffsetCache(radius, density);
        Physics.SyncTransforms();
        if (mapper == null || prev_numBinForLength != numBinForLength) {
            mapper = new DoubleTeeBinMapper(numBinForLength);
            prev_numBinForLength = numBinForLength;
        }

        gazes[0] = gazepoint;

        BinWallManager.ResetWalls();
        BinWallManager.IdentifyObjects(gazes, c, binWallPrefab, mapper);
        BinWallManager.ViewBinGazes(gazes, c, binWallPrefab, mapper);
    }
}
