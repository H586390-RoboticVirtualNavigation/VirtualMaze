using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;

/// <summary>
/// Notifies the listeners when Filebrower hides itself
/// </summary>
/// <param name="path">Returns null if canceled, else returns path of selected directory </param>
public delegate void OnFileBrowserExitEvent(string path);

public class FileBrowser : MonoBehaviour {

    // user of FileBrowser will suscribe his methods to this event
    public event OnFileBrowserExitEvent OnFileBrowserExit;

    //drag from editor
    public DirectoryContents directoryContents;
    public InputField filePath;
    public CanvasGroup canvasGroup;

    void Awake() {
        Hide();
    }

    private void Exit(string path) {
        Hide();
        OnFileBrowserExit.Invoke(path);
    }

    public void Show(string initalPath) {
        InitWithDirectory(initalPath);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void Hide() {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnChoose() {
        Exit(filePath.text);
    }

    public void OnCancel() {
        Exit(null);
    }

    public void OnUp() {
        GoUpADirectory();
    }

    public void OnNewFolder() {

    }

    public void OnNewFile() {

    }

    private void InitWithDirectory(string path) {
        directoryContents.InitWithContentsOfDirectory(path, OnFileClicked);
        filePath.text = directoryContents.currentDirectory.FullName;
    }

    public void GoToDirectory(string path) {
        directoryContents.GetContentsOfDirectory(path);
        filePath.text = directoryContents.currentDirectory.FullName;
    }

    public void GoUpADirectory() {
        directoryContents.GetContentsOfDirectory(directoryContents.currentDirectory.Parent.FullName);
        filePath.text = directoryContents.currentDirectory.FullName;
    }

    void OnFileClicked(FileItem item, bool wasDoubleClick) {

        //double click
        if (wasDoubleClick) {
            //folder
            if (item.isFolder) {
                GoToDirectory(item.directoryInfo.FullName);
                filePath.text = item.directoryInfo.FullName;
            }
            //file
            else {
                filePath.text = item.fileInfo.FullName;
            }
        }

        //single click
        else {
            //folder
            if (item.isFolder) {
                filePath.text = item.directoryInfo.FullName;
            }
            //file
            else {
                filePath.text = item.fileInfo.FullName;
            }
        }
    }
}
