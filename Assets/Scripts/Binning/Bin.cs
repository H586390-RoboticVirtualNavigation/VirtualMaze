using UnityEngine;
using UnityEngine.UI;

public class Bin : MonoBehaviour {
    [SerializeField]
    private int _id = -9999;

    public Text idText;
    public GameObject textCanvas = null;

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

    [SerializeField]
    private int manualGroup = -1;


    public string Owner { get => parent.owner; }

    //use group set in the scene if parent does not exist
    public int Group { get => parent == null ? manualGroup : parent.group; }

    public delegate void OnBinHitEvent(Bin hit);

    public event OnBinHitEvent OnBinHit;

    public void Init(int id, BinWall wall) {
        parent = wall;
        this.id = id;
    }

    public void Hit() {
        OnBinHit?.Invoke(this);
    }

    internal void SetTextCanvasActive(bool v) {
        if (textCanvas != null) {
            textCanvas.SetActive(v);
        }
    }
}
