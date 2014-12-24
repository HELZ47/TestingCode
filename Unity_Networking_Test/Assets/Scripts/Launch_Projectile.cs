using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Handles lauching the projectile and its related controls
public class Launch_Projectile : MonoBehaviour {

	//Fields
	bool rightTriggerDown, rightTriggerReset;
	
	// Use this for initialization
	void Start () {
		rightTriggerDown = false;
		rightTriggerReset = false;
	}


	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) { //Skip this function if the object is not the player's
			return;
		}

		//Debug: Print out the names of the controllers detected -----------------------------
		bool leftTrigger = false, leftBumper = false, rightBumper = false;
		if (Application.platform == RuntimePlatform.OSXDashboardPlayer ||
		    Application.platform == RuntimePlatform.OSXEditor ||
		    Application.platform == RuntimePlatform.OSXPlayer ||
		    Application.platform == RuntimePlatform.OSXWebPlayer) {
			leftTrigger = Input.GetAxis ("Mac_LeftTrigger") > 0;
			leftBumper = Input.GetButtonDown ("Mac_LeftBumper");
			rightBumper = Input.GetButtonDown ("Mac_RightBumper");
			//Testing if the right trigger has been pulled from its resting position (like getButtonDown)
			if (Input.GetAxis("Mac_RightTrigger") > -1 && rightTriggerDown == false && rightTriggerReset == true) {
				rightTriggerDown = true;
			}
			else if (rightTriggerDown && Input.GetAxis("Mac_RightTrigger") > -1) {
				rightTriggerDown = false;
				rightTriggerReset = false;
			}
			else if (rightTriggerReset == false && Input.GetAxis("Mac_RightTrigger") == -1) {
				rightTriggerReset = true;
			}
		}
		else if (Application.platform == RuntimePlatform.WindowsEditor ||
		         Application.platform == RuntimePlatform.WindowsPlayer ||
		         Application.platform == RuntimePlatform.WindowsWebPlayer) {
			leftTrigger = Input.GetAxis ("Windows_LeftTrigger") > 0;
			leftBumper = Input.GetButtonDown ("Windows_LeftBumper");
			rightBumper = Input.GetButtonDown ("Windows_RightBumper");
			//Testing if the right trigger has been pulled from its resting position (like getButtonDown)
			if (Input.GetAxis("Windows_RightTrigger") > -1 && rightTriggerDown == false && rightTriggerReset == true) {
				rightTriggerDown = true;
			}
			else if (rightTriggerDown && Input.GetAxis("Windows_RightTrigger") > -1) {
				rightTriggerDown = false;
				rightTriggerReset = false;
			}
			else if (rightTriggerReset == false && Input.GetAxis("Windows_RightTrigger") == -1) {
				rightTriggerReset = true;
			}
		}
		//------------------------------------------------------------------------------------

//		//Testing if the right trigger has been pulled from its resting position (like getButtonDown)
//		if (Input.GetAxis("Mac_RightTrigger") > -1 && rightTriggerDown == false && rightTriggerReset == true) {
//			rightTriggerDown = true;
//		}
//		else if (rightTriggerDown && Input.GetAxis("Mac_RightTrigger") > -1) {
//			rightTriggerDown = false;
//			rightTriggerReset = false;
//		}
//		else if (rightTriggerReset == false && Input.GetAxis("Mac_RightTrigger") == -1) {
//			rightTriggerReset = true;
//		}
		//Mouse Buttons: 0-->Left, 1-->right, 2-->wheel
		//launched: rightTriggerDown or rightBumperDown or leftBumperDown
		bool launched = rightTriggerDown || rightBumper || leftBumper;
		//If the corresponding input is pressed, launch the corresponding projectile
		if (Input.GetMouseButtonDown (0) || launched) {
			//Get the third person camera and it's target transform
			ThirdPersonCamera TPCamera = GetComponentInChildren<ThirdPersonCamera>();
			Transform targetTransform = TPCamera.cameraTarget.transform;
			//Get the start position of the projectile based on the direction of the camera target
			Vector3 projectileDirection = (targetTransform.position - TPCamera.transform.position).normalized;
			Vector3 projectionStartPos = targetTransform.position + projectileDirection;
			RaycastHit rayHit;
			Vector3 projectileTargetPosition;
			//Raycast from the player to the target to see if we are target something
			if (Physics.Raycast (projectionStartPos, projectileDirection, out rayHit)) {
				projectileTargetPosition = rayHit.point;
			}
			//If not targeting anything, set the target far in that direction
			else {
				projectileTargetPosition = projectionStartPos + (projectileDirection*100f);
			}

			//Create a RPC call that creates the corresponding projectile
			GameObject fireBall;
			if (rightTriggerDown) {
				Vector3 direction = (projectileTargetPosition - transform.position).normalized;
				networkView.RPC ("CreateProjectile", RPCMode.AllBuffered, "Prefabs/Fire_Bullet", transform.position+(projectileDirection), new Quaternion(), direction.normalized, Network.player);
			}
			else if (rightBumper) {
				Vector3 direction = (projectileTargetPosition - transform.position).normalized;
				networkView.RPC ("CreateProjectile", RPCMode.AllBuffered, "Prefabs/Fire_Grenade", transform.position+(projectileDirection), new Quaternion(), direction.normalized, Network.player);
			}
			else if (leftBumper) {
				Vector3 direction = (projectileTargetPosition - transform.position).normalized;
				networkView.RPC ("CreateProjectile", RPCMode.AllBuffered, "Prefabs/Fire_Orb", transform.position+(projectileDirection), new Quaternion(), direction.normalized, Network.player);
			}
			//Mouse can only lauch one kind of projectile
			else {
				Vector3 direction = (projectileTargetPosition - transform.position).normalized;
				networkView.RPC ("CreateProjectile", RPCMode.AllBuffered, "Prefabs/Fire_Orb", transform.position+(projectileDirection), new Quaternion(), direction.normalized, Network.player);
			}
		}

		//Handles the zooming function with right mouse click or left trigger
		if (Input.GetMouseButton(1) || leftTrigger) {
			GetComponentInChildren<Camera>().fieldOfView -= 200f * Time.deltaTime;
			if (GetComponentInChildren<Camera>().fieldOfView < 30f) {
				GetComponentInChildren<Camera>().fieldOfView = 30f;
			}
		}
		else {
			GetComponentInChildren<Camera>().fieldOfView += 200f * Time.deltaTime;
			if (GetComponentInChildren<Camera>().fieldOfView > 60f) {
				GetComponentInChildren<Camera>().fieldOfView = 60f;
			}
		}
	}


	[RPC]
	//RPC call that creates the projectile on the server side
	public void CreateProjectile (string source, Vector3 position, Quaternion rotation, Vector3 direction, NetworkPlayer np) {
		if (Network.isServer) {
			GameObject projectile = Network.Instantiate (Resources.Load(source), position, rotation, 0) as GameObject;
			projectile.GetComponent<Projectile>().InitVariables (direction.normalized, np);
		}
	}
}
