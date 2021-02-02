using UnityEngine;

public class NonTargetRaycast : MonoBehaviour
{
    public Camera cam;
    public CheckPoster checkPoster;
    public AudioClip errorClip;
    public LevelController levelController;
    private bool isSoundTriggered = false;
    private bool isPosterInView = false;
    private float sweepValue;
    private bool FlagLeft;
    private bool FlagRight;
    private bool FlagStraight;
    private bool Flag1;
    private bool Flag2;
    private bool Flag3;
    private bool Flag4;
    private bool Flag5;
    private bool Flag6;
    private bool Flag7;
    private bool Flag8;
    private bool Flag9;
    private bool Flag10;
    private bool Flag11;
    private bool Flag12;
    private bool Flag13;
    private bool Flag14;
    private bool Flag15;
    private bool Flag16;
    private bool Flag17;
    private bool Flag18;
    
    void Update()
    {
        //For Testing:
        /*if (Input.GetKeyDown("space"))
        {
            Shoot();
        }*/

        /*for (sweepValue = -90; sweepValue <= 90; sweepValue += 10)
        {
            Shoot();
        }*/

        Shoot();

        void Shoot()
        {
            RaycastHit hitstraight;
            RaycastHit hitleft;
            RaycastHit hitright;
            RaycastHit checkleft;
            RaycastHit checkright;
            //Vector3 straightline = cam.transform.forward;
            Vector3 straightline = Quaternion.AngleAxis(sweepValue / 2f, Vector3.up) * cam.transform.forward;
            Vector3 leftline = Quaternion.AngleAxis(-97f / 2f, Vector3.up) * cam.transform.forward;
            Vector3 rightline = Quaternion.AngleAxis(97f / 2f, Vector3.up) * cam.transform.forward;
            Vector3 checkleftline = Quaternion.AngleAxis(-102f / 2f, Vector3.up) * cam.transform.forward;
            Vector3 checkrightline = Quaternion.AngleAxis(102f / 2f, Vector3.up) * cam.transform.forward;
            straightline.y = 0;
            leftline.y = 0;
            rightline.y = 0;
            checkleftline.y = 0;
            checkrightline.y = 0;
            //Debug.Log(straightline);
            //Debug.Log(isPosterInView);
            if (Physics.Raycast(cam.transform.position, checkleftline, out checkleft, 500))
            {
                Debug.DrawLine(cam.transform.position, checkleft.point, Color.green);
                if (checkleft.transform.name == "Poster")
                {
                    isPosterInView = false;
                }
            }

            if (Physics.Raycast(cam.transform.position, checkrightline, out checkright, 500))
            {
                Debug.DrawLine(cam.transform.position, checkright.point, Color.green);
                if (checkright.transform.name == "Poster")
                {
                    isPosterInView = false;
                }
            }

            if (Physics.Raycast(cam.transform.position, leftline, out hitleft, 500))
            {
                Debug.DrawLine(cam.transform.position, hitleft.point);
                //Debug.Log(hitleft.transform.name);
                if (hitleft.transform.name == "Poster")
                {
                    FlagLeft = true;
                    string posterImage = hitleft.transform.GetComponent<Renderer>().material.name;
                    //Debug.Log(posterImage);
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    //Debug.Log(strcheck);
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    FlagLeft = false;
                }
            }

            if (Physics.Raycast(cam.transform.position, rightline, out hitright, 500))
            {
                Debug.DrawLine(cam.transform.position, hitright.point);
                //Debug.Log(hitleft.transform.name);
                if (hitright.transform.name == "Poster")
                {
                    FlagRight = true;
                    string posterImage = hitright.transform.GetComponent<Renderer>().material.name;
                    //Debug.Log(posterImage);
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    //Debug.Log(strcheck);
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    FlagRight = false;
                }
            }

            if (Physics.Raycast(cam.transform.position, straightline, out hitstraight, 500))
            {
                //Debug.DrawLine(cam.transform.position, hitstraight.point);
                //Debug.Log(hitstraight.transform.name);
                if (hitstraight.transform.name == "Poster")
                {
                    FlagStraight = true;
                    string posterImage = hitstraight.transform.GetComponent<Renderer>().material.name;
                    //Debug.Log(posterImage);
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    //Debug.Log(strcheck);
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    FlagStraight = false;
                }
            }

            RaycastHit hit1;
            Vector3 line1 = Quaternion.AngleAxis(-87f / 2f, Vector3.up) * cam.transform.forward;
            line1.y = 0;
            if (Physics.Raycast(cam.transform.position, line1, out hit1, 500))
            {
                Debug.DrawLine(cam.transform.position, hit1.point);
                if (hit1.transform.name == "Poster")
                {
                    Flag1 = true;
                    string posterImage = hit1.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag1 = false;
                }
            }

            RaycastHit hit2;
            Vector3 line2 = Quaternion.AngleAxis(-77f / 2f, Vector3.up) * cam.transform.forward;
            line2.y = 0;
            if (Physics.Raycast(cam.transform.position, line2, out hit2, 500))
            {
                Debug.DrawLine(cam.transform.position, hit2.point);
                if (hit2.transform.name == "Poster")
                {
                    Flag2 = true;
                    string posterImage = hit2.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag2 = false;
                }
            }

            RaycastHit hit3;
            Vector3 line3 = Quaternion.AngleAxis(-67f / 2f, Vector3.up) * cam.transform.forward;
            line3.y = 0;
            if (Physics.Raycast(cam.transform.position, line3, out hit3, 500))
            {
                Debug.DrawLine(cam.transform.position, hit3.point);
                if (hit3.transform.name == "Poster")
                {
                    Flag3 = true;
                    string posterImage = hit3.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag3 = false;
                }
            }

            RaycastHit hit4;
            Vector3 line4 = Quaternion.AngleAxis(-57f / 2f, Vector3.up) * cam.transform.forward;
            line4.y = 0;
            if (Physics.Raycast(cam.transform.position, line4, out hit4, 500))
            {
                //Debug.DrawLine(cam.transform.position, hit4.point);
                if (hit4.transform.name == "Poster")
                {
                    Flag4 = true;
                    string posterImage = hit4.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag4 = false;
                }
            }

            RaycastHit hit5;
            Vector3 line5 = Quaternion.AngleAxis(-47f / 2f, Vector3.up) * cam.transform.forward;
            line5.y = 0;
            if (Physics.Raycast(cam.transform.position, line5, out hit5, 500))
            {
                //Debug.DrawLine(cam.transform.position, hit5.point);
                if (hit5.transform.name == "Poster")
                {
                    Flag5 = true;
                    string posterImage = hit5.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag5 = false;
                }
            }

            RaycastHit hit6;
            Vector3 line6 = Quaternion.AngleAxis(-37f / 2f, Vector3.up) * cam.transform.forward;
            line6.y = 0;
            if (Physics.Raycast(cam.transform.position, line6, out hit6, 500))
            {
                //Debug.DrawLine(cam.transform.position, hit6.point);
                if (hit6.transform.name == "Poster")
                {
                    Flag6 = true;
                    string posterImage = hit6.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag6 = false;
                }
            }

            RaycastHit hit7;
            Vector3 line7 = Quaternion.AngleAxis(-27f / 2f, Vector3.up) * cam.transform.forward;
            line7.y = 0;
            if (Physics.Raycast(cam.transform.position, line7, out hit7, 500))
            {
                //Debug.DrawLine(cam.transform.position, hit7.point);
                if (hit7.transform.name == "Poster")
                {
                    Flag7 = true;
                    string posterImage = hit7.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag7 = false;
                }
            }

            RaycastHit hit8;
            Vector3 line8 = Quaternion.AngleAxis(-17f / 2f, Vector3.up) * cam.transform.forward;
            line8.y = 0;
            if (Physics.Raycast(cam.transform.position, line8, out hit8, 500))
            {
                //Debug.DrawLine(cam.transform.position, hit8.point);
                if (hit8.transform.name == "Poster")
                {
                    Flag8 = true;
                    string posterImage = hit8.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag8 = false;
                }
            }

            RaycastHit hit9;
            Vector3 line9 = Quaternion.AngleAxis(-7f / 2f, Vector3.up) * cam.transform.forward;
            line9.y = 0;
            if (Physics.Raycast(cam.transform.position, line9, out hit9, 500))
            {
                //Debug.DrawLine(cam.transform.position, hit9.point);
                if (hit9.transform.name == "Poster")
                {
                    Flag9 = true;
                    string posterImage = hit9.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag9 = false;
                }
            }

            RaycastHit hit10;
            Vector3 line10 = Quaternion.AngleAxis(7f / 2f, Vector3.up) * cam.transform.forward;
            line10.y = 0;
            if (Physics.Raycast(cam.transform.position, line10, out hit10, 500))
            {
                //Debug.DrawLine(cam.transform.position, hit10.point);
                if (hit10.transform.name == "Poster")
                {
                    Flag10 = true;
                    string posterImage = hit10.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag10 = false;
                }
            }

            RaycastHit hit11;
            Vector3 line11 = Quaternion.AngleAxis(17f / 2f, Vector3.up) * cam.transform.forward;
            line11.y = 0;
            if (Physics.Raycast(cam.transform.position, line11, out hit11, 500))
            {
                //Debug.DrawLine(cam.transform.position, hit11.point);
                if (hit11.transform.name == "Poster")
                {
                    Flag11 = true;
                    string posterImage = hit11.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag11 = false;
                }
            }

            RaycastHit hit12;
            Vector3 line12 = Quaternion.AngleAxis(27f / 2f, Vector3.up) * cam.transform.forward;
            line12.y = 0;
            if (Physics.Raycast(cam.transform.position, line12, out hit12, 500))
            {
                //Debug.DrawLine(cam.transform.position, hit12.point);
                if (hit12.transform.name == "Poster")
                {
                    Flag12 = true;
                    string posterImage = hit12.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag12 = false;
                }
            }

            RaycastHit hit13;
            Vector3 line13 = Quaternion.AngleAxis(37f / 2f, Vector3.up) * cam.transform.forward;
            line13.y = 0;
            if (Physics.Raycast(cam.transform.position, line13, out hit13, 500))
            {
                //Debug.DrawLine(cam.transform.position, hit13.point);
                if (hit13.transform.name == "Poster")
                {
                    Flag13 = true;
                    string posterImage = hit13.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag13 = false;
                }
            }

            RaycastHit hit14;
            Vector3 line14 = Quaternion.AngleAxis(47f / 2f, Vector3.up) * cam.transform.forward;
            line14.y = 0;
            if (Physics.Raycast(cam.transform.position, line14, out hit14, 500))
            {
                //Debug.DrawLine(cam.transform.position, hit14.point);
                if (hit14.transform.name == "Poster")
                {
                    Flag14 = true;
                    string posterImage = hit14.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag14 = false;
                }
            }

            RaycastHit hit15;
            Vector3 line15 = Quaternion.AngleAxis(57f / 2f, Vector3.up) * cam.transform.forward;
            line15.y = 0;
            if (Physics.Raycast(cam.transform.position, line15, out hit15, 500))
            {
                //Debug.DrawLine(cam.transform.position, hit15.point);
                if (hit15.transform.name == "Poster")
                {
                    Flag15 = true;
                    string posterImage = hit15.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag15 = false;
                }
            }

            RaycastHit hit16;
            Vector3 line16 = Quaternion.AngleAxis(67f / 2f, Vector3.up) * cam.transform.forward;
            line16.y = 0;
            if (Physics.Raycast(cam.transform.position, line16, out hit16, 500))
            {
                Debug.DrawLine(cam.transform.position, hit16.point);
                if (hit16.transform.name == "Poster")
                {
                    Flag16 = true;
                    string posterImage = hit16.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag16 = false;
                }
            }

            RaycastHit hit17;
            Vector3 line17 = Quaternion.AngleAxis(77f / 2f, Vector3.up) * cam.transform.forward;
            line17.y = 0;
            if (Physics.Raycast(cam.transform.position, line17, out hit17, 500))
            {
                Debug.DrawLine(cam.transform.position, hit17.point);
                if (hit17.transform.name == "Poster")
                {
                    Flag17 = true;
                    string posterImage = hit17.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag17 = false;
                }
            }

            RaycastHit hit18;
            Vector3 line18 = Quaternion.AngleAxis(87f / 2f, Vector3.up) * cam.transform.forward;
            line18.y = 0;
            if (Physics.Raycast(cam.transform.position, line18, out hit18, 500))
            {
                Debug.DrawLine(cam.transform.position, hit18.point);
                if (hit18.transform.name == "Poster")
                {
                    Flag18 = true;
                    string posterImage = hit18.transform.GetComponent<Renderer>().material.name;
                    string cueImage = checkPoster.IsPosterSame();
                    string strcheck = cueImage + " (Instance)";
                    if (posterImage == strcheck)
                    {
                        //Debug.Log("Correct Poster");
                    }
                    else if (posterImage != strcheck && isSoundTriggered == false)
                    {
                        //Debug.Log("Wrong Poster");
                        PlayerAudio.instance.PlayErrorClip();
                        isSoundTriggered = true;
                    }
                }
                else
                {
                    Flag18 = false;
                }
            }

            if (FlagLeft || FlagRight || FlagStraight || Flag1 || Flag2 || Flag3 || Flag4 || Flag5 || Flag6 || Flag7 || Flag8 || Flag9 || Flag10 || Flag11 || Flag12 || Flag13 || Flag14 || Flag15 || Flag16 || Flag17 || Flag18)
            {
                isPosterInView = true;
            }
            else if (!FlagLeft && !FlagRight && !FlagStraight && !Flag1 && !Flag2 && !Flag3 && !Flag4 && !Flag5 && !Flag6 && !Flag7 && !Flag8 && !Flag9 && !Flag10 && !Flag11 && !Flag12 && !Flag13 && !Flag14 && !Flag15 && !Flag16 && !Flag17 && !Flag18)
            {
                isPosterInView = false;
            }

            if (isPosterInView == false)
            {
                isSoundTriggered = false;
            }
        }
    }
}