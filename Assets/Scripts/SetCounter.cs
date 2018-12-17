using UnityEngine;
using UnityEngine.UI;


public class SetCounter : MonoBehaviour {

    private int counter;
    public Text countText;

    //void OnEnable()
    //{
    //    EventManager.StartListening("Begin", addCount);
    //}

    //void OnDisable()
    //{
    //    EventManager.StopListening("Begin", addCount);
    //}

    // Use this for initialization
    void Start () {
        EventManager.StartListening("Reward", addCount);
        counter = 0;
        SetCountText();
    }

    // Update is called once per frame
    void Update () {
        //counter = BasicLevelController.count;
        //EventManager.StartListening("Begin", addCount);
        SetCountText();
    }

    void addCount () {
        counter = counter + 1;
    }

    void SetCountText() {
        countText.text = "Count: " + counter.ToString();
    }
}
