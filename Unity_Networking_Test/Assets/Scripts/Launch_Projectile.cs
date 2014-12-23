using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Launch_Projectile : MonoBehaviour {

	//Fields
	bool rightTriggerDown, rightTriggerReset;
	List<GameObject> launchedProjectiles;
	int ProjectileGroup;


	// Use this for initialization
	void Start () {
		rightTriggerDown = false;
		rightTriggerReset = false;
		launchedProjectiles = new List<GameObject> ();
	}


	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) {
			return;
		}
		ProjectileGroup = Random.Range (10000, 99999);
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
		//Mouse Buttons: 0-->Left, 1-->right, 2-->wheel
		bool launched = rightTriggerDown || Input.GetButtonDown ("Mac_RightBumper") ||
			Input.GetButtonDown ("Mac_LeftBumper");
		if (Input.GetMouseButtonDown (0) || launched) {


			ThirdPersonCamera TPCamera = GetComponentInChildren<ThirdPersonCamera>();
			Transform targetTransform = TPCamera.cameraTarget.transform;

			Vector3 projectileDirection = (targetTransform.position - TPCamera.transform.position).normalized;
			Vector3 projectionStartPos = targetTransform.position + projectileDirection;
			RaycastHit rayHit;

			Vector3 projectileTargetPosition;
			if (Physics.Raycast (projectionStartPos, projectileDirection, out rayHit)) {
				projectileTargetPosition = rayHit.point;	
				//print ("Target Aquired!");
			}
		
			else {
				projectileTargetPosition = projectionStartPos + (projectileDirection*100f);
				//print ("Target Too Far!");
			}

			GameObject fireBall;
			if (rightTriggerDown) {
				//fireBall =  Network.Instantiate (Resources.Load("Prefabs/Fire_Bullet"), transform.position+(projectileDirection), new Quaternion(), 0) as GameObject;
				Vector3 direction = (projectileTargetPosition - transform.position).normalized;
				networkView.RPC ("CreateProjectile", RPCMode.AllBuffered, "Prefabs/Fire_Bullet", transform.position+(projectileDirection), new Quaternion(), direction.normalized, Network.player);
			}
			else if (Input.GetButtonDown ("Mac_RightBumper")) {
//				fireBall =  Network.Instantiate (Resources.Load("Prefabs/Fire_Grenade"), transform.position+(projectileDirection), new Quaternion(), 0) as GameObject;
				Vector3 direction = (projectileTargetPosition - transform.position).normalized;
				networkView.RPC ("CreateProjectile", RPCMode.AllBuffered, "Prefabs/Fire_Grenade", transform.position+(projectileDirection), new Quaternion(), direction.normalized, Network.player);
			}
			else if (Input.GetButtonDown ("Mac_LeftBumper")) {
//				fireBall =  Network.Instantiate (Resources.Load("Prefabs/Fire_Orb"), transform.position+(projectileDirection), new Quaternion(), 0) as GameObject;
				Vector3 direction = (projectileTargetPosition - transform.position).normalized;
				networkView.RPC ("CreateProjectile", RPCMode.AllBuffered, "Prefabs/Fire_Orb", transform.position+(projectileDirection), new Quaternion(), direction.normalized, Network.player);
			}
			else {
				//fireBall =  Network.Instantiate (Resources.Load("Prefabs/Fire_Orb"), transform.position+(projectileDirection), new Quaternion(), ProjectileGroup) as GameObject;
				Vector3 direction = (projectileTargetPosition - transform.position).normalized;
				networkView.RPC ("CreateProjectile", RPCMode.AllBuffered, "Prefabs/Fire_Orb", transform.position+(projectileDirection), new Quaternion(), direction.normalized, Network.player);
			}
			//launchedProjectiles.Add (fireBall.gameObject);
			////Vector3 direction = GetComponentInChildren<ThirdPersonCamera>().cameraTarget.transform.position - GetComponentInChildren<ThirdPersonCamera>().transform.position;
			//Vector3 direction = (projectileTargetPosition - transform.position).normalized;
			//fireBall.GetComponent<Projectile>().InitVariables (direction.normalized, Network.player, ProjectileGroup);
		}

		if (Input.GetMouseButton(1) || Input.GetAxis("Mac_LeftTrigger") > 0) {
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

//		GameObject[] projectilesTBR = new GameObject[launchedProjectiles.Count];
//		int i = 0;
//		foreach (GameObject gObject in launchedProjectiles) {
//			if (gObject.GetComponent<Projectile>().removeRPCs = true) {
//				Network.RemoveRPCs (gObject.GetComponent<NetworkView>().viewID);
//				projectilesTBR[i] = gObject;
//				i++;
//			}
//		}
//		for (int j = 0; j < projectilesTBR.Length; j++) {
//			launchedProjectiles.Remove (projectilesTBR[j]);
//		}
	}

	[RPC]
	public void CreateProjectile (string source, Vector3 position, Quaternion rotation, Vector3 direction, NetworkPlayer np) {
		if (Network.isServer) {
			GameObject projectile = Network.Instantiate (Resources.Load(source), position, rotation, 0) as GameObject;
			projectile.GetComponent<Projectile>().InitVariables (direction.normalized, np);
		}
	}
}
