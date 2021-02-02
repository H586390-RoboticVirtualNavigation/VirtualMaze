using UnityEngine;
using UnityEngine.UI;

public class CheckPoster : MonoBehaviour
{
    public string IsPosterSame()
    {
        GameObject cueImage = GameObject.Find("cueImage");
        //Debug.Log(gameObject.GetComponent<Image>().sprite.name);
        return gameObject.GetComponent<Image>().sprite.name;
    }
}