using UnityEngine;
using System.Collections;

public class CueImage : MonoBehaviour
{
    public CheckPoster checkPoster;
    public static string cueImage { get; private set; }

    void Update()
    {
        cueImage = checkPoster.GetCueImageName();
    }
}
