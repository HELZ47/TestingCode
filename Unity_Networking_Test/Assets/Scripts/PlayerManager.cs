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


	// Use this for initialization
	void Start () {
		movementState = MovementState.IDLE;
		mainCamera = GetComponentInChildren<Camera> ();
		powerState = PowerState.Normal;
//		if (networkView.isMine) {
//			activated = true;
//		}
//		else {
//			activated = false;
//		}
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
