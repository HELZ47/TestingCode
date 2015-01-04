using UnityEngine;
using System.Collections;

public class MovementController : MonoBehaviour {

	#region Fields
	//Adjustable
	public Transform playerTransform;
	public float maxWalkingSpeed, maxRunningSpeed, maxMidAirSpeed;
	public float walkingAcceleration, runningAcceleration, jumpImpulse, midAirAcceleration;
	//Not Adjustable
	[HideInInspector]
	public Camera mainCamera;
	PlayerManager playerManager;
	Vector3 directionVector;
	bool moveForward, moveBackward, moveLeft, moveRight, isControllerRunning, isControllerJumping;
	bool isKbRunning, isKbJumping;
	#endregion

	#region Initialization
	// Initializa internal variables
	void Awake () {
		mainCamera = GetComponentInChildren<Camera>();
		playerManager = GetComponent<PlayerManager>();
		moveForward = false;
		moveBackward = false;
		moveLeft = false;
		moveRight = false;
		isControllerRunning = false;
		isControllerJumping = false;
		isKbRunning = false;
		isKbJumping = false;
	}
	#endregion
	

	// Update is called once per frame
	void Update () {
		//Skip the rest if this player is not mine
		if (!GetComponentInParent<NetworkView>().isMine) {
			return;	
		}
		CheckInput();
		//print ("current speed: " + rigidbody.velocity.magnitude);
		//print ("directionVector: " + directionVector);
	}


	// Fixed Update for physics objects and rigidbody interactions and character rotation
	void FixedUpdate () {
		//Skip the rest if this player is not mine
		if (!GetComponentInParent<NetworkView>().isMine) {
			return;
		}

		//Debug if the player is dashing, go no further-----------------------------------------------------------
		if (playerManager.powerState != PlayerManager.PowerState.Normal) {
			return;
		}
		//----------------------------------------------------------------

		/*If the player is moving, rotate the player object slowly towards the direction of the camera*/
		if (playerManager.movementState == PlayerManager.MovementState.WALKING || playerManager.movementState == PlayerManager.MovementState.RUNNING ||
		    playerManager.movementState == PlayerManager.MovementState.Jumping) {
			/*If the player is moving forward, slerp to its own forward position*/
			if (playerManager.movementDirection == PlayerManager.MovementDirection.FORWARD ||
			    playerManager.movementDirection == PlayerManager.MovementDirection.FORWARD_LEFT ||
			    playerManager.movementDirection == PlayerManager.MovementDirection.FORWARD_RIGHT) {
				Vector3 forwardXZ = new Vector3 (directionVector.x, 0, directionVector.z);
				playerTransform.forward = Vector3.Slerp (playerTransform.forward, forwardXZ, 0.1f);
			}
			/*If the player not moving forward, slerp towards the direction of the camera*/
			else {
				Vector3 forwardXZ = new Vector3 (mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
				playerTransform.forward = Vector3.Slerp (playerTransform.forward, forwardXZ, 0.1f);
			}
		}

		//Move the character according to its movement state
		switch (playerManager.movementState) {
		case PlayerManager.MovementState.IDLE:
			/*If the current speed is very slow, stop immediately*/
			if (rigidbody.velocity.magnitude < 0.5f) {
				rigidbody.velocity = new Vector3 (0, rigidbody.velocity.y, 0);
			}
			/*If the current speed is still significant, slow it down*/
			else {
				//This is not the best implementation, can do better
				rigidbody.velocity = new Vector3 (rigidbody.velocity.x * 0.85f, rigidbody.velocity.y, rigidbody.velocity.z * 0.85f);
			}
			break;
		case PlayerManager.MovementState.WALKING:
			/*Perform walking calculation, but cap the max walking speed without changing the gravity*/
			rigidbody.AddForce (directionVector.normalized * walkingAcceleration, ForceMode.Acceleration);
			if (rigidbody.velocity.magnitude > maxWalkingSpeed) {
				Vector3 targetVelocity = directionVector.normalized * maxWalkingSpeed;
				rigidbody.velocity = new Vector3 (targetVelocity.x, rigidbody.velocity.y, targetVelocity.z);
			}
			break;
		case PlayerManager.MovementState.RUNNING:
			/*Perform running acceleration on rigidbody, but cap the max running speed without changing the gravity*/
			rigidbody.AddForce (directionVector.normalized * runningAcceleration, ForceMode.Acceleration);
			if (rigidbody.velocity.magnitude > maxRunningSpeed) {
				Vector3 targetVelocity = directionVector.normalized * maxRunningSpeed;
				rigidbody.velocity = new Vector3 (targetVelocity.x, rigidbody.velocity.y, targetVelocity.z);
			}
			break;
		case PlayerManager.MovementState.Jumping:
			//Ascend-->goes up, Descend-->goes down
			if (playerManager.jumpState == PlayerManager.JumpState.ASCENDING) {
				//print ((rigidbody.velocity - new Vector3 (0, rigidbody.velocity.y, 0)).magnitude);
				if ((rigidbody.velocity - new Vector3 (0, rigidbody.velocity.y, 0)).magnitude < maxWalkingSpeed*1.5f) {
					maxMidAirSpeed = maxWalkingSpeed;
				}
				else  {
					maxMidAirSpeed = maxRunningSpeed;
				}  
				rigidbody.AddForce (Vector3.up * jumpImpulse, ForceMode.Impulse);
				playerManager.jumpState = PlayerManager.JumpState.DESCENDING;
			}
			//Needs better implementation, can jump if around the edge of the wall
			else if (playerManager.jumpState == PlayerManager.JumpState.DESCENDING) {
				CapsuleCollider capCol = GetComponent<CapsuleCollider>();
				BoxCollider boxCol = GetComponent<BoxCollider>();
				foreach (Collider col in Physics.OverlapSphere (transform.position, capCol.height/2f)) {
					if ((col.tag == "Ground" || col.tag == "Environment") && boxCol.bounds.Intersects (col.bounds)) {
						playerManager.movementState = PlayerManager.MovementState.IDLE;
					}
				}
				if (playerManager.movementState == PlayerManager.MovementState.Jumping) {
					rigidbody.AddForce (directionVector.normalized * midAirAcceleration, ForceMode.Acceleration);
					Vector2 XZVelocity = new Vector2 (rigidbody.velocity.x, rigidbody.velocity.z);
					if (XZVelocity.magnitude > maxMidAirSpeed) {
						XZVelocity = XZVelocity.normalized * maxMidAirSpeed;
						rigidbody.velocity = new Vector3 (XZVelocity.x, rigidbody.velocity.y, XZVelocity.y);
					}
				}
			}
			break;
		}
	}


	//Update controller's input
	void UpdateControllerInput () {
		//Debug: Detecting controller input depending on the platform the game's in -----------------------------
		if (Application.platform == RuntimePlatform.OSXDashboardPlayer ||
		    Application.platform == RuntimePlatform.OSXEditor ||
		    Application.platform == RuntimePlatform.OSXPlayer ||
		    Application.platform == RuntimePlatform.OSXWebPlayer) {
			moveForward = Input.GetAxis("Mac_LeftYAxis") > 0.35f;
			moveBackward = Input.GetAxis("Mac_LeftYAxis") < -0.35f;
			moveLeft = Input.GetAxis("Mac_LeftXAxis") < -0.35f;
			moveRight = Input.GetAxis("Mac_LeftXAxis") > 0.35f;
			isControllerRunning = new Vector2 (Input.GetAxis ("Mac_LeftXAxis"), Input.GetAxis("Mac_LeftYAxis")).magnitude > 0.85f;
			isControllerJumping = Input.GetButtonDown("Mac_A");
		}
		else if (Application.platform == RuntimePlatform.WindowsEditor ||
		         Application.platform == RuntimePlatform.WindowsPlayer ||
		         Application.platform == RuntimePlatform.WindowsWebPlayer) {
			moveForward = Input.GetAxis("Windows_LeftYAxis") > 0.35f;
			moveBackward = Input.GetAxis("Windows_LeftYAxis") < -0.35f;
			moveLeft = Input.GetAxis("Windows_LeftXAxis") < -0.35f;
			moveRight = Input.GetAxis("Windows_LeftXAxis") > 0.35f;
			isControllerRunning = new Vector2 (Input.GetAxis ("Windows_LeftXAxis"), Input.GetAxis("Windows_LeftYAxis")).magnitude > 0.85f;
			isControllerJumping = Input.GetButtonDown("Windows_A");
		}
		//------------------------------------------------------------------------------------
	}


	//Update keyboard's input
	void UpdateKeyboardInput () {
		moveForward = Input.GetKey (KeyCode.W);
		moveBackward = Input.GetKey (KeyCode.S);
		moveLeft = Input.GetKey (KeyCode.A);
		moveRight = Input.GetKey (KeyCode.D);
		if (Input.GetKey (KeyCode.LeftControl)) {
			isControllerRunning = false;
		}
		else {
			isControllerRunning = true;
		}
		//isControllerRunning = new Vector2 (Input.GetAxis ("Mac_LeftXAxis"), Input.GetAxis("Mac_LeftYAxis")).magnitude > 0.85f;
		isKbJumping = Input.GetKeyDown (KeyCode.Space);
	}


	//Check the keyboard input and update the player accordingly
	void CheckInput () {

		//Initialize some needed variables
		Vector3 forwardXZ = new Vector3 (mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
		Vector3 rightXZ = new Vector3 (mainCamera.transform.right.x, 0, mainCamera.transform.right.z);
		int keysPressed = 0;
		Vector3 nextDirectionVector = new Vector3 ();

		//Update the controller input by updating the flags
		UpdateControllerInput ();

		//Update the keyboard input with the flags
		UpdateKeyboardInput ();

		//Update the player's movement state based on the flags
		if (moveForward) {
			keysPressed++;
			nextDirectionVector += forwardXZ;
			playerManager.movementDirection = PlayerManager.MovementDirection.FORWARD;
		}
		if (moveBackward) {
			keysPressed++;
			nextDirectionVector -= forwardXZ;
			playerManager.movementDirection = PlayerManager.MovementDirection.BACKWARD;
		}
		if (moveLeft) {
			keysPressed++;
			nextDirectionVector -= rightXZ;
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
		if (moveRight) {
			keysPressed++;
			nextDirectionVector += rightXZ;
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
		//If the player is not jumping, check for run/walk status
		if (keysPressed > 0 && playerManager.movementState != PlayerManager.MovementState.Jumping) {
			directionVector = nextDirectionVector;
			if (isKbRunning|| isControllerRunning) {
				playerManager.movementState = PlayerManager.MovementState.RUNNING;
			}
			else {
				playerManager.movementState = PlayerManager.MovementState.WALKING;
			}
		}
		//If the player is not moving nor jumping, make it idle
		else if (playerManager.movementState!= PlayerManager.MovementState.Jumping){
			playerManager.movementState = PlayerManager.MovementState.IDLE;
			directionVector = Vector3.zero;
		}
		//If the player is jumping, moving around change its direction a bit
		else if (playerManager.movementState == PlayerManager.MovementState.Jumping) {
			directionVector = directionVector + nextDirectionVector.normalized*0.2f;
		}

		//Detect jumping
		if ((isKbJumping|| isControllerJumping) && playerManager.movementState != PlayerManager.MovementState.Jumping) {
			directionVector = nextDirectionVector;
			playerManager.movementState = PlayerManager.MovementState.Jumping;
			playerManager.jumpState = PlayerManager.JumpState.ASCENDING;
		}
	}
}
