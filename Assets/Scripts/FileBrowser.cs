﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using System;

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

    private string prevDir;

    void Awake() {
        HideCanvas();
        FileItem.FileClickedEvent += OnFileClicked;
    }

    private void Exit(string path) {
        HideCanvas();
        OnFileBrowserExit.Invoke(path);
    }

    public void Show(string initalPath) {
        InitWithDirectory(initalPath);
        DisplayCanvas();
    }

    public bool TryShow(string initialPath, string defaultPath) {
        if (File.Exists(initialPath)) {
            Show(Path.GetDirectoryName(initialPath));
            return true;
        }
        else if (Directory.Exists(initialPath)) {
            Show(initialPath);
            return true;
        }
        else {
            Show(defaultPath);
            return false;
        }
    }

    public static string AddPathSeperator(string path) {
        print(path);
        string dir = path;
        if (!string.IsNullOrEmpty(path) && !(path.EndsWith($"{Path.DirectorySeparatorChar}") || path.EndsWith($"{Path.AltDirectorySeparatorChar}"))) {
            dir = $"{dir}{Path.DirectorySeparatorChar}";
        }
        print(dir);
        return dir;
    }

    public void OnFileDirectoryChanged(string newDir) {
        
        TryShow(newDir, filePath.text);

    }

    private void DisplayCanvas() {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void HideCanvas() {
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
        directoryContents.PopulateContents(path);
        filePath.text = directoryContents.currentDirectory.FullName;

    }

    public void GoToDirectory(string path) {
        directoryContents.PopulateContents(path);
        filePath.text = AddPathSeperator(directoryContents.currentDirectory.FullName);
    }

    public void GoUpADirectory() {
        GoToDirectory(AddPathSeperator(directoryContents.currentDirectory.Parent.FullName));
    }

    private void OnFileClicked(FileItem item, bool wasDoubleClick) {

        //double click
        if (wasDoubleClick) {
            //folder
            if (item.IsFolder) {
                GoToDirectory(item.directoryInfo.FullName);
            }
            //file
            else {
                filePath.text = item.fileInfo.FullName;
            }
        }

        //single click
        else {
            //folder
            if (item.IsFolder) {
                filePath.text = AddPathSeperator(item.directoryInfo.FullName);
            }
            //file
            else {
                filePath.text = item.fileInfo.FullName;
            }
        }
    }

    internal static bool IsValidFolder(string customPath) {
        return !string.IsNullOrEmpty(customPath) && Directory.Exists(customPath);
    }
}
