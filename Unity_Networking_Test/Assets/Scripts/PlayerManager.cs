using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour {

	//Fields
	public enum MovementState { IDLE, WALKING, RUNNING, Jumping };
	public MovementState movementState;
	public enum JumpState { ASCENDING, DESCENDING };
	public JumpState jumpState;
	public enum MovementDirection { FORWARD, BACKWARD, LEFT, RIGHT, FORWARD_LEFT, FORWARD_RIGHT, BACKWARD_LEFT, BACKWARD_RIGHT }
	public MovementDirection movementDirection;
	public enum PowerState { Normal, Boost };
	public PowerState powerState;
	public Camera mainCamera;
	public float particleSize;
	//public bool activated;
	MovementController myMovementController;
	AnimationController myAnimationController;


	// Use this for initialization
	void Awake () {
		movementState = MovementState.IDLE;
		mainCamera = GetComponentInChildren<Camera> ();
		powerState = PowerState.Normal;
		myMovementController = GetComponent<MovementController> ();
		myAnimationController = GetComponent<AnimationController> ();
	}


	//State synchronization stuff
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		Vector3 playerForward = Vector3.zero;
		Vector3 position = Vector3.zero;
		Vector3 rbVelocity = Vector3.zero;
		int direction = 0, movementType = 0;
		bool jump = false;
		float partSize = 0f;
		if (stream.isWriting) {
			//Sending
			playerForward = myMovementController.playerTransform.forward;
			position = transform.position;
			rbVelocity = rigidbody.velocity;
			direction = myAnimationController.direction;
			movementType = myAnimationController.movementType;
			jump = myAnimationController.jump;
			partSize = particleSize;
			stream.Serialize(ref playerForward);
			stream.Serialize (ref position);
			stream.Serialize (ref rbVelocity);
			stream.Serialize (ref direction);
			stream.Serialize (ref movementType);
			stream.Serialize (ref jump);
			stream.Serialize (ref partSize);
		} else {
			//Receiving
			stream.Serialize(ref playerForward);
			stream.Serialize (ref position);
			stream.Serialize (ref rbVelocity);
			stream.Serialize (ref direction);
			stream.Serialize (ref movementType);
			stream.Serialize (ref jump);
			stream.Serialize (ref partSize);
			myMovementController.playerTransform.forward = playerForward;
			transform.position = position;
			rigidbody.velocity = rbVelocity;
			myAnimationController.animator.SetInteger ("direction", direction);
			myAnimationController.animator.SetInteger ("movementType", movementType);
			myAnimationController.animator.SetBool ("jump", jump);
			particleSize = partSize;
		}
	}


	// Update is called once per frame
	void Update () {
		//if (networkView.isMine) {
			ParticleSystem projectParticles = GetComponent<ParticleSystem>();
			//Debug----------------------------
			ParticleSystem.Particle[] particles = new ParticleSystem.Particle[projectParticles.particleCount+1];
			int numOfParticles = projectParticles.GetParticles (particles);
			Vector3 pVelocity = rigidbody.velocity * -1f;
			if (pVelocity.magnitude > particleSize) {
				pVelocity = pVelocity.normalized * particleSize;
			}
			if (pVelocity.magnitude < 1f) {
				pVelocity = new Vector3 (0, particleSize, 0);
			}
			int i = 0;
			while (i<numOfParticles) {
				particles[i].velocity = pVelocity;
				i++;
			}
			projectParticles.SetParticles (particles, numOfParticles);
			//---------------------------------
		//}
	}
}
