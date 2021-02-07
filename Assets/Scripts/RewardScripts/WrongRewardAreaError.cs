using UnityEngine;

public class WrongRewardAreaError : MonoBehaviour
{
    //public CheckPoster checkPoster;
    public AudioClip errorClip;
    public RewardArea rewardArea;
    private static CueController cueController;
    private float timer = 100f;
    private float overallBlinkDuration = 1f;

    void Start()
    {
        cueController = GameObject.FindObjectOfType(typeof(CueController)) as CueController;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        HintBlink();
    }

    private void OnTriggerEnter(Collider other)
    {
        string areaPosterImage = rewardArea.cueImage.name;
        //Debug.Log(areaPosterImage);
        string cueImage = NonTargetRaycast.cueImage;
        //Debug.Log(cueImage);
        if (areaPosterImage != cueImage)
        {
            PlayerAudio.instance.PlayErrorClip();
            timer = 0f;
        }
    }

    private void HintBlink() //2 off/on cycles
    {
        if (timer >= 0 && timer < (overallBlinkDuration / 4))
        {
            cueController.HideHint();
        }
        if (timer >= (overallBlinkDuration / 4) && timer < (overallBlinkDuration / 2))
        {
            cueController.ShowHint();
            Debug.Log("Test2");
        }
        if (timer >= (overallBlinkDuration / 2) && timer < (3 * overallBlinkDuration / 4))
        {
            cueController.HideHint();
        }
        if (timer >= (3 * overallBlinkDuration / 4) && timer < overallBlinkDuration)
        {
           cueController.ShowHint();
        }
    }
}
