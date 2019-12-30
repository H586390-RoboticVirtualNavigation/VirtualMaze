using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class InterfaceTest : MonoBehaviour, IDisposable {
    int layerMaskRing = 0, layerMaskArea = 0;

    public float radius = 1f;
    public int density = 150000;

    [SerializeField, Range(1, 100)]
    private int mod = 15;

    [Range(-1, 1)]
    public float pow = 0.5f;

    private List<Vector3> rayOffsetPrelim = new List<Vector3>();
    private List<Vector3> rayOffsetArea = new List<Vector3>();
    private HashSet<Collider> hitPool = new HashSet<Collider>();

    public GameObject binWallPrefab;

    private void Start() {
        int shift = LayerMask.NameToLayer("Binning");
        layerMaskArea = (1 << shift);
        layerMaskRing = (1 << shift) ^ -5; //-5 is default layermask

        //init offset for preliminary raycast
        CreateOffsetList(radius, density / 100, rayOffsetPrelim);

        //init offset for actual raycast
        CreateOffsetList(radius, density, rayOffsetArea);

        print(rayOffsetPrelim.Count);
        print(rayOffsetArea.Count);
    }

    private void Update() {
        BinWallManager.Reset();
        hitPool.Clear();
        RingCast();

        AreaCast(mod);

        LogBinWalls();
    }

    void LogBinWalls() {
        foreach (BinWall w in BinWall.WallsHit) {
            foreach (Bin b in w.binsHit) {
                b.idText.text = mapper.MapBinToId(w, b).ToString();
                b.SetTextCanvasActive(true);
            }
        }
    }
    void AreaCast(int divider = 1) {
        int numRaysArea = rayOffsetArea.Count / divider;

        NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(numRaysArea, Allocator.TempJob);
        NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(numRaysArea, Allocator.TempJob);

        Vector3 dir;
        Vector3 forward = transform.forward;
        Vector3 position = transform.position;

        float xRot = transform.localRotation.x;
        float yRot = transform.rotation.eulerAngles.y;

        int idx = 0;

        for (int i = 0; i < rayOffsetArea.Count; i++) {
            if (i % divider == 0) {
                dir = forward + Quaternion.Euler(xRot, yRot, 0) * rayOffsetArea[i];
                commands[idx] = new RaycastCommand(position, dir, float.MaxValue, layerMaskArea);
                Debug.DrawRay(transform.position, dir, Color.Lerp(Color.red, Color.blue, i / numRaysArea), 5);
                idx++;
            }
            if (idx == numRaysArea) {
                break;
            }
        }

        using (results)
        using (commands) {
            JobHandle h = RaycastCommand.ScheduleBatch(commands, results, 1, default);

            h.Complete();

            for (int i = 0; i < numRaysArea; i++) {
                Collider c = results[i].collider;
                if (c != null) {
                    hitPool.Add(c);
                    Bin b = c.GetComponent<Bin>();
                    b.Hit();
                    //Debug.DrawLine(transform.position, results[i].point, Color.yellow, 1);
                }
            }
        }
    }

    DoubleTeeBinMapper mapper = new DoubleTeeBinMapper();

    private void RingCast() {
        NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(rayOffsetPrelim.Count, Allocator.TempJob);
        NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(rayOffsetPrelim.Count, Allocator.TempJob);

        Vector3 dir;
        Vector3 forward = transform.forward;
        Vector3 position = transform.position;

        float xRot = transform.localRotation.x;
        float yRot = transform.rotation.eulerAngles.y;

        for (int i = 0; i < rayOffsetPrelim.Count; i++) {
            dir = forward + Quaternion.Euler(xRot, yRot, 0) * rayOffsetPrelim[i];
            commands[i] = new RaycastCommand(position, dir, float.MaxValue, layerMaskRing);
        }

        float min = float.PositiveInfinity;
        float max = float.NegativeInfinity;

        using (results)
        using (commands) {
            JobHandle h = RaycastCommand.ScheduleBatch(commands, results, 1, default);

            h.Complete();

            for (int i = 0; i < rayOffsetPrelim.Count; i++) {
                Collider c = results[i].collider;
                if (c != null) {
                    hitPool.Add(c);
                    //Debug.DrawLine(transform.position, results[i].point, Color.blue);
                    float distSqr = (results[i].point - position).sqrMagnitude;

                    min = Mathf.Min(min, distSqr);
                    max = Mathf.Max(max, distSqr);
                }
            }
        }

        foreach (Collider c in hitPool) {
            BinWallManager.BinObject(c.gameObject, binWallPrefab, mapper);
        }

        Debug.LogWarning($"min: {min}, max: {max}");
    }

    public void Dispose() {
        throw new NotImplementedException();
    }

    //golden ratio of degrees to offset rays by
    const float turnFraction = 0.618f * 2 * Mathf.PI;

    /// <summary>
    /// Clears and fills the given list with a spread of evenly spaced Direction vectors
    /// </summary>
    /// <param name="radius">Radius of the spread</param>
    /// <param name="density">Density of the vectors per unit area</param>
    /// <param name="toFill">List to fill</param>
    void CreateOffsetList(float radius, int density, List<Vector3> toFill) {
        toFill.Clear();

        //Calculate number of Rays
        int numRays = Mathf.CeilToInt(Mathf.Pow(radius, 2) * Mathf.PI * density);

        for (int i = 0; i < numRays; i++) {
            //sqrt to reduce clustering at the center
            float dst = Mathf.Sqrt(i / (numRays - 1f)) * radius;
            float angle = turnFraction * i;

            float x = dst * Mathf.Cos(angle);
            float y = dst * Mathf.Sin(angle);

            toFill.Add(new Vector3(x, y));
        }
    }
}
