using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour {

	//Fields
	public enum MovementState { IDLE, WALKING, RUNNING, Jumping };
	public MovementState movementState;
	public enum JumpState { ASCENDING, DESCENDING };
	public JumpState jumpState;


	// Use this for initialization
	void Start () {
		movementState = MovementState.IDLE;
	}


	// Update is called once per frame
	void Update () {
	
	}
}
