using UnityEngine;

public class WrongRewardAreaError : MonoBehaviour
{
    //public CheckPoster checkPoster;
    public AudioClip errorClip;
    public RewardArea rewardArea;
    private static CueController cueController;
    private float timer = 100f;
    private float overallBlinkDuration = 0.5f;

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
        string cueImage = CueImage.cueImage;
        Debug.Log(cueImage);
        if (areaPosterImage != cueImage)
        {
            PlayerAudio.instance.PlayErrorClip();
            timer = 0f;
        }
    }

    private void HintBlink() //2 off/on cycles
    {
        if (timer >= 0 && timer < (overallBlinkDuration / 2))
        {
            cueController.HideHint();
        }
        if (timer >= (overallBlinkDuration / 2) && timer < (overallBlinkDuration))
        {
            cueController.ShowHint();
        }
        if (timer >= (overallBlinkDuration) && timer < (3 * overallBlinkDuration / 2))
        {
            cueController.HideHint();
        }
        if (timer >= (3 * overallBlinkDuration / 2) && timer < (2 * overallBlinkDuration))
        {
           cueController.ShowHint();
        }
        if (timer >= (2 * overallBlinkDuration) && timer < (5 * overallBlinkDuration / 2))
        {
            cueController.HideHint();
        }
        if (timer >= (5 * overallBlinkDuration / 2) && timer < (3 * overallBlinkDuration))
        {
            cueController.ShowHint();
        }
        if (timer >= (3 * overallBlinkDuration) && timer < (7 * overallBlinkDuration / 2))
        {
            cueController.HideHint();
        }
        if (timer >= (7 * overallBlinkDuration / 2) && timer < (4 * overallBlinkDuration))
        {
            cueController.ShowHint();
        }
    }
}
