using UnityEngine;
using System.Collections;

public class AnimationController : MonoBehaviour {

	//Fields
	public Animation animation;
	PlayerManager playerManager;


	// Use this for initialization
	void Start () {
		playerManager = GetComponent<PlayerManager>();
	}


	// Update is called once per frame
	void Update () {
		switch (playerManager.movementState) {
		case PlayerManager.MovementState.IDLE:
			animation.enabled = true;
			animation.Play ("idle");
			break;
		case PlayerManager.MovementState.WALKING:
			animation.enabled = true;
			animation.Play ("walk");
			break;
		case PlayerManager.MovementState.RUNNING:
			animation.enabled = true;
			animation.Play ("run");
			break;
		case PlayerManager.MovementState.Jumping:
			animation["jump_pose"].wrapMode = WrapMode.Once;
			//if (!animation.isPlaying) {
			if (!animation.IsPlaying ("jump_pose")) {
				animation.Play ("jump_pose");
			}
			if (animation.isPlaying && animation["jump_pose"].time > 0.1f) {
				animation.enabled = false;
			}
			//}


		break;
	}
}
}
