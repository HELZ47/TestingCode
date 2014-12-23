using UnityEngine;
using System.Collections;

public class FirstPersonKeys : MonoBehaviour {

	//Fields
	Camera mainCamera;
	int speed = 3;
	public bool activated;

//	void Awake () {
//		DontDestroyOnLoad (transform);
//	}

	// Use this for initialization
	void Start () {
		mainCamera = GetComponentInChildren<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) {
			activated = false;
			return;
		}
		else {
			activated = true;
		}
		CheckInput ();
	}

	//Check the keyboard input and update the player accordingly
	void CheckInput () {
		Vector3 forwardXZ = new Vector3 (mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
		Vector3 rightXZ = new Vector3 (mainCamera.transform.right.x, 0, mainCamera.transform.right.z);
		int keysPressed = 0;
		Vector3 directionVector = new Vector3 ();


		if (Input.GetKey(KeyCode.W)) {
			keysPressed++;
			print ("hahaha");
		}
		if (Input.GetKey(KeyCode.S)) {
			keysPressed++;
			print ("hahaha");
		}
		if (Input.GetKey(KeyCode.A)) {
			keysPressed++;
		}
		if (Input.GetKey(KeyCode.D)) {
			keysPressed++;
		}

		if (Input.GetKey(KeyCode.W)) {
			//transform.Translate (forwardXZ * ((float)speed/(float)keysPressed) * Time.deltaTime);
			directionVector += forwardXZ;
		}
		if (Input.GetKey(KeyCode.S)) {
			//transform.Translate (-forwardXZ * ((float)speed/(float)keysPressed) * Time.deltaTime);
			directionVector -= forwardXZ;
		}
		if (Input.GetKey(KeyCode.A)) {
			//transform.Translate (-rightXZ * ((float)speed/(float)keysPressed) * Time.deltaTime);
			directionVector -= rightXZ;
		}
		if (Input.GetKey(KeyCode.D)) {
			//transform.Translate (rightXZ * ((float)speed/(float)keysPressed) * Time.deltaTime);
			directionVector += rightXZ;
		}

		transform.Translate (directionVector.normalized * speed * Time.deltaTime);
	}
}
