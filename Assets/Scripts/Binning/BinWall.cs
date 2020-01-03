using System.Collections.Generic;
using UnityEngine;

public class BinWall : MonoBehaviour {
    [SerializeField]
    private GameObject binPrefab = null;

    [SerializeField]
    private string _owner;
    public string owner { get => _owner; private set => _owner = value; }

    [SerializeField]
    private int _group;
    public int group { get => _group; private set => _group = value; }

    private List<Bin> binList = new List<Bin>();
    public HashSet<Bin> binsHit = new HashSet<Bin>();

    public static HashSet<BinWall> WallsHit = new HashSet<BinWall>();


    public int numWidth { get; private set; }
    public int numHeight { get; private set; }

    private void OnDrawGizmos() {
        Gizmos.DrawSphere(transform.position, 0.5f);
    }

    private void Start() {
        //3/5
        //CreateWall(3f / 5f, 25 / 40f, 3f, 5);
    }

    public void CreateWall(BinWallConfig config) {
        CreateWall(config.binHeight, config.binWidth, config.fillHeight, config.fillWidth);
    }

    public void Reset() {
        //owner = null;
        //group = -1;

        foreach (Bin b in binsHit) {
            b.SetTextCanvasActive(false);
        }

        binsHit.Clear();
        WallsHit.Clear();
    }

    public void CreateWall(float binHeight, float binWidth, float fillHeight, float fillWidth) {
        numWidth = Mathf.FloorToInt(fillWidth / binWidth);
        numHeight = Mathf.FloorToInt(fillHeight / binHeight);

        float widthEdgeOffset = binWidth / 2;
        float heightEdgeOffset = binHeight / 2f;

        float widthCenterOffset = fillWidth / 2f;
        float heightCenterOffset = fillHeight / 2f;

        int id = 0;

        for (int h = 0; h < numHeight; h++) {
            for (int w = 0; w < numWidth; w++) {
                GameObject obj = Instantiate(binPrefab, new Vector3(w * binWidth - widthCenterOffset + widthEdgeOffset,
                    -h * binHeight + heightCenterOffset - heightEdgeOffset, 0), Quaternion.identity);

                obj.transform.SetParent(transform, false);
                obj.transform.localScale = new Vector3(binWidth, binHeight, 1);

                Bin b = obj.GetComponent<Bin>();
                b.Init(id, this);
                b.OnBinHit += RegisterBinHit;
                id++;
                binList.Add(b);
            }
        }
    }

    //moves wall to collider
    public void AttachTo(Location c, string owner, int group) {
        transform.SetPositionAndRotation(c.position, c.rotation);
        gameObject.SetActive(true);
        this.owner = owner;
        this.group = group;
    }

    private void RegisterBinHit(Bin hit) {
        binsHit.Add(hit);
        WallsHit.Add(this);
    }
}

public struct BinWallConfig {
    public float binWidth;
    public float binHeight;
    public float fillWidth;
    public float fillHeight;

    public BinWallConfig(float binWidth, float binHeight, float fillWidth, float fillHeight) {
        this.binWidth = binWidth;
        this.binHeight = binHeight;
        this.fillWidth = fillWidth;
        this.fillHeight = fillHeight;
    }
}