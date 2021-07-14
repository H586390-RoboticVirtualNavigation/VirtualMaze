using UnityEngine;
using System.Collections;

public class CueImage : MonoBehaviour
{
    public CheckPoster checkPoster;
    public static string cueImage { get; private set; }

    void Update()
    {
        if (LevelController.sessionStarted)
        {
            if (checkPoster == null) {
                Debug.Log("Poster is null");
            }
            cueImage = checkPoster.GetCueImageName();
        }
    }
}
