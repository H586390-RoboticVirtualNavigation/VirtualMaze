using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public delegate void FileClickedDelegate(FileItem fileItem, bool isDoubleClick);

public class FileItem : MonoBehaviour {

	public event FileClickedDelegate fileClickedEvent; //Annouce this event for others to suscribe to

	public Sprite folderSprite;
	public Sprite fileSprite;
	public Image iconImage;
	public Text nameText;

	public DirectoryInfo directoryInfo;
	public FileInfo fileInfo;
	public bool isFolder;

	float doubleClickStart = 0;

	public void Init(DirectoryInfo directoryInfo, params FileClickedDelegate[] OnFileClick){
		this.directoryInfo = directoryInfo;
		this.isFolder = true;
		iconImage.sprite = folderSprite;
		nameText.text = directoryInfo.Name;
		foreach (FileClickedDelegate del in OnFileClick) {
			fileClickedEvent += del;
		}
	}

	public void Init(FileInfo fileInfo, params FileClickedDelegate[] OnFileClick){
		this.fileInfo = fileInfo;
		this.isFolder = false;
		iconImage.sprite = fileSprite;
		nameText.text = fileInfo.Name;
		foreach (FileClickedDelegate del in OnFileClick) {
			fileClickedEvent += del;
		}
	}

	//register this emthod with the Unity Event Trigger (in editor)
	public void Clicked(){
		
		//double click condition detected
		if ((Time.time - doubleClickStart) < 0.3f) {
			fileClickedEvent.Invoke(this, true); //Invoke event to all suscribers
			doubleClickStart = -1; //must start with a normal click again
		} 
		//normal click
		else {
			fileClickedEvent.Invoke(this, false);
			doubleClickStart = Time.time; //start timing for doubleclick detection
		}
	}
}









