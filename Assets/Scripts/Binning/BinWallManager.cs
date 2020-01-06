using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class BinWallManager {
    const int GroundCeiling = 0;
    const int PillarWalls = 1;
    const int MazeWalls = 2;

    static List<Vector2> primaryOffset = new List<Vector2>();
    static List<Vector2> secondaryOffset = new List<Vector2>();
    private static float gazeSqDistThreshold;

    private static Dictionary<int, List<BinWall>> wallCache = new Dictionary<int, List<BinWall>>();

    private static HashSet<string> activated = new HashSet<string>();

    static int a = LayerMask.NameToLayer("");

    static readonly int layerMaskArea;
    static readonly int layerMaskRing;

    const int UNIT_AREA = 100 * 100; //100 by 100 pixels
    const float TURN_FRACTION = 0.618f * 2 * Mathf.PI;

    static BinWallManager() {
        int shift = LayerMask.NameToLayer("Binning");
        layerMaskArea = (1 << shift);
        layerMaskRing = (1 << shift) ^ -5; //-5 is default layermask, XOR with desired mask
        ReconfigureGazeOffsetCache(50, 220); //radius of 50 pixels, density of approx 1 raycast per 45 pixels
    }
    public static void DisplayGazes(List<Vector2> gazes, Camera c, GameObject binWallPrefab, BinMapper mapper) {
        IdentifyObjects(gazes, c, binWallPrefab, mapper);
        BinGazes(gazes, c, binWallPrefab, mapper);
        gazes.Clear();
    }

    public static void BinGaze(Vector2 gaze, Camera cam, GameObject binWallPrefab, BinMapper mapper, HashSet<int> binsHitId) {
        if (gaze.isNaN()) {
            return;
        }

        int numRaycasts = secondaryOffset.Count;

        NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(numRaycasts, Allocator.TempJob);
        NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(numRaycasts, Allocator.TempJob);
        try {
            int counter = 0;
            foreach (Vector2 offset in secondaryOffset) {
                Ray r = cam.ScreenPointToRay(gaze + offset);
                commands[counter] = new RaycastCommand(r.origin, r.direction, float.MaxValue, layerMaskArea);
                counter++;
            }

            JobHandle h = RaycastCommand.ScheduleBatch(commands, results, 1, default);
            h.Complete();

            for (int i = 0; i < numRaycasts; i++) {
                Collider c = results[i].collider;
                if (c != null) {
                    Bin b = c.GetComponent<Bin>();
                    b.Hit();

                    int mappedId = mapper.MapBinToId(b.parent, b);

                    binsHitId.Add(mappedId);

                    b.idText.text = mappedId.ToString();
                    b.SetTextCanvasActive(true);
                }
            }
        }
        finally {
            commands.Dispose();
            results.Dispose();
        }
    }

    public static void BinGazes(IEnumerable<Vector2> gazes, Camera cam, GameObject binWallPrefab, BinMapper mapper) {
        int numRaycasts = secondaryOffset.Count;

        NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(numRaycasts, Allocator.TempJob);
        NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(numRaycasts, Allocator.TempJob);

        Vector2 prevGaze = Vector2.positiveInfinity;

        try {
            foreach (Vector2 gaze in gazes) {
                if (gaze.isNaN()) {
                    continue;
                }

                //raycast only if gaze is sufficently far from the previous gaze
                if ((gaze - prevGaze).sqrMagnitude < gazeSqDistThreshold) {
                    continue;
                }

                prevGaze = gaze;

                int counter = 0;
                foreach (Vector2 offset in secondaryOffset) {
                    Ray r = cam.ScreenPointToRay(gaze + offset);
                    commands[counter] = new RaycastCommand(r.origin, r.direction, float.MaxValue, layerMaskArea);
                    counter++;
                }

                JobHandle h = RaycastCommand.ScheduleBatch(commands, results, 1, default);
                h.Complete();

                for (int i = 0; i < numRaycasts; i++) {
                    Collider c = results[i].collider;
                    if (c != null) {
                        Bin b = c.GetComponent<Bin>();
                        b.Hit();

                        b.idText.text = mapper.MapBinToId(b.parent, b).ToString();
                        b.SetTextCanvasActive(true);
                    }
                }
            }
        }
        finally {
            commands.Dispose();
            results.Dispose();
        }
    }

    public static void PrepareBins(Vector2 gaze, Camera cam, GameObject binWallPrefab, BinMapper mapper) {
        int numRaycasts = primaryOffset.Count;

        NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(numRaycasts, Allocator.TempJob);
        NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(numRaycasts, Allocator.TempJob);

        int counter = 0;
        HashSet<Collider> hitPool = new HashSet<Collider>();

        try {
            foreach (Vector2 offset in primaryOffset) {
                Ray r = cam.ScreenPointToRay(gaze + offset);
                commands[counter] = new RaycastCommand(r.origin, r.direction, float.MaxValue, layerMaskRing);
                counter++;
            }

            JobHandle h = RaycastCommand.ScheduleBatch(commands, results, 1, default);
            h.Complete();

            for (int i = 0; i < counter; i++) {
                Collider c = results[i].collider;
                if (c != null) {
                    hitPool.Add(c);
                }
            }
        }
        finally {
            commands.Dispose();
            results.Dispose();
        }
        foreach (Collider c in hitPool) {
            BinWallManager.AssignBinwall(c.gameObject, binWallPrefab, mapper);
        }
    }

    public static void IdentifyObjects(List<Vector2> gazes, Camera cam, GameObject binWallPrefab, BinMapper mapper) {
        int numRaycasts = gazes.Count * primaryOffset.Count;

        NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(numRaycasts, Allocator.TempJob);
        NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(numRaycasts, Allocator.TempJob);

        int counter = 0;

        HashSet<Collider> hitPool = new HashSet<Collider>();

        try {
            foreach (Vector2 gaze in gazes) {
                if (gaze.isNaN()) {
                    continue;
                }

                foreach (Vector2 offset in primaryOffset) {
                    Ray r = cam.ScreenPointToRay(gaze + offset);
                    commands[counter] = new RaycastCommand(r.origin, r.direction, float.MaxValue, layerMaskRing);
                    counter++;
                }
            }

            JobHandle h = RaycastCommand.ScheduleBatch(commands, results, 1, default);
            h.Complete();

            for (int i = 0; i < counter; i++) {
                Collider c = results[i].collider;
                if (c != null) {
                    hitPool.Add(c);
                }
            }
        }
        finally {
            commands.Dispose();
            results.Dispose();
        }
        foreach (Collider c in hitPool) {
            BinWallManager.AssignBinwall(c.gameObject, binWallPrefab, mapper);
        }
    }

    public static void ReconfigureGazeOffsetCache(int radius, int density) {
        gazeSqDistThreshold = Mathf.Pow(radius / 2f, 2) + 10;

        /*primary offset density is reduced as it is only used to identify all
          possible objects in the scene covered by the gaze */
        CreateOffsetList(radius, Mathf.Max(density / 10, 10), primaryOffset);
        CreateOffsetList(radius, density, secondaryOffset);
    }

    /// <summary>
    /// Clears and fills the given list with a spread of evenly spaced Direction vectors
    /// </summary>
    /// <param name="radius">Radius of the spread in pixels</param>
    /// <param name="density">Density of the vectors per unit area (in a 100 by 100 pixel square)</param>
    /// <param name="toFill">List to fill</param>
    private static void CreateOffsetList(float radius, int density, List<Vector2> toFill) {
        toFill.Clear();

        //Calculate number of Rays
        int numRays = Mathf.CeilToInt(Mathf.Pow(radius, 2) * Mathf.PI * density / UNIT_AREA);

        for (int i = 0; i < numRays; i++) {
            //sqrt to reduce clustering at the center
            float dst = Mathf.Sqrt(i / (numRays - 1f)) * radius;
            float angle = TURN_FRACTION * i;

            float x = dst * Mathf.Cos(angle);
            float y = dst * Mathf.Sin(angle);

            toFill.Add(new Vector2(x, y));
        }
    }

    public static void AssignBinwall(GameObject obj, GameObject binWallPrefab, BinMapper mapper) {
        if (activated.Contains(obj.name)) {
            return;
        }

        activated.Add(obj.name);

        int group = mapper.GetGroupID(obj);
        string specialID = mapper.GetSpecialCacheId(group, obj);

        if (!string.IsNullOrEmpty(specialID)) {
            if (activated.Contains(specialID)) {
                return;
            }
            else {
                activated.Add(specialID);
            }
        }

        Location location = mapper.GetLocationOfBinWall(group, obj);

        BinWall binWall = GetAvailableBinWall(group, binWallPrefab, mapper);
        binWall?.AttachTo(location, obj.name, group);
    }

    public static void Reset() {
        activated.Clear();
        foreach (List<BinWall> pool in wallCache.Values) {
            foreach (BinWall wall in pool) {
                wall.Reset();
                wall.gameObject.SetActive(false);
            }
        }
    }

    private static BinWall GetAvailableBinWall(int group, GameObject binWallPrefab, BinMapper mapper) {
        if (group == BinMapper.NoGroup) {
            return null;
        }

        BinWallConfig config = mapper.MapObjectToBinWallConfig(group, 40);

        int cache_id = mapper.MapGroupToWallCache(group);

        if (cache_id == 1 && activated.Contains(group.ToString())) {
            return null;
        }

        activated.Add(group.ToString());

        if (wallCache.TryGetValue(cache_id, out List<BinWall> pool)) {
            foreach (BinWall wall in pool) {
                if (!wall.gameObject.activeSelf) {
                    return wall;
                }
            }
        }
        else {
            pool = new List<BinWall>();
            wallCache.Add(cache_id, pool);
        }

        BinWall binWall = GameObject.Instantiate(binWallPrefab).GetComponent<BinWall>();

        binWall.CreateWall(config);
        pool.Add(binWall);
        return binWall;
    }
}
