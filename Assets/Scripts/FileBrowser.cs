using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;

public delegate void FileBrowserCancel ();
public delegate void FileBrowserChoose(string path);

public class FileBrowser : MonoBehaviour {

	// user of FileBrowser will suscribe his methods to these 2 events
	public event FileBrowserCancel fileBrowserCancelEvent;
	public event FileBrowserChoose fileBrowserChooselEvent;

	//drag from editor
	public DirectoryContents directoryContents;
	public InputField filePath;
	public CanvasGroup canvasGroup;

	private bool _display;
	public bool display{
		get{
			return _display;
		}set{
			_display = value;
			if(value){
				canvasGroup.alpha = 1f;
				canvasGroup.interactable = true;
				canvasGroup.blocksRaycasts = true;
				this.gameObject.transform.SetAsLastSibling();
			}else{
				canvasGroup.alpha = 0f;
				canvasGroup.interactable = false;
				canvasGroup.blocksRaycasts = false;
				this.gameObject.transform.SetAsFirstSibling();
			}
		}
	}

	void Awake(){
		display = false;
	}

	public void OnChoose(){
		fileBrowserChooselEvent.Invoke (filePath.text);
	}

	public void OnCancel(){
		fileBrowserCancelEvent.Invoke ();
	}

	public void OnUp(){
		GoUpADirectory ();
	}

	public void OnNewFolder(){
		
	}

	public void OnNewFile(){
		
	}

	public void InitWithDirectory(string path){
		directoryContents.InitWithContentsOfDirectory (path, FileClicked);
		filePath.text = directoryContents.currentDirectory.FullName;
	}

	public void GoToDirectory(string path){
		directoryContents.GetContentsOfDirectory (path);
		filePath.text = directoryContents.currentDirectory.FullName;
	}
	
	public void GoUpADirectory(){
		directoryContents.GetContentsOfDirectory (directoryContents.currentDirectory.Parent.FullName);
		filePath.text = directoryContents.currentDirectory.FullName;
	}

	void FileClicked(FileItem item, bool wasDoubleClick){
		
		//double click
		if (wasDoubleClick) {
			//folder
			if(item.isFolder){
				GoToDirectory(item.directoryInfo.FullName);
				filePath.text = item.directoryInfo.FullName;
			}
			//file
			else{
				filePath.text = item.fileInfo.FullName;
			}
		} 
		
		//single click
		else {
			//folder
			if(item.isFolder){
				filePath.text = item.directoryInfo.FullName;
			}
			//file
			else{
				filePath.text = item.fileInfo.FullName;
			}
		}
	}
}













