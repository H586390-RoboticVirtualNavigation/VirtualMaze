using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class DirectoryContents : MonoBehaviour {

	public Transform contentPanel;
	public GameObject fileItemPrefab;

	public DirectoryInfo currentDirectory;
	private DirectoryInfo[] folders;
	private FileInfo[] files;
	private FileClickedDelegate fileClickedDelegate;
	private GameObject currentlySelectedItem;

	public void InitWithContentsOfDirectory(string path, FileClickedDelegate OnFileClicked){
		fileClickedDelegate = OnFileClicked;
		GetContentsOfDirectory(path);
	}

	public void GetContentsOfDirectory(string path){
		try{
			currentDirectory = new DirectoryInfo (path);
			folders = currentDirectory.GetDirectories ();
			files = currentDirectory.GetFiles ();
			Populate ();
		} catch {
			Debug.LogError("invalid directory path");
		}
	}

	void Populate(){

		ResetContents();
		foreach (DirectoryInfo folder in folders) {
			GameObject item = Instantiate(fileItemPrefab) as GameObject;
			item.GetComponent<FileItem>().Init(folder, SelectionHighlights, fileClickedDelegate);
			item.transform.SetParent(contentPanel,false);
		}
		foreach (FileInfo file in files) {
			GameObject item = Instantiate(fileItemPrefab) as GameObject;
			item.GetComponent<FileItem>().Init(file, SelectionHighlights, fileClickedDelegate);
			item.transform.SetParent(contentPanel,false);
		}
	}

	void ResetContents(){
		foreach (Transform child in contentPanel) {
			Destroy(child.gameObject);
		}
	}

	void SelectionHighlights(FileItem item, bool wasDoubleClick){

		if (!(currentlySelectedItem == item.gameObject)) {
			
			Color col;
			
			//deselect previously selected item
			if(currentlySelectedItem != null){
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
