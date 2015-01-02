using UnityEngine;
using System.Collections;

//Handles the bot's animation states and transitions
public class BotAnimator : MonoBehaviour {

	#region Fields
	[HideInInspector]
	public bool isMoving, takingDamage, isDead, isAttacking;
	Animator myAnimator;
	BotManager myBotManager;
	HealthManager myHealthManager;
	NavMeshAgent myNavMeshAgent;
	bool rbEnabled;
	#endregion


	// Use this for initialization
	void Awake () {
		myAnimator = GetComponent<Animator> ();
		myBotManager = GetComponent<BotManager> ();
		myHealthManager = GetComponent<HealthManager> ();
		myNavMeshAgent = GetComponent<NavMeshAgent> ();
	}

//	[RPC]
//	void UpdateAnimation (bool isMoving, bool takingDamage, bool isDead, bool isAttacking) {
//		myAnimator.SetBool ("isMoving", isMoving);
//		myAnimator.SetBool ("takingDamage", takingDamage);
//		myAnimator.SetBool ("isDead", isDead);
//		myAnimator.SetBool ("isAttacking", isAttacking);
//	}

	// Update is called once per frame
	void Update () {
		if (networkView.isMine) {
			rbEnabled = !rigidbody.isKinematic;

			//Movement animations
			if (rbEnabled) {
				Vector3 rigidSpeed = rigidbody.velocity - new Vector3 (0, rigidbody.velocity.y, 0);
				if (rigidSpeed.magnitude < 0.25f) {
					myAnimator.SetBool ("isMoving", false);
					isMoving = false;
				}
				else {
					myAnimator.SetBool ("isMoving", true);
					isMoving = true;
				}
			}
			else {
				if (myNavMeshAgent.velocity.magnitude < 0.25f) {
					myAnimator.SetBool ("isMoving", false);
					isMoving = false;
				}
				else {
					myAnimator.SetBool ("isMoving", true);
					isMoving = true;
				}
			}

			//Attacking animation
			if (myBotManager.isAttacking &&
			    !myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
				myAnimator.SetBool ("isAttacking", true);
				isAttacking = true;
			}
			else if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
				//myBotManager.isAttacking = false;
				myAnimator.SetBool ("isAttacking", false);
				isAttacking = false;
			}

			//Death animation
			if (myHealthManager.isDead && !myHealthManager.deathAnimationFinished) {
				myAnimator.SetBool ("isDead", true);
				isDead = true;
				if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Death") &&
				    myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f) {
					myHealthManager.deathAnimationFinished = true;
				}
			}

			//Taking DamageAnimation
//			if (myHealthManager.isTakingDamage) {
//				if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("TakingDamage")) {
//					myAnimator.SetBool ("takingDamage", false);
//					takingDamage = false;
//					myHealthManager.isTakingDamage = false;
//				}
//				else {
//					GetComponent<Animator> ().SetBool ("takingDamage", true);
//					takingDamage = true;
//				}
//			}
			takingDamage = false;
			//networkView.RPC("UpdateAnimation", RPCMode.All, isMoving, takingDamage, isDead, isAttacking);
		}
	}
}
