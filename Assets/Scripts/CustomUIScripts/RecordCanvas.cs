using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordCanvas : MonoBehaviour
{
    [SerializeField]
    private Text trialNumText;

    [SerializeField]
    private Text frameNumText;

    [SerializeField]
    private Text trialStatusText;

    [SerializeField]
    private CanvasGroup canvasGroup;

    public void Hide() {
        canvasGroup.SetVisibility(false);
    }

    public void Show() {
        canvasGroup.SetVisibility(true);
    }

    public string TrialNum { set => trialNumText.text = value; }
    public string FrameNum { set => frameNumText.text = value; }
    public string TrialStatus { set => trialStatusText.text = value; }

}
