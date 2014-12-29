using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	//Fields
	string registerGameName = "UbisoftCompetitionConcordiaTeam1";
	bool isRefreshing = false;
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

	// Awake function
	void Awake () {
		DontDestroyOnLoad (gameObject);
		levelNames = new string[]{testLevelName, testLevel1Name, testLevel2Name, testLevel3Name};
	}

	//Start server function
	public void StartServer () {
		int listenPort = 25000; //25002;
		bool serverInitialized = false;
		while (!serverInitialized) {
			//try {


			//NetworkConnectionError error = Network.InitializeServer (16, listenPort, false); //25002 is default for Unity
			bool useNat = !Network.HavePublicAddress();
			NetworkConnectionError error = Network.InitializeServer(32, listenPort, useNat);
			if (error == NetworkConnectionError.NoError) {
				serverInitialized = true;
				MasterServer.RegisterHost (registerGameName, "Team_1_Network_Test", "This is a network test");
			}
			else {
				serverInitialized = false;
				listenPort++;
			}
			//}
//			catch (System.Exception e) {
//				listenPort++;
//				continue;
//			}
//			serverInitialized = true;
		}
		//Network.InitializeServer (16, 25002, false); //25002 is default for Unity
		//MasterServer.RegisterHost (registerGameName, "Team_1_Network_Test", "This is a network test");
	}

	//Events built into Unity
	void OnServerInitialized () {
		print ("Server has been initialized!");
		//SpawnPlayer();
	}

	void OnMasterServerEvent (MasterServerEvent masterServerEvent) {
		if (masterServerEvent == MasterServerEvent.RegistrationSucceeded) {
			print ("Registration Successful!");
		}
	}

	void OnConnectedToServer () {
		print ("Connected to server!");
		//SpawnPlayer();
	}

	void OnDisconnectedFromServer(NetworkDisconnection info) {
		print (info.ToString());
//		if (Network.isServer) {
//			Network.Disconnect(200);
//			MasterServer.UnregisterHost();
//		}

//		Network.RemoveRPCs (player);
//		Network.DestroyPlayerObjects (player);
		Application.LoadLevel (menuLevelName);
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

	void OnPlayerDisconnected (NetworkPlayer player) {
		print ("Clean up after player " + player);
		Network.RemoveRPCs (player);
		Network.DestroyPlayerObjects (player);
	}

	public IEnumerator RefreshHostList () {
		print ("Refreshing...");
		MasterServer.RequestHostList (registerGameName);

		float timeStarted = Time.time;
		float timeEnd = Time.time + refreshRequestLength;

		while (Time.time < timeEnd) {
			hostData = MasterServer.PollHostList();
			yield return new WaitForEndOfFrame();
		}

		if (hostData == null || hostData.Length == 0) {
			print ("No active servers have been found.");
		}
		else {
			print (hostData.Length + " hosts has been found.");
		}
	}

	//Spawnning a player through the network
	public void SpawnPlayer () {
		print ("Spawning player...");
		//Network.Instantiate (Resources.Load ("Prefabs/Player"), new Vector3 (0, 1, 0), Quaternion.identity, 0);
		Network.Instantiate (Resources.Load ("Prefabs/Player_ThirdPerson"), new Vector3 (0, 1, 0), Quaternion.identity, 0);
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
		//If the loaded level is not the main menu
		if (level != 0) {
			SpawnPlayer();
			Network.isMessageQueueRunning = true;
		}
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

	//OnGui
	public void OnGUI () {

		if (Application.loadedLevelName == menuLevelName) {
			if (Network.isServer) {
//				if (GUI.Button (new Rect(30f, 30f, 150f, 30f), "Load Test Level")) {
//					//Application.LoadLevel (testLevelName);
//					networkView.RPC ("LoadLevel", RPCMode.AllBuffered, testLevelName, lastLevelPrefix+1);
//				}
				for (int i = 0; i < totalLevelNum; i++) {
					if (GUI.Button(new Rect(Screen.width/2, (30f*i), 300f, 30f), "Load " + levelNames[i])) {
						//Load the chosen level
						//Network.Connect(hostData[i]);
						networkView.RPC ("LoadLevel", RPCMode.AllBuffered, levelNames[i], lastLevelPrefix+1);
					}
				}
			}
			else if (!Network.isClient && !Network.isServer) {
				if (GUI.Button(new Rect(25f, 25f, 150f, 30f), "Start New Server")) {
					// Start Server Function Here
					StartServer ();
				}
				
				if (GUI.Button(new Rect(25f, 60f, 150f, 30f), "Refresh Server List:")) {
					//Refresh Server List Function Here
					StartCoroutine("RefreshHostList");
				}
				
				//Display a button for each host found
				if (hostData != null) {
					for (int i = 0; i < hostData.Length; i++) {
						if (GUI.Button(new Rect(Screen.width/2, 65f+(30f*i), 300f, 30f), hostData[i].gameName)) {
							//Connect to that host/server
							NetworkConnectionError error = Network.Connect(hostData[i]);
							print (error);
						}
					}
				}
			}
		}
		else {
			if (Network.isClient || Network.isServer) {
				if (GUI.Button(new Rect(25f, 25f, 100f, 30f), "Spawn Player")) {
					// Spawn a player
					SpawnPlayer ();
				}
			}
		}

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
