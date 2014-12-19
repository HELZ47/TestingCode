using UnityEngine;
using System.Collections;

public class MovementController : MonoBehaviour {

	//Fields
	public Camera mainCamera;
	public Transform playerTransform;
	public float maxWalkingSpeed, maxRunningSpeed;
	public float walkingAcceleration, runningAcceleration, jumpImpulse;
	PlayerManager playerManager;
	Vector3 directionVector;

	// Use this for initialization
	void Start () {
		mainCamera = GetComponentInChildren<Camera>();
		playerManager = GetComponent<PlayerManager>();
	}
	

	// Update is called once per frame
	void Update () {
		if (!GetComponentInParent<NetworkView>().isMine) {
			//enabled = false;
			return;
		}
		CheckInput();
		//print ("current speed: " + rigidbody.velocity.magnitude);
		//print ("directionVector: " + directionVector);
	}


	// Fixed Update for physics objects and rigidbody interactions
	void FixedUpdate () {
		if (!GetComponentInParent<NetworkView>().isMine) {
			//enabled = false;
			return;
		}
//		if (!playerManager.activated) {
//			return;
//		}
		/*If the player is moving, rotate the player object slowly towards the direction of the camera*/
		if (playerManager.movementState == PlayerManager.MovementState.WALKING || playerManager.movementState == PlayerManager.MovementState.RUNNING ||
		    playerManager.movementState == PlayerManager.MovementState.Jumping) {
			if (playerManager.movementDirection == PlayerManager.MovementDirection.FORWARD ||
			    playerManager.movementDirection == PlayerManager.MovementDirection.FORWARD_LEFT ||
			    playerManager.movementDirection == PlayerManager.MovementDirection.FORWARD_RIGHT) {
				Vector3 forwardXZ = new Vector3 (directionVector.x, 0, directionVector.z);
				//print ("Slerping! forwardXZ: " + forwardXZ);
				playerTransform.forward = Vector3.Slerp (playerTransform.forward, forwardXZ, 0.1f);
			}
			else {
				Vector3 forwardXZ = new Vector3 (mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
				playerTransform.forward = Vector3.Slerp (playerTransform.forward, forwardXZ, 0.1f);
			}
		}


		switch (playerManager.movementState) {
		case PlayerManager.MovementState.IDLE:
			/*If the current speed is very slow, stop immediately*/
			if (rigidbody.velocity.magnitude < 0.5f) {
				rigidbody.velocity = new Vector3 (0, rigidbody.velocity.y, 0);
			}
			/*If the current speed is still significant, slow it down*/
			else {
				rigidbody.velocity = new Vector3 (rigidbody.velocity.x * 0.85f, rigidbody.velocity.y, rigidbody.velocity.z * 0.85f);
			}
			break;
		case PlayerManager.MovementState.WALKING:
			/*If the previous speed is faster than max walking speed (1.35 times), means that the player was running, 
			therefore it needs to slow down to max walking speed*/
			if (rigidbody.velocity.magnitude / maxWalkingSpeed > 1.35f) {
				//print ("Oh no!!!!");
				//rigidbody.AddForce (directionVector.normalized * (-runningAcceleration), ForceMode.Acceleration);
//				if (rigidbody.velocity.magnitude < maxWalkingSpeed) {
//					Vector3 targetVelocity = directionVector.normalized * maxWalkingSpeed;
//					rigidbody.velocity = new Vector3 (targetVelocity.x, rigidbody.velocity.y, targetVelocity.z);
//				}
			}
			/*Otherwise, perform normal walking calculation, but cap the max walking speed*/
			else {
				//print ("Even worse!!!!");
				rigidbody.AddForce (directionVector.normalized * walkingAcceleration, ForceMode.Acceleration);
				if (rigidbody.velocity.magnitude > maxWalkingSpeed) {
					Vector3 targetVelocity = directionVector.normalized * maxWalkingSpeed;
					rigidbody.velocity = new Vector3 (targetVelocity.x, rigidbody.velocity.y, targetVelocity.z);
				}
			}
			break;
		case PlayerManager.MovementState.RUNNING:
			/*Perform running acceleration on rigidbody, but cap the max running speed*/
			rigidbody.AddForce (directionVector.normalized * runningAcceleration, ForceMode.Acceleration);
			if (rigidbody.velocity.magnitude > maxRunningSpeed) {
				Vector3 targetVelocity = directionVector.normalized * maxRunningSpeed;
				rigidbody.velocity = new Vector3 (targetVelocity.x, rigidbody.velocity.y, targetVelocity.z);
			}
			break;
		case PlayerManager.MovementState.Jumping:
			if (playerManager.jumpState == PlayerManager.JumpState.ASCENDING) {
				rigidbody.AddForce (Vector3.up * jumpImpulse, ForceMode.Impulse);
				playerManager.jumpState = PlayerManager.JumpState.DESCENDING;
			}
			else if (playerManager.jumpState == PlayerManager.JumpState.DESCENDING) {
				if (GetComponent<CapsuleCollider>().bounds.Intersects (GameObject.FindGameObjectWithTag("Ground").GetComponent<MeshCollider>().bounds)) {
					playerManager.movementState = PlayerManager.MovementState.IDLE;
					print ("collide with ground!");
				}
			}
			break;
		}
	}


	//Check the keyboard input and update the player accordingly
	void CheckInput () {

		Vector3 forwardXZ = new Vector3 (mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
		Vector3 rightXZ = new Vector3 (mainCamera.transform.right.x, 0, mainCamera.transform.right.z);
		int keysPressed = 0;
		directionVector = new Vector3 ();

		if (Input.GetKey(KeyCode.W)) {
			keysPressed++;
			directionVector += forwardXZ;
			playerManager.movementDirection = PlayerManager.MovementDirection.FORWARD;
		}
		if (Input.GetKey(KeyCode.S)) {
			keysPressed++;
			directionVector -= forwardXZ;
			playerManager.movementDirection = PlayerManager.MovementDirection.BACKWARD;
		}
		if (Input.GetKey(KeyCode.A)) {
			keysPressed++;
			directionVector -= rightXZ;
			if (playerManager.movementDirection == PlayerManager.MovementDirection.FORWARD) {
				playerManager.movementDirection = PlayerManager.MovementDirection.FORWARD_LEFT;
			}
			else if (playerManager.movementDirection == PlayerManager.MovementDirection.BACKWARD) {
				playerManager.movementDirection = PlayerManager.MovementDirection.BACKWARD_LEFT;
			}
			else {
				playerManager.movementDirection = PlayerManager.MovementDirection.LEFT;
			}
		}
		if (Input.GetKey(KeyCode.D)) {
			keysPressed++;
			directionVector += rightXZ;
			if (playerManager.movementDirection == PlayerManager.MovementDirection.FORWARD) {
				playerManager.movementDirection = PlayerManager.MovementDirection.FORWARD_RIGHT;
			}
			else if (playerManager.movementDirection == PlayerManager.MovementDirection.BACKWARD) {
				playerManager.movementDirection = PlayerManager.MovementDirection.BACKWARD_RIGHT;
			}
			else {
				playerManager.movementDirection = PlayerManager.MovementDirection.RIGHT;
			}
		}


		if (keysPressed > 0 && playerManager.movementState != PlayerManager.MovementState.Jumping) {
			if (Input.GetKey(KeyCode.LeftShift)) {
				playerManager.movementState = PlayerManager.MovementState.RUNNING;
			}
			else {
				playerManager.movementState = PlayerManager.MovementState.WALKING;
			}
		}
		else if (playerManager.movementState!= PlayerManager.MovementState.Jumping){
			playerManager.movementState = PlayerManager.MovementState.IDLE;
		}

		
		if (Input.GetKeyDown (KeyCode.Space) && playerManager.movementState != PlayerManager.MovementState.Jumping) {
			print("jump!");
			playerManager.movementState = PlayerManager.MovementState.Jumping;
			playerManager.jumpState = PlayerManager.JumpState.ASCENDING;
//			rigidbody.AddForce (Vector3.up * jumpImpulse, ForceMode.Impulse);
		}
	}
}
