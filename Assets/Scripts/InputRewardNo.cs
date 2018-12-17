using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class InputRewardNo : MonoBehaviour
{

    public static string inputrewardno;

    void Start()
    {
        var input = gameObject.GetComponent<InputField>();
        var se = new InputField.SubmitEvent();
        se.AddListener(SubmitName);
        input.onEndEdit = se;
    }

    private void SubmitName(string arg0)
    {
        Debug.Log("No. of rewards = " + arg0);
        inputrewardno = (arg0);
    }
}