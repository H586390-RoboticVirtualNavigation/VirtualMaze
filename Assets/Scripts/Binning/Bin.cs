using UnityEngine;
using UnityEngine.UI;

public class Bin : MonoBehaviour {
    [SerializeField]
    private int hitNum = 0;

    [SerializeField]
    private int _id = -9999;

    public Text idText;
    public GameObject textCanvas;

    public int id {
        get => _id;
        private set {
            _id = value;
            if (idText != null && parent != null) {
                idText.text = _id.ToString();
            }
        }
    }
    public BinWall parent { get; private set; }

    public string owner { get => parent.owner; }
    public int group { get => parent.group; }

    public delegate void OnBinHitEvent(Bin hit);

    public event OnBinHitEvent OnBinHit;

    public void Init(int id, BinWall wall) {
        parent = wall;
        this.id = id;
    }

    public void Hit() {
        OnBinHit.Invoke(this);
        hitNum++;
    }

    internal void SetTextCanvasActive(bool v) {
        textCanvas.SetActive(v);
    }
}
