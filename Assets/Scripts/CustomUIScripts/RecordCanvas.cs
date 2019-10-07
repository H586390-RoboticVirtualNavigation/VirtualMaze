using System;
using UnityEngine;
using UnityEngine.UI;

public class RecordCanvas : MonoBehaviour {
    [SerializeField]
    private Text trialNumText = null;

    [SerializeField]
    private Text frameNumText = null;

    [SerializeField]
    private Text trialStatusText = null;

    [SerializeField]
    private CanvasGroup canvasGroup = null;

    [SerializeField]
    private Text timeDisplayText = null;

    public void Hide() {
        canvasGroup.SetVisibility(false);
    }

    public void Show() {
        canvasGroup.SetVisibility(true);
    }

    public string TrialNum { set => trialNumText.text = value; }
    public string FrameNum { set => frameNumText.text = value; }
    public string TrialStatus { set => trialStatusText.text = value; }
    public uint TimeStatus {
        set {
            timeDisplayText.text = TimeSpan.FromMilliseconds(value).ToString(@"hh\:mm\:ss\:fff");
        }
    }
}
