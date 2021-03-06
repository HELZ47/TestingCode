﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Handles lauching the projectile and its related controls
public class Launch_Projectile : MonoBehaviour {

	#region Fields
	bool rightTriggerDown, rightTriggerReset;
	bool rightTriggerPressedOnce;
	bool leftTrigger, leftBumper, rightBumper;
	bool k1active, k2active, k3active;
	PlayerManager myPlayerManager;
	EnergyManager myEnergyManager;
	#endregion


	#region Initialization
	// Use this for initialization
	void Awake () {
		rightTriggerDown = false;
		rightTriggerReset = true; //Make this true so that first trigger press creates projectile
		leftTrigger = false;
		leftBumper = false; 
		rightBumper = false;
		k1active = false;
		k2active = false;
		k3active = false;
		myPlayerManager = GetComponent<PlayerManager> ();
		myEnergyManager = GetComponent<EnergyManager> ();
	}
	#endregion


	//Update controller status
	void UpdateControllerInput () {
		//On Mac, the controller's L/R triggers are initialized to 0 instead of -1, this fix that issue
		if (Input.GetAxis("Mac_RightTrigger")>0 && rightTriggerPressedOnce == false) {
			rightTriggerPressedOnce = true;
		}
		//Debug: Get the controller's input based on the OS version -----------------------------
		//Mac OS: triggers -1->1  Windows: triggers 0->1
		if (Application.platform == RuntimePlatform.OSXDashboardPlayer ||
		    Application.platform == RuntimePlatform.OSXEditor ||
		    Application.platform == RuntimePlatform.OSXPlayer ||
		    Application.platform == RuntimePlatform.OSXWebPlayer) {
			leftTrigger = Input.GetAxis ("Mac_LeftTrigger") > 0;
			leftBumper = Input.GetButtonDown ("Mac_LeftBumper");
			rightBumper = Input.GetButtonDown ("Mac_RightBumper");
			//Testing if the right trigger has been pulled from its resting position (like getButtonDown)
			if (Input.GetAxis("Mac_RightTrigger") > -1 && rightTriggerDown == false && rightTriggerReset == true &&
			    rightTriggerPressedOnce) {
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
			if (Input.GetAxis("Windows_RightTrigger") > 0 && rightTriggerDown == false && rightTriggerReset == true) {
				rightTriggerDown = true;
			}
			else if (rightTriggerDown && Input.GetAxis("Windows_RightTrigger") > 0) {
				rightTriggerDown = false;
				rightTriggerReset = false;
			}
			else if (rightTriggerReset == false && Input.GetAxis("Windows_RightTrigger") == 0) {
				rightTriggerReset = true;
			}
			//			print ("right trigger down: " + rightTriggerDown);
			//			print ("right trigger reset: " + rightTriggerReset);
			//print ("right trigger value: " + Input.GetAxis ("Windows_RightTrigger"));
		}
		//------------------------------------------------------------------------------------
	}


	void UpdateKbInput () {
		k1active = false;
		k2active = false;
		k3active = false;
		if (Input.GetMouseButtonDown (0)) {
			k1active = true;
		}
		else if (Input.GetKeyDown (KeyCode.Q)) {
			k2active = true;
		}
		else if (Input.GetKeyDown (KeyCode.E)) {
			k3active = true;
		}
	}


	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) { //Skip this function if the object is not the player's
			return;
		}

		UpdateControllerInput ();
		UpdateKbInput ();

		//Mouse Buttons: 0-->Left, 1-->right, 2-->wheel
		//launched: rightTriggerDown or rightBumperDown or leftBumperDown
		bool launched = rightTriggerDown || rightBumper || leftBumper;
		launched = k1active || k2active || k3active;
		//If the corresponding input is pressed, launch the corresponding projectile
		if (launched) {
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
			if (rightTriggerDown || k1active) {
				if (myEnergyManager.SpendEnergy (myPlayerManager.fireBulletCost)) {
					Vector3 direction = (projectileTargetPosition - transform.position).normalized;
					networkView.RPC ("CreateProjectile", RPCMode.AllBuffered, "Prefabs/Fire_Bullet", transform.position+(projectileDirection), new Quaternion(), direction.normalized, Network.player);
				}
			}
			else if (rightBumper || k2active) {
				if (myEnergyManager.SpendEnergy (myPlayerManager.fireGrenadeCost)) {
					Vector3 direction = (projectileTargetPosition - transform.position).normalized;
					networkView.RPC ("CreateProjectile", RPCMode.AllBuffered, "Prefabs/Fire_Grenade", transform.position+(projectileDirection), new Quaternion(), direction.normalized, Network.player);
				}
			}
			else if (leftBumper || k3active) {
				if (myEnergyManager.SpendEnergy (myPlayerManager.fireOrbCost)) {
					Vector3 direction = (projectileTargetPosition - transform.position).normalized;
					networkView.RPC ("CreateProjectile", RPCMode.AllBuffered, "Prefabs/Fire_Orb", transform.position+(projectileDirection), new Quaternion(), direction.normalized, Network.player);
				}
			}
//			//Mouse can only lauch one kind of projectile
//			else {
//				Vector3 direction = (projectileTargetPosition - transform.position).normalized;
//				networkView.RPC ("CreateProjectile", RPCMode.AllBuffered, "Prefabs/Fire_Orb", transform.position+(projectileDirection), new Quaternion(), direction.normalized, Network.player);
//			}
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
			int teamNum = 99;
			if (tag == "Team 1") {
				teamNum = 1;
			}
			else if (tag == "Team 2") {
				teamNum = 2;
			}
			GameObject projectile = Network.Instantiate (Resources.Load(source), position, rotation, 0) as GameObject;
			projectile.GetComponent<Projectile>().InitVariables (direction.normalized, teamNum);
		}
	}
}
