using UnityEngine;
using System.Collections;

public class AnimationController : MonoBehaviour {

	//Fields
	//public Animation animation;
	public Animator animator;
	PlayerManager playerManager;
	public int direction, movementType;
	public bool jump;


	// Use this for initialization
	void Start () {
		playerManager = GetComponent<PlayerManager>();
	}

	[RPC]
	void UpdateAnimation (int direction, int movementType, bool jump) {
		animator.SetInteger ("direction", direction);
		animator.SetInteger ("movementType", movementType);
		animator.SetBool ("jump", jump);
	}

	// Update is called once per frame
	void Update () {
//		/*Change the player's animation based on the current movement state and direction
//		 -direction: 0->forward, 1->forward_right, 2->right, 3->backward_right, 4->backward, 5->backward_left,
//		 	6->left, 7->forward_left, 8->NULL
//		 -movementType: 0->Idle, 1->Walk, 2->Run, 3->other*/
//		switch (playerManager.movementState) {
//		case PlayerManager.MovementState.IDLE:
//			//Set the direction to NULL and other bool checks to false
//			animator.SetInteger ("direction", 8);
//			animator.SetInteger ("movementType", 0);
//			animator.SetBool ("jump", false);
//			//Code for the constructor model
//			//animation.enabled = true;
//			//animation.Play ("idle");
//			break;
//		case PlayerManager.MovementState.WALKING:
//			//Set directionPress to true, but other flags to false
//			animator.SetInteger ("movementType", 1);
//			animator.SetBool ("jump", false);
//			//Set the direction flag
//			switch (playerManager.movementDirection) {
//			case PlayerManager.MovementDirection.FORWARD:
//				animator.SetInteger ("direction", 0);
//				break;
//			case PlayerManager.MovementDirection.FORWARD_RIGHT:
//				animator.SetInteger ("direction", 1);
//				break;
//			case PlayerManager.MovementDirection.RIGHT:
//				animator.SetInteger ("direction", 2);
//				break;
//			case PlayerManager.MovementDirection.BACKWARD_RIGHT:
//				animator.SetInteger ("direction", 3);
//				break;
//			case PlayerManager.MovementDirection.BACKWARD:
//				animator.SetInteger ("direction", 4);
//				break;
//			case PlayerManager.MovementDirection.BACKWARD_LEFT:
//				animator.SetInteger ("direction", 5);
//				break;
//			case PlayerManager.MovementDirection.LEFT:
//				animator.SetInteger ("direction", 6);
//				break;
//			case PlayerManager.MovementDirection.FORWARD_LEFT:
//				animator.SetInteger ("direction", 7);
//				break;
//			}
//			//Code for the constructor model
//			//animation.enabled = true;
//			//animation.Play ("walk");
//			break;
//		case PlayerManager.MovementState.RUNNING:
//			//Set directionPress and runcheck to true, but other flags to false
//			animator.SetInteger ("movementType", 2);
//			animator.SetBool ("jump", false);
//			//Set the direction flag
//			switch (playerManager.movementDirection) {
//			case PlayerManager.MovementDirection.FORWARD:
//				animator.SetInteger ("direction", 0);
//				break;
//			case PlayerManager.MovementDirection.FORWARD_RIGHT:
//				animator.SetInteger ("direction", 1);
//				break;
//			case PlayerManager.MovementDirection.RIGHT:
//				animator.SetInteger ("direction", 2);
//				break;
//			case PlayerManager.MovementDirection.BACKWARD_RIGHT:
//				animator.SetInteger ("direction", 3);
//				break;
//			case PlayerManager.MovementDirection.BACKWARD:
//				animator.SetInteger ("direction", 4);
//				break;
//			case PlayerManager.MovementDirection.BACKWARD_LEFT:
//				animator.SetInteger ("direction", 5);
//				break;
//			case PlayerManager.MovementDirection.LEFT:
//				animator.SetInteger ("direction", 6);
//				break;
//			case PlayerManager.MovementDirection.FORWARD_LEFT:
//				animator.SetInteger ("direction", 7);
//				break;
//			}
//			//Code for the constructor model
//			//animation.enabled = true;
//			//animation.Play ("run");
//			break;
//		case PlayerManager.MovementState.Jumping:
//			//Set jump to true, but other flags to false
//			animator.SetInteger ("movementType", 3);
//			animator.SetBool ("jump", true);
//			//Code for the constructor model
//			//animation["jump_pose"].wrapMode = WrapMode.Once;
//			//if (!animation.IsPlaying ("jump_pose")) {
//			//	animation.Play ("jump_pose");
//			//}
//			//if (animation.isPlaying && animation["jump_pose"].time > 0.1f) {
//			//	animation.enabled = false;
//			//}
//		break;
//		}




		if (networkView.isMine) {
//			int direction = 8, movementType = 3;
//			bool jump = false;
			/*Change the player's animation based on the current movement state and direction
			 -direction: 0->forward, 1->forward_right, 2->right, 3->backward_right, 4->backward, 5->backward_left,
			 	6->left, 7->forward_left, 8->NULL
			 -movementType: 0->Idle, 1->Walk, 2->Run, 3->other*/
			switch (playerManager.movementState) {
			case PlayerManager.MovementState.IDLE:
				//Set the direction to NULL and other bool checks to false
				//animator.SetInteger ("direction", 8);
				//animator.SetInteger ("movementType", 0);
				//animator.SetBool ("jump", false);
				direction = 8;
				movementType = 0;
				jump = false;
				//Code for the constructor model
				//animation.enabled = true;
				//animation.Play ("idle");
				break;
			case PlayerManager.MovementState.WALKING:
				//Set directionPress to true, but other flags to false
				//animator.SetInteger ("movementType", 1);
				//animator.SetBool ("jump", false);
				movementType = 1;
				jump = false;
				//Set the direction flag
				switch (playerManager.movementDirection) {
				case PlayerManager.MovementDirection.FORWARD:
					//animator.SetInteger ("direction", 0);
					direction = 0;
					break;
				case PlayerManager.MovementDirection.FORWARD_RIGHT:
					//animator.SetInteger ("direction", 1);
					direction = 1;
					break;
				case PlayerManager.MovementDirection.RIGHT:
					//animator.SetInteger ("direction", 2);
					direction = 2;
					break;
				case PlayerManager.MovementDirection.BACKWARD_RIGHT:
					//animator.SetInteger ("direction", 3);
					direction = 3;
					break;
				case PlayerManager.MovementDirection.BACKWARD:
					//animator.SetInteger ("direction", 4);
					direction = 4;
					break;
				case PlayerManager.MovementDirection.BACKWARD_LEFT:
					//animator.SetInteger ("direction", 5);
					direction = 5;
					break;
				case PlayerManager.MovementDirection.LEFT:
					//animator.SetInteger ("direction", 6);
					direction = 6;
					break;
				case PlayerManager.MovementDirection.FORWARD_LEFT:
					//animator.SetInteger ("direction", 7);
					direction = 7;
					break;
				}
				//Code for the constructor model
				//animation.enabled = true;
				//animation.Play ("walk");
				break;
			case PlayerManager.MovementState.RUNNING:
				//Set directionPress and runcheck to true, but other flags to false
				//animator.SetInteger ("movementType", 2);
				//animator.SetBool ("jump", false);
				movementType = 2;
				jump = false;
				//Set the direction flag
				switch (playerManager.movementDirection) {
				case PlayerManager.MovementDirection.FORWARD:
					//animator.SetInteger ("direction", 0);
					direction = 0;
					break;
				case PlayerManager.MovementDirection.FORWARD_RIGHT:
					//animator.SetInteger ("direction", 1);
					direction = 1;
					break;
				case PlayerManager.MovementDirection.RIGHT:
					//animator.SetInteger ("direction", 2);
					direction = 2;
					break;
				case PlayerManager.MovementDirection.BACKWARD_RIGHT:
					//animator.SetInteger ("direction", 3);
					direction = 3;
					break;
				case PlayerManager.MovementDirection.BACKWARD:
					//animator.SetInteger ("direction", 4);
					direction = 4;
					break;
				case PlayerManager.MovementDirection.BACKWARD_LEFT:
					//animator.SetInteger ("direction", 5);
					direction = 5;
					break;
				case PlayerManager.MovementDirection.LEFT:
					//animator.SetInteger ("direction", 6);
					direction = 6;
					break;
				case PlayerManager.MovementDirection.FORWARD_LEFT:
					//animator.SetInteger ("direction", 7);
					direction = 7;
					break;
				}
				//Code for the constructor model
				//animation.enabled = true;
				//animation.Play ("run");
				break;
			case PlayerManager.MovementState.Jumping:
				//Set jump to true, but other flags to false
				//animator.SetInteger ("movementType", 3);
				//animator.SetBool ("jump", true);
				movementType = 3;
				jump = true;
				//Code for the constructor model
				//animation["jump_pose"].wrapMode = WrapMode.Once;
				//if (!animation.IsPlaying ("jump_pose")) {
				//	animation.Play ("jump_pose");
				//}
				//if (animation.isPlaying && animation["jump_pose"].time > 0.1f) {
				//	animation.enabled = false;
				//}
				break;
			}
			animator.SetInteger ("direction", direction);
			animator.SetInteger ("movementType", movementType);
			animator.SetBool ("jump", jump);
			//networkView.RPC ("UpdateAnimation", RPCMode.All, direction, movementType, jump);
		}
	}
}
