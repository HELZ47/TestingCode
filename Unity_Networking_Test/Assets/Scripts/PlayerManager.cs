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
	//public bool activated;


	// Use this for initialization
	void Start () {
		movementState = MovementState.IDLE;
//		if (networkView.isMine) {
//			activated = true;
//		}
//		else {
//			activated = false;
//		}
	}


	// Update is called once per frame
	void Update () {
	
	}
}
