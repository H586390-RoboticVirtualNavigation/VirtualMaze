using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Text;
using System.IO;
using UnityEngine.SceneManagement;

public class NetworkConnection : MonoBehaviour {

	private BackgroundWorker bwListener;
	private Socket listenerSocket;
	private Int32 port = 8000;
	private string ipAddress = "127.0.0.1";
	private NetworkStream networkStream;
	private bool changeLevelFlag = false;
	private string changeToLevel;
	public bool replayMode = false;
	public Transform robot;

	private bool setRobotFlag = false;
	private float robotposx;
	private float robotposz;
	private float robotroty;

	private static NetworkConnection _instance;
	public static NetworkConnection instance{
		get{
			if(!_instance){
				_instance = FindObjectOfType(typeof(NetworkConnection)) as NetworkConnection;
				if(!_instance){
					Debug.Log("need at least 1 network connection game object");
				}
			}
			return _instance;
		}
	}

	void Update(){
		if (changeLevelFlag == true) {
			changeLevelFlag = false;
            SceneManager.LoadScene(changeToLevel);
		}
		if (setRobotFlag == true) {
			setRobotFlag = false;
			robot.position = new Vector3(robotposx, 0.5f, robotposz);
			robot.eulerAngles = new Vector3(0,robotroty,0);
		}
	}

	void ProcessCommands(string line){

		string[] words = line.Split(' ');

		if(words.Length < 1){
			Debug.Log("Invalid Command");
		}else{

			if (words[0].Equals("ChangeLevel")) {
				if(words.Length != 2){
					Debug.Log("Invalid Command");
				}else {
					Debug.Log(words[1]);
					if(words[1].Equals("Linear")){
						changeLevelFlag = true;
						changeToLevel = "Linear";
						replayMode = true;
					}
					else if(words[1].Equals("Tee")){
						changeLevelFlag = true;
						changeToLevel = "Tee";
						replayMode = true;
					}
					else if(words[1].Equals("Four-Arm")){
						changeLevelFlag = true;
						changeToLevel = "Four-Arm";
						replayMode = true;
					}
				}
			}

			if (words[0].Equals("SetRobot")) {
				if(words.Length != 4){
					Debug.Log("Invalid Command");
				}else{
					setRobotFlag = true;
					robotposx = (float)Convert.ToDouble(words[1]);
					robotposz = (float)Convert.ToDouble(words[2]);
					robotroty = (float)Convert.ToDouble(words[3]);
				}
			}
		}
	}


	private TcpListener server = null;
	private TcpClient client = null;

	void BackGroundListen(object sender , DoWorkEventArgs e){

		//TcpListener server=null;   
		try
		{
			// Set the TcpListener on port 8000.
			IPAddress localAddr = IPAddress.Parse(ipAddress);
			
			// TcpListener server = new TcpListener(port);
			server = new TcpListener(localAddr, port);
			
			// Start listening for client requests.
			server.Start();

			// Enter the listening loop.
			while(true) 
			{
				Debug.Log("Waiting for a connection... ");
				
				// Perform a blocking call to accept requests.
				// You could also user server.AcceptSocket() here.
				client = server.AcceptTcpClient();            
				Debug.Log("Connected!");

				// Get a stream object for reading and writing
				NetworkStream stream = client.GetStream();

				try{
					using(StreamReader streamReader = new StreamReader(stream)){
						string line = null;
						
						do{
							line = streamReader.ReadLine();
							ProcessCommands(line);
						}while(line != null);
					}
				}catch (Exception excp) {
					Debug.Log(excp.ToString());
				}


				// Shutdown and end connection
				Debug.Log("Connection closed");
				client.Close();
			}
		}
		catch(SocketException ex)
		{
			Debug.Log(String.Format("SocketException: {0}", ex));
		}
		finally
		{
			// Stop listening for new clients.
			server.Stop();
		}

	}

	public void StartReplayServer (){
		bwListener = new BackgroundWorker();
		bwListener.DoWork += new DoWorkEventHandler(BackGroundListen);	
		bwListener.RunWorkerAsync();
	}

	public void StopReplayServer(){
		if (client != null)
			client.Close ();
		if (server != null)
			server.Stop ();
		bwListener.Dispose ();
	}

}









