using UnityEngine;

public class WrongRewardAreaError : MonoBehaviour
{
    //public CheckPoster checkPoster;
    public AudioClip errorClip;
    public RewardArea rewardArea;
    private static CueController cueController;
    private float timer = 1000f;

    // Number and duration of blinks
    int numBlinks = 4;
    private float overallBlinkDuration = 0.5f;

    void Start()
    {
        cueController = GameObject.FindObjectOfType(typeof(CueController)) as CueController;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        HintBlink();
        if (!LevelController.sessionStarted)
        {
            Reset();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (LevelController.sessionStarted)
        {
            string areaPosterImage = rewardArea.cueImage.name;
            //Debug.Log(areaPosterImage);
            string cueImage = CueImage.cueImage;
            //Debug.Log(cueImage);
            if (areaPosterImage != cueImage)
            {
                PlayerAudio.instance.PlayErrorClip();
                timer = 0f;
            }
        }
    }

    private void HintBlink()
    {
        for (int i = 0; i < numBlinks; i++)
        {
            if (timer >= (i * overallBlinkDuration) && timer < (((2 * i) + 1) * overallBlinkDuration / 2))
            {
                cueController.HideHint();
            }
            if (timer >= (((2 * i) + 1) * overallBlinkDuration / 2) && timer < ((i + 1) * overallBlinkDuration))
            {
                cueController.ShowHint();
            }
        }
    }

    private void Reset()
    {
        timer = 1000f;
    }
}
