using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public delegate void FileClickedDelegate(FileItem fileItem, bool isDoubleClick);

public class FileItem : MonoBehaviour {

    public static event FileClickedDelegate FileClickedEvent; //Annouce this event for others to suscribe to

    public Sprite folderSprite;
    public Sprite fileSprite;
    public Image iconImage;
    public Text nameText;

    public DirectoryInfo directoryInfo;
    public FileInfo fileInfo;
    public bool IsFolder { get; private set; }

    float doubleClickStart = 0;

    public void Init(DirectoryInfo directoryInfo) {
        this.directoryInfo = directoryInfo;
        this.IsFolder = true;
        iconImage.sprite = folderSprite;
        nameText.text = directoryInfo.Name;
    }

    public void Init(FileInfo fileInfo) {
        this.fileInfo = fileInfo;
        this.IsFolder = false;
        iconImage.sprite = fileSprite;
        nameText.text = fileInfo.Name;
    }

    //register this emthod with the Unity Event Trigger (in editor)
    public void Clicked() {

        //double click condition detected
        if ((Time.time - doubleClickStart) < 0.3f) {
            FileClickedEvent.Invoke(this, true); //Invoke event to all suscribers
            doubleClickStart = -1; //must start with a normal click again
        }
        //normal click
        else {
            FileClickedEvent.Invoke(this, false);
            doubleClickStart = Time.time; //start timing for doubleclick detection
        }
    }
}
