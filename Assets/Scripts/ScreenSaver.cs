using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScreenSaver : MonoBehaviour {

	private bool levelLoaded;
	public CanvasGroup screenSaverCanvas;
	public CanvasGroup fileBrowserCanvas;
	public InputField sessionInput;
	public InputField folderInput;
	public FileBrowser fb;
	public InputField from;
	public InputField to;
	public Text sessionInfo;
	private string[] allLines;
	private GameObject gazeIndicator;

	private bool _display;
	public bool display{
		get{
			return _display;
		}set{
			_display = value;
			if(value){
				screenSaverCanvas.alpha = 1f;
				screenSaverCanvas.interactable = true;
				screenSaverCanvas.blocksRaycasts = true;
			}else{
				screenSaverCanvas.alpha = 0f;
				screenSaverCanvas.interactable = false;
				screenSaverCanvas.blocksRaycasts = false;
			}
		}
	}

	public void OnDone(){

		GuiController gui = GameObject.Find ("GUI").GetComponent<GuiController> ();
		gui.guiEnable = true;

		Destroy(screenSaverCanvas.gameObject);
		Destroy(this.gameObject);
		Destroy(fb.gameObject);

        SceneManager.LoadScene("Start");
	}

	public void OnRender() {

		if (sessionInput.GetComponent<Image> ().color == Color.green) {
			if (folderInput.GetComponent<Image> ().color == Color.green){
				long st = long.Parse(from.text);
				long end = long.Parse(to.text);
				if((end >= st) && (end <= (allLines.Length-1))){
					//everything okay to render
					StartCoroutine(ProcessSessionData (sessionInput.text, folderInput.text));
				}
			}
		}
	}

	public void onBrowseSession(){
		fb.InitWithDirectory (Application.dataPath);
		fb.OnFileBrowserExit += ChooseSession;
		fb.display = true;
		display = false;
	}

	public void onBrowseFolder(){
		fb.InitWithDirectory (Application.dataPath);
		fb.OnFileBrowserExit += ChooseFolder;
		fb.display = true;
		display = false;
	}

	void ChooseSession(string file){
        fb.OnFileBrowserExit -= ChooseSession;
        if (string.IsNullOrEmpty(file)) return;
		
		fb.display = false;
		display = true;
		sessionInput.text = file;
		if (File.Exists (file)) {
			sessionInput.GetComponent<Image> ().color = Color.green;

			//get number of lines
			allLines = File.ReadAllLines(file);
			Debug.Log(file);
			Debug.Log(allLines.LongLength);

			int count = allLines.Length - 1;
			from.text = "1";
			to.text = count.ToString();
			sessionInfo.text = count.ToString() + " frames"; 

		} else {
			sessionInput.GetComponent<Image> ().color = Color.red;
		}
	}

	void ChooseFolder(string file){
        fb.OnFileBrowserExit -= ChooseFolder;
        if (string.IsNullOrEmpty(file)) { return; }
		
		fb.display = false;
		display = true;
		folderInput.text = file;
		if (Directory.Exists (file)) {
			folderInput.GetComponent<Image> ().color = Color.green;
		} else {
			folderInput.GetComponent<Image> ().color = Color.red;
		}
	}

	void BrowserCancel(string path){
		display = true;
		fb.OnFileBrowserExit -= ChooseFolder;
		fb.OnFileBrowserExit -= ChooseSession;
	}

	private GameObject robot;
	private Camera cam;

	void Start() {

		// dont destroy
		DontDestroyOnLoad(screenSaverCanvas.gameObject);
		DontDestroyOnLoad(this.gameObject);
		DontDestroyOnLoad(fb.gameObject);

		fb.OnFileBrowserExit += BrowserCancel;

		// find robot
		robot = GameObject.Find ("Robot");
		gazeIndicator = GameObject.Find ("gazeIndicator");

		// find Camera
		cam = GameObject.Find("Left Camera").GetComponent<Camera>();
		screenSaverCanvas.gameObject.GetComponent<Canvas> ().worldCamera = cam;

		// hide gui stuff
		GuiController gui = GameObject.Find ("GUI").GetComponent<GuiController> ();
		gui.guiEnable = false;

        SceneManager.sceneLoaded += this.OnSceneLoadCallback;
	}

    private void OnApplicationQuit()
    {
        levelLoaded = false;
        SceneManager.sceneLoaded -= this.OnSceneLoadCallback;
    }

    void OnSceneLoadCallback(Scene scene, LoadSceneMode sceneMode)
    {
        levelLoaded = true;
    }

    IEnumerator ProcessSessionData(string sessionPath, string toFolderPath){

		if (!File.Exists (sessionPath)) {
			Debug.LogError(sessionPath + " does not exist");
			yield break;
		}
		if (!Directory.Exists (toFolderPath)) {
			Debug.LogError(toFolderPath + " does not exist");
			yield break;
		}

		StreamReader sr = new StreamReader (sessionPath);

		// get level, first line
		string[] level = allLines[0].Split(new char[] {'=', ' '}, System.StringSplitOptions.RemoveEmptyEntries);
		Debug.Log("Screen capturing Level: " + level[level.Length - 1]);

		// load level and wait
		levelLoaded = false;
        SceneManager.LoadScene(level[level.Length - 1]);
		while(levelLoaded == false){
			yield return new WaitForSeconds(0.5f);
		}

		// disable stuff
		GameObject levelController = GameObject.Find ("LevelController");
		if (levelController != null) {
			levelController.SetActive (false);
		}

		//hide all gui stuff when rendering
		display = false;

		// read data	
		int st = int.Parse (from.text);
		int end = int.Parse (to.text);

		for (int counter = st; counter <= end; counter++) {

			string [] parameters = allLines[counter].Split(new char[] {' '},System.StringSplitOptions.RemoveEmptyEntries);
			float zpos,xpos,rot,eyex,eyey;
			
			// no trigger
			if(parameters.Length == 6){
				xpos = float.Parse(parameters[1]);
				zpos = float.Parse(parameters[2]);
				rot = float.Parse(parameters[3]);
				eyex = float.Parse(parameters[4]);
				eyey = float.Parse(parameters[5]);
				// move robot
				MoveRobotTo(robot,xpos,zpos,rot);
				//gazeindicator
				gazeIndicator.transform.localPosition = new Vector3(eyex,eyey,0.4f);
				// capture screen
				yield return new WaitForEndOfFrame();
				SaveScreen(cam, toFolderPath + "/" + counter.ToString() + ".jpg");
			}
			
			// trigger
			else if(parameters.Length == 7){
				xpos = float.Parse(parameters[2]);
				zpos = float.Parse(parameters[3]);
				rot = float.Parse(parameters[4]);
				eyex = float.Parse(parameters[5]);
				eyey = float.Parse(parameters[6]);

				Debug.Log(eyex + " " + eyey);

				// move robot
				MoveRobotTo(robot,xpos,zpos,rot);
				//gazeindicator
				gazeIndicator.transform.localPosition = new Vector3(eyex,eyey,0.4f);
				// capture screen
				yield return new WaitForEndOfFrame();
				SaveScreen(cam, toFolderPath + "/" + counter.ToString() + ".jpg");
			} else {
				Debug.Log("Nothing");
			}
		}

		//reshow gui
		display = true;
	}

	void MoveRobotTo(GameObject robot, float x, float z, float rot){
		Vector3 pos = robot.transform.position;
		pos.x = x;
		pos.z = z;
		Vector3 newrot = robot.transform.eulerAngles;
		newrot.y = rot;

		robot.transform.position = pos;
		robot.transform.eulerAngles = newrot;
	}

	void SaveScreen(Camera cam, string filename){


		Texture2D tex = new Texture2D (cam.pixelWidth, cam.pixelHeight, TextureFormat.RGB24, false);
		tex.ReadPixels (cam.pixelRect, 0, 0);
		tex.Apply ();
		byte[] bytes = tex.EncodeToJPG ();
		Destroy (tex);

		File.WriteAllBytes (filename, bytes);
	}
}
