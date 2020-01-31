using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

public class BinWallManager {
    const int GroundCeiling = 0;
    const int PillarWalls = 1;
    const int MazeWalls = 2;

    public static List<Vector2> primaryOffset = new List<Vector2>();
    public static List<Vector2> secondaryOffset = new List<Vector2>();
    public static float gazeSqDistThreshold;

    private static Dictionary<int, List<BinWall>> wallCache = new Dictionary<int, List<BinWall>>();

    private static HashSet<string> activated = new HashSet<string>();
    public static float maxSqDist;

    private static readonly int binningLayerOnly;
    public static readonly int ignoreBinningLayer;

    const int UNIT_AREA = 100 * 100; //100 by 100 pixels

    /* 0.618 is golden ratio, reduces chances of raycast overlapping */
    private const float TURN_FRACTION = 0.618f * 2 * Mathf.PI;

    private const int MINIMUM_RAYCAST_DENSITY = 10;

    /* Reduces the density by this amount */
    private const int PRI_DENSITY_DIVISOR = 10;
    public const int Default_Radius = 50;
    public const int Default_Density = 220;

    internal static void DestroyAllWalls() {
        foreach (List<BinWall> l in wallCache.Values) {
            foreach (BinWall b in l) {
                UnityEngine.Object.Destroy(b);
            }
        }
        wallCache.Clear();
    }

    private static int radius = -1;
    private static int density = -1;

    static BinWallManager() {
        int shift = LayerMask.NameToLayer("Binning");
        binningLayerOnly = (1 << shift);
        ignoreBinningLayer = (1 << shift) ^ Physics.DefaultRaycastLayers; //-5 is default layermask, XOR with desired mask
        ReconfigureGazeOffsetCache(Default_Radius, Default_Density); //radius of 50 pixels, density of approx 1 raycast per 45 pixels
    }

    public static void DisplayGazes(List<Vector2> gazes, Camera c, GameObject binWallPrefab, BinMapper mapper) {
        IdentifyObjects(gazes, c, binWallPrefab, mapper);
        ViewBinGazes(gazes, c, binWallPrefab, mapper);
        gazes.Clear();
    }

    /// <summary>
    /// Raycasts a set area around the gaze point.
    /// </summary>
    /// <param name="gaze">Screen point cooridinates of the gaze</param>
    /// <param name="cam">View of the subject</param>
    /// <param name="sampleTime"> timestamp of the gazepoint</param>
    /// <param name="binsHitId">Hashset to contain the actual ids of the bins hit</param>
    /// <param name="dependency">Jobs to run this job after</param>
    /// <param name="modder">Number to mod the number of rays to be casted for near objects</param>
    /// <remarks> Size and density of the raycast area can be changed via <see cref="ReconfigureGazeOffsetCache(int, int)"/></remarks>
    public static BinGazeJobData BinGaze(Vector2 gaze, uint sampleTime, Camera cam, JobHandle dependency, int modder) {
        if (gaze.isNaN()) {
            return null;
        }

        int numRaycasts;

        if (modder > 1) {
            numRaycasts = secondaryOffset.Count / modder + 1;
        }
        else {
            numRaycasts = secondaryOffset.Count;
        }

        NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(numRaycasts, Allocator.TempJob);
        NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(numRaycasts, Allocator.TempJob);

        int counter = 0;
        int idx = 0;

        Profiler.BeginSample("Fill array");
        foreach (Vector2 offset in secondaryOffset) {
            if (counter % modder == 0) {
                Ray r = cam.ScreenPointToRay(gaze + offset);
                commands[idx] = new RaycastCommand(r.origin, r.direction, layerMask: binningLayerOnly);
                idx++;
            }
            counter++;
        }
        Profiler.EndSample();

        JobHandle h = RaycastCommand.ScheduleBatch(commands, results, 1, dependency);

        return new BinGazeJobData(h, commands, results, numRaycasts, sampleTime);
    }

    public class BinGazeJobData : IDisposable {
        public JobHandle h;
        public NativeArray<RaycastCommand> cmds;
        public NativeArray<RaycastHit> results;
        public int numRaycasts;
        public uint sampleTime;

        public BinGazeJobData(JobHandle h, NativeArray<RaycastCommand> cmds, NativeArray<RaycastHit> results, int numRaycasts, uint time) {
            this.h = h;
            this.cmds = cmds;
            this.results = results;
            this.numRaycasts = numRaycasts;
            this.sampleTime = time;
        }

        public void Dispose() {
            cmds.Dispose();
            results.Dispose();
        }

        public void process(BinMapper mapper, HashSet<int> binsHitId) {
            for (int i = 0; i < numRaycasts; i++) {
                Profiler.BeginSample("GetCollider");
                Collider c = results[i].collider;
                Profiler.EndSample();
                if (c != null) {
                    Profiler.BeginSample("GetComponent");
                    Bin b = c.GetComponent<Bin>();
                    Profiler.EndSample();

                    Profiler.BeginSample("HIT");
                    b.Hit();
                    Profiler.EndSample();

                    Profiler.BeginSample("Mapper");
                    int mappedId = mapper.MapBinToId(b.parent, b);
                    Profiler.EndSample();

                    if (binsHitId.Add(mappedId)) {
                        b.idText.text = mappedId.ToString();
                    }
                }
            }
        }
    }

    static Color transparentRed = new Color(1, 0, 0, 0.3f);
    static Color transGreen = new Color(0, 1, 0, 0.3f);

    public static void ViewBinGazes(IEnumerable<Vector2> gazes, Camera cam, GameObject binWallPrefab, BinMapper mapper) {
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
                    commands[counter] = new RaycastCommand(r.origin, r.direction, float.MaxValue, binningLayerOnly);
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

    public static float IdentifyObjects(List<Vector2> gazes, Camera cam, GameObject binWallPrefab, BinMapper mapper) {
        int numRaycasts = gazes.Count * primaryOffset.Count;

        NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(numRaycasts, Allocator.TempJob);
        NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(numRaycasts, Allocator.TempJob);

        int counter = 0;

        HashSet<Collider> hitPool = new HashSet<Collider>();

        float maxSqDist = -1;

        try {
            foreach (Vector2 gaze in gazes) {
                if (gaze.isNaN()) {
                    continue;
                }

                foreach (Vector2 offset in primaryOffset) {
                    Ray r = cam.ScreenPointToRay(gaze + offset);
                    commands[counter] = new RaycastCommand(r.origin, r.direction, float.MaxValue, ignoreBinningLayer);
                    counter++;
                }
            }

            JobHandle h = RaycastCommand.ScheduleBatch(commands, results, 1, default);
            h.Complete();

            for (int i = 0; i < counter; i++) {
                Collider c = results[i].collider;
                if (c != null) {
                    maxSqDist = Mathf.Max(maxSqDist, Vector3.SqrMagnitude(results[i].point - commands[i].from));
                    hitPool.Add(c);
                    Debug.DrawLine(commands[i].from, results[i].point, transGreen);
                }
            }
        }
        finally {
            commands.Dispose();
            results.Dispose();
        }
        foreach (Collider c in hitPool) {
            AssignBinwall(c.gameObject, binWallPrefab, mapper);
        }

        return maxSqDist;
    }

    public static void ReconfigureGazeOffsetCache(int radius, int density) {
        if (BinWallManager.radius != radius || BinWallManager.density != density) {
            BinWallManager.radius = radius;
            BinWallManager.density = density;

            gazeSqDistThreshold = Mathf.Pow(radius / 2f, 2);

            /*primary offset density is reduced as it is only used to identify all
              possible objects in the scene covered by the gaze */
            CreateOffsetList(radius, Mathf.Max(density / PRI_DENSITY_DIVISOR, MINIMUM_RAYCAST_DENSITY), primaryOffset);
            CreateOffsetList(radius, Mathf.Max(density, MINIMUM_RAYCAST_DENSITY), secondaryOffset);
        }
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
        numRays = Math.Max(numRays, 2); //minimum of 2 to prevent divide by 0 later

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

        BinWall binWall = GetAvailableBinWall(group, binWallPrefab, mapper);
        mapper.PlaceBinWall(group, obj, binWall);
    }

    public static void ResetManager() {
        maxSqDist = -1;
    }

    public static void ResetWalls() {
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

        if (mapper.IsSingleWallGroup(group) && activated.Contains(group.ToString())) {
            return null;
        }

        BinWallConfig config = mapper.MapObjectToBinWallConfig(group);
        int cache_id = mapper.MapGroupToWallCache(group);

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

        Profiler.BeginSample("PhysicsSimulation");
        Physics.SyncTransforms();
        Profiler.EndSample();

        return binWall;
    }
}
