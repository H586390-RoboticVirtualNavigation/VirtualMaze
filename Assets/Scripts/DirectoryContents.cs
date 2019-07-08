﻿using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class DirectoryContents : MonoBehaviour {

    public Transform contentPanel;
    public GameObject fileItemPrefab;

    public DirectoryInfo currentDirectory;
    private DirectoryInfo[] folders;
    private FileInfo[] files;
    private GameObject currentlySelectedItem;

    private void Awake() {
        FileItem.FileClickedEvent += SelectionHighlights;
    }

    public void PopulateContents(string path) {
        try {
            currentDirectory = new DirectoryInfo(path);
            folders = currentDirectory.GetDirectories();
            files = currentDirectory.GetFiles();
            Populate();
        }
        catch {
            Debug.LogWarning($"Unable to open: {path}");
        }
    }

    void Populate() {

        ResetContents();
        foreach (DirectoryInfo folder in folders) {
            GameObject item = Instantiate(fileItemPrefab) as GameObject;
            item.GetComponent<FileItem>().Init(folder);
            item.transform.SetParent(contentPanel, false);
        }
        foreach (FileInfo file in files) {
            GameObject item = Instantiate(fileItemPrefab) as GameObject;
            item.GetComponent<FileItem>().Init(file);
            item.transform.SetParent(contentPanel, false);
        }
    }

    void ResetContents() {
        foreach (Transform child in contentPanel) {
            Destroy(child.gameObject);
        }
    }

    void SelectionHighlights(FileItem item, bool wasDoubleClick) {

        if (!(currentlySelectedItem == item.gameObject)) {

            Color col;

            //deselect previously selected item
            if (currentlySelectedItem != null) {
                col = currentlySelectedItem.GetComponent<Image>().color;
                col.a = 0.5f;
                currentlySelectedItem.GetComponent<Image>().color = col;
            }

            //set new selected
            currentlySelectedItem = item.gameObject;
            col = item.GetComponent<Image>().color;
            col.a = 1.0f;
            item.GetComponent<Image>().color = col;
        }
    }

}
