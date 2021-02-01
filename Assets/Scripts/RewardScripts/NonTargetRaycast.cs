using UnityEngine;

public class NonTargetRaycast : MonoBehaviour
{
    public Camera cam;
    public CheckPoster checkPoster;
    public AudioClip errorClip;
    public LevelController levelController;
    private bool isSoundTriggered = false;

    void Update()
    {
        //For Testing:
        /*if (Input.GetKeyDown("space"))
        {
            Shoot();
        }*/

        Shoot();

        void Shoot()
        {
            RaycastHit hit;
            RaycastHit hitleft;
            RaycastHit hitright;
            Vector3 straightline = cam.transform.forward;
            Vector3 leftline = Quaternion.AngleAxis(-110f / 2f, Vector3.up) * cam.transform.forward;
            Vector3 rightline = Quaternion.AngleAxis(110f / 2f, Vector3.up) * cam.transform.forward;
            straightline.y = 0;
            leftline.y = 0;
            rightline.y = 0;
            //Debug.Log(straightline);
            if (Physics.Raycast(cam.transform.position, leftline, out hitleft, 500))
            {
                //Debug.DrawLine(cam.transform.position, hitleft.point);
                //Debug.Log(hitleft.transform.name);
                if (hitleft.transform.name == "Poster")
                {
                    string posterImage = hitleft.transform.GetComponent<Renderer>().material.name;
                    //Debug.Log(posterImage);
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    //Debug.Log(strcheck);
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                        isSoundTriggered = false;
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
            }

            if (Physics.Raycast(cam.transform.position, rightline, out hitright, 500))
            {
                //Debug.DrawLine(cam.transform.position, hitright.point);
                //Debug.Log(hitleft.transform.name);
                if (hitright.transform.name == "Poster")
                {
                    string posterImage = hitright.transform.GetComponent<Renderer>().material.name;
                    //Debug.Log(posterImage);
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    //Debug.Log(strcheck);
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                        isSoundTriggered = false;
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
            }

            if (Physics.Raycast(cam.transform.position, straightline, out hit, 500))
            {
                //Debug.DrawLine(cam.transform.position, hit.point);
                //Debug.Log(hit.transform.name);
                if (hit.transform.name == "Poster")
                {
                    string posterImage = hit.transform.GetComponent<Renderer>().material.name;
                    //Debug.Log(posterImage);
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    //Debug.Log(strcheck);
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                        isSoundTriggered = false;
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else if (hit.transform.name != "Poster")
                {
                    //isSoundTriggered = false;
                }
            }
        }
    }
}