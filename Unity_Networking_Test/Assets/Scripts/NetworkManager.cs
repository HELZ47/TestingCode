using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour {

	//Fields
	string registerGameName = "UbisoftCompetitionConcordiaTeam1";
	//bool isRefreshing = false;
	float refreshRequestLength = 3f;
	HostData[] hostData;
	int lastLevelPrefix = 0;

	public string menuLevelName = "Main_Menu";
	public string testLevelName = "Test";
	public string testLevel1Name = "Test 2";
	public string testLevel2Name = "Test 3";
	public string testLevel3Name = "Test 4";
	public int totalLevelNum = 4;
	public string[] levelNames;

	public RectTransform panelTransform;
	List<GameObject> UI_Buttons;

	// Initiate its own/local variables
	void Awake () {
		DontDestroyOnLoad (gameObject); //Keep the network manager alive through the level transitions
		levelNames = new string[]{testLevelName, testLevel1Name, testLevel2Name, testLevel3Name};
		UI_Buttons = new List<GameObject> ();
	}


	#region Start the server and show the levels
	//Start the server by registering the host with the master server
	public void StartServer () {
		//IF the program is already a server, then don't execute the function
		if (Network.isServer) {
			return;
		}
		StopAllCoroutines (); //If the program was pulling the host info, stop that before start the server
		int listenPort = 25000; //25002 is Unity default, but apparantly 25000 works
		bool serverInitialized = false;
		//Keep changing listenning ports until the game session is registered to the master server
		while (!serverInitialized) {
			bool useNat = !Network.HavePublicAddress(); //Test if NAT punch through is needed, it is if there's no public ip address
			NetworkConnectionError error = Network.InitializeServer(16, listenPort, useNat);
			if (error == NetworkConnectionError.NoError) {
				serverInitialized = true;
				MasterServer.RegisterHost (registerGameName, "Team_1_Network_Test", "This is a network test");
			}
			else {
				print (error);
				serverInitialized = false;
				listenPort++;
				print ("ListenPort changed to " + listenPort);
			}
		}
		//Show the levels that can be loaded as buttons inside of the panel
		ShowLevels ();
	}

	//Refresh and show buttons that represents the levels to be loaded
	public void ShowLevels () {
		for(int i = 0; i < UI_Buttons.Count; i++) {
			Destroy (UI_Buttons[i]);
			print ("Button deleted!");
		}
		UI_Buttons.Clear();
		for (int i = 0; i < totalLevelNum; i++) {
			int levelIndex = i;
			GameObject button = Instantiate (Resources.Load ("Prefabs/UI_Button"), Vector3.zero, new Quaternion ()) as GameObject;
			button.transform.SetParent (panelTransform, true);
			button.GetComponent<Button>().onClick.AddListener (() => LoadLevelHelper(levelNames[levelIndex]));
			button.GetComponent<Button>().GetComponentInChildren<Text>().text = levelNames[levelIndex];
			UI_Buttons.Add (button);
			//			if (GUI.Button(new Rect(Screen.width/2, (30f*i), 300f, 30f), "Load " + levelNames[i])) {
			//				//Load the chosen level
			//				networkView.RPC ("LoadLevel", RPCMode.AllBuffered, levelNames[i], lastLevelPrefix+1);
			//			}
		}
	}
	#endregion


	#region NetworkEvents_Unused
	void OnServerInitialized () {
		print ("Server has been initialized!");
		//SpawnPlayer();
	}

	void OnConnectedToServer () {
		print ("Connected to server!");
		//SpawnPlayer();
	}
	void OnFailedToConnect(NetworkConnectionError error) {
		print (error.ToString());
	}
	
	void OnFailedToConnectToMasterServer (NetworkConnectionError info) {
		print (info.ToString());
	}
	
	void OnNetworkInstantiate (NetworkMessageInfo info) {
		print (info.ToString());
	}
	
	void OnPlayerConnected () {
		
	}

	void OnMasterServerEvent (MasterServerEvent masterServerEvent) {
		if (masterServerEvent == MasterServerEvent.RegistrationSucceeded) {
			print ("Registration Successful!");
		}
	}
	#endregion

	#region NetworkEvents_Used
	//If the client is disconnected from the server, load back to the main menu
	void OnDisconnectedFromServer(NetworkDisconnection info) {
		print (info.ToString());
		Application.LoadLevel (menuLevelName);
	}
	
	//IF a player is disconnected, clean up after than player by removing its RPCs and distroy its objects
	void OnPlayerDisconnected (NetworkPlayer player) {
		print ("Clean up after player " + player);
		Network.RemoveRPCs (player);
		Network.DestroyPlayerObjects (player);
	}

	//Called when the application quits and clean up
	public void OnApplicationQuit () {
		if (Network.isServer) {
			Network.Disconnect (200);
			MasterServer.UnregisterHost();
		}
		if (Network.isClient) {
			Network.Disconnect(200);
		}
	}
	
	//OnApplicationLoad
	public void OnLevelWasLoaded(int level) {
		//If the loaded level is not the main menu, spawn the player
		if (level != 0) {
			SpawnPlayer();
			Network.isMessageQueueRunning = true;
		}
	}
	#endregion

	#region Load Level
	//The RPC helper function that calls the RPC function to load the level
	public void LoadLevelHelper (string levelName) {
		networkView.RPC ("LoadLevel", RPCMode.AllBuffered, levelName, lastLevelPrefix+1);
	}

	//RPC function that loads a level
	[RPC]
	public void LoadLevel (string levelName, int levelPrefix) {
		lastLevelPrefix = levelPrefix;
		//Network.SetSendingEnabled (0, false);
		Network.isMessageQueueRunning = false;
		Network.SetLevelPrefix (levelPrefix);
		Application.LoadLevel (levelName);
		//yield return null;
		//yield return null;
		//Network.isMessageQueueRunning = true;
		//Network.SetSendingEnabled (0, true);
	}
	#endregion

	#region Pull Host List and connect to a host
	//Refrest the list of hosts by starting a coroutine that pulls the hosts for a set time
	public void RefreshHList () {
		if (!Network.isServer) {
			StopAllCoroutines ();
			StartCoroutine("RefreshHostList");
		}
//		else {
//			StopAllCoroutines ();
//			MasterServer.UnregisterHost();
//			Network.Disconnect (200);
//			//StartCoroutine("RefreshHostList");
//		}
	}

	//Retrieve a list of host information
	public IEnumerator RefreshHostList () {
		print ("Refreshing...");
		MasterServer.RequestHostList (registerGameName);
		float timeStarted = Time.time;
		float timeEnd = Time.time + refreshRequestLength;
		//Get the host data for certain set time
		while (Time.time < timeEnd) { 
			hostData = MasterServer.PollHostList();
			HostRefresh (); //Refresh and display the buttons that represents the hosts
			yield return new WaitForEndOfFrame();
		}
		//Debug output that shows whether any server has been found, at the end of the refresh time
		if (hostData == null || hostData.Length == 0) {
			print ("No active servers have been found.");
		}
		else {
			print (hostData.Length + " hosts has been found.");
		}
	}

	//Refresh and display the list of buttons that connects to the available hosts
	public void HostRefresh () {
		if (hostData.Length != UI_Buttons.Count) {
			for(int i = 0; i < UI_Buttons.Count; i++) {
				Destroy (UI_Buttons[i]);
				print ("Button deleted!");
			}
			UI_Buttons.Clear();
			for (int j = 0; j < hostData.Length; j++) {
				GameObject button = Instantiate (Resources.Load ("Prefabs/UI_Button"), Vector3.zero, new Quaternion ()) as GameObject;
				button.transform.SetParent (panelTransform, true);
				int hostIndex = j;
				button.GetComponent<Button>().onClick.AddListener (() => ConnectToHost(hostIndex));
				button.GetComponent<Button>().GetComponentInChildren<Text>().text = hostData[hostIndex].gameName;
				UI_Buttons.Add (button);
				//UI_Buttons[i].GetComponent<Button>().onClick.AddListener (() => ConnectToHost(i));
				print ("Button created on index " + j);
				//				print (button);
			}
		}
	}

	//Helper function that connect to hosts, this is linked to the host buttons
	public void ConnectToHost (int hostIndex) {
		print ("Connect to host " + hostIndex);
		Network.Connect (hostData [hostIndex]);
	}
	#endregion


	//Spawnning a player through the network
	public void SpawnPlayer () {
		print ("Spawning player...");
		//Network.Instantiate (Resources.Load ("Prefabs/Player"), new Vector3 (0, 1, 0), Quaternion.identity, 0);
		Network.Instantiate (Resources.Load ("Prefabs/Player_ThirdPerson"), new Vector3 (0, 1, 0), Quaternion.identity, 0);
	}





	//OnGui
	public void OnGUI () {
		//If the game is in the mainmenu
//		if (Application.loadedLevelName == menuLevelName) {
//			if (Network.isServer) {
////				if (GUI.Button (new Rect(30f, 30f, 150f, 30f), "Load Test Level")) {
////					//Application.LoadLevel (testLevelName);
////					networkView.RPC ("LoadLevel", RPCMode.AllBuffered, testLevelName, lastLevelPrefix+1);
////				}
////				for (int i = 0; i < totalLevelNum; i++) {
////					if (GUI.Button(new Rect(Screen.width/2, (30f*i), 300f, 30f), "Load " + levelNames[i])) {
////						//Load the chosen level
////						//Network.Connect(hostData[i]);
////						networkView.RPC ("LoadLevel", RPCMode.AllBuffered, levelNames[i], lastLevelPrefix+1);
////					}
////				}
//			}
//			else if (!Network.isClient && !Network.isServer) {
////				if (GUI.Button(new Rect(25f, 25f, 150f, 30f), "Start New Server")) {
////					// Start Server Function Here
////					StartServer ();
////				}
////				
////				if (GUI.Button(new Rect(25f, 60f, 150f, 30f), "Refresh Server List:")) {
////					//Refresh Server List Function Here
////					StartCoroutine("RefreshHostList");
////				}
//				
//				//Display a button for each host found
//				if (hostData != null) {
//					for (int i = 0; i < hostData.Length; i++) {
////						GameObject button = Instantiate (Resources.Load ("Prefabs/UI_Button"), Vector3.zero, new Quaternion ()) as GameObject;
////						//button.GetComponent<RectTransform>().SetParent (panelTransform, true);
////						button.transform.SetParent (panelTransform, true);
////						print (button);
////						if (GUI.Button(new Rect(Screen.width/2, 65f+(30f*i), 300f, 30f), hostData[i].gameName)) {
////							//Connect to that host/server
////							NetworkConnectionError error = Network.Connect(hostData[i]);
////							print (error);
////						}
//					}
//				}
//			}
//		}
//		//IF a game level is loaded
//		else {
////			if (Network.isClient || Network.isServer) {
////				if (GUI.Button(new Rect(25f, 25f, 100f, 30f), "Spawn Player")) {
////					// Spawn a player
////					SpawnPlayer ();
////				}
////			}
//		}

		if (Network.isServer) {
			GUILayout.Label("Running as a server.");
		}
		else if (Network.isClient) {
			GUILayout.Label("Running as a client.");
		}

//		if (Network.isClient) {
//			if (GUI.Button(new Rect(25f, 25f, 100f, 30f), "Spawn Player")) {
//				// Spawn a player
//				SpawnPlayer ();
//			}
//		}

//		if (!Network.isClient && !Network.isServer) {
//			if (GUI.Button(new Rect(25f, 25f, 150f, 30f), "Start New Server")) {
//				// Start Server Function Here
//				StartServer ();
//			}
//
//			if (GUI.Button(new Rect(25f, 60f, 150f, 30f), "Refresh Server List:")) {
//				//Refresh Server List Function Here
//				StartCoroutine("RefreshHostList");
//			}
//
//			//Display a button for each host found
//			if (hostData != null) {
//				for (int i = 0; i < hostData.Length; i++) {
//					if (GUI.Button(new Rect(Screen.width/2, 65f+(30f*i), 300f, 30f), hostData[i].gameName)) {
//						//Connect to that host/server
//						Network.Connect(hostData[i]);
//					}
//				}
//			}
//		}
	}
}
