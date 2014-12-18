using UnityEngine;
using System.Collections;

public class AnimationController : MonoBehaviour {

	//Fields
	public Animation animation;
	public Animator animator;
	PlayerManager playerManager;


	// Use this for initialization
	void Start () {
		playerManager = GetComponent<PlayerManager>();
	}


	// Update is called once per frame
	void Update () {
		switch (playerManager.movementState) {
		case PlayerManager.MovementState.IDLE:
			animator.SetBool ("RunCheck", false);
			animator.SetBool ("directionPress", false);
			animator.SetBool ("jump", false);
			animation.enabled = true;
			animation.Play ("idle");
			break;
		case PlayerManager.MovementState.WALKING:
			animator.SetBool ("RunCheck", false);
			animator.SetBool ("directionPress", true);
			animator.SetBool ("jump", false);
			animation.enabled = true;
			animation.Play ("walk");
			break;
		case PlayerManager.MovementState.RUNNING:
			animator.SetBool ("RunCheck", true);
			animator.SetBool ("directionPress", true);
			animator.SetBool ("jump", false);
			animation.enabled = true;
			animation.Play ("run");
			break;
		case PlayerManager.MovementState.Jumping:
			animation["jump_pose"].wrapMode = WrapMode.Once;
			animator.SetBool ("jump", true);
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
