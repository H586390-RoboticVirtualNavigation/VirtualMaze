using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FileSelector : InputField {
    public FileBrowser fb;
    public Button browseBtn;

    public string defaultPath;

    public OnPathSelectedEvent OnPathSelected = new OnPathSelectedEvent();

    protected override void Awake() {
        browseBtn.onClick.AddListener(OnBrowseButtonClicked);
        onEndEdit.AddListener(OnEndEditPath);
        defaultPath = Application.dataPath;
    }

    private void OnBrowseButtonClicked() {
        fb.OnFileBrowserExit += OnBrowserExit;
        fb.TryShow(text, defaultPath);
    }

    private void OnEndEditPath(string text) {
        OnPathSelected.Invoke(text);
    }

    private void OnBrowserExit(string path) {
        fb.OnFileBrowserExit -= OnBrowserExit;

        if (!string.IsNullOrEmpty(path)) {
            text = path;
            OnPathSelected.Invoke(path);
        }
    }

    [Serializable]
    public class OnPathSelectedEvent : UnityEvent<string> { }
}
