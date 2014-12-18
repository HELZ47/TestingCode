using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {

	//Fields
	public string testLevelName = "Test";

	// Awake function
	void Awake () {
		//DontDestroyOnLoad (gameObject);
	}

	//OnGUI
	public void OnGUI() {
		if (Network.isServer) {
			if (Application.loadedLevelName == "Main_Menu") {
				if (GUI.Button (new Rect(30f, 30f, 150f, 30f), "Load Test Level")) {
					Application.LoadLevel (testLevelName);
				}
			}
		}
	}
}
