using UnityEngine;
using System.Collections;

public class Launch_Projectile : MonoBehaviour {

	//Fields


	// Use this for initialization
	void Start () {
	
	}


	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) {
			return;
		}
		//Mouse Buttons: 0-->Left, 1-->right, 2-->wheel
		if (Input.GetMouseButtonDown (0)) {
			print ("Launching Projectile");

			ThirdPersonCamera TPCamera = GetComponentInChildren<ThirdPersonCamera>();
			Transform targetTransform = TPCamera.cameraTarget.transform;

			Vector3 projectileDirection = (targetTransform.position - TPCamera.transform.position).normalized;
			Vector3 projectionStartPos = targetTransform.position + projectileDirection;
			RaycastHit rayHit;

			Vector3 projectileTargetPosition;
			if (Physics.Raycast (projectionStartPos, projectileDirection, out rayHit)) {
				projectileTargetPosition = rayHit.point;	
			}
		
			else {
				projectileTargetPosition = projectionStartPos + (projectileDirection*100f);
			}

			GameObject fireBall =  Network.Instantiate (Resources.Load("Prefabs/Fire_Bullet"), transform.position+(projectileDirection), new Quaternion(), 0) as GameObject;
			//Vector3 direction = GetComponentInChildren<ThirdPersonCamera>().cameraTarget.transform.position - GetComponentInChildren<ThirdPersonCamera>().transform.position;
			Vector3 direction = (projectileTargetPosition - transform.position).normalized;
			fireBall.GetComponent<Projectile>().InitVariables (direction.normalized, Network.player);
		}

		if (Input.GetMouseButton(1)) {
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
}
