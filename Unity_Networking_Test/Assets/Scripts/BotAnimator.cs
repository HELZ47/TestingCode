using UnityEngine;
using System.Collections;

public class BotAnimator : MonoBehaviour {

	//Fields
	Animator myAnimator;
	BotManager myBotManager;
	HealthManager myHealthManager;

	// Use this for initialization
	void Awake () {
		myAnimator = GetComponent<Animator> ();
		myBotManager = GetComponent<BotManager> ();
		myHealthManager = GetComponent<HealthManager> ();
	}
	
	// Update is called once per frame
	void Update () {
		//Movement animations
		Vector3 rigidSpeed = rigidbody.velocity - new Vector3 (0, rigidbody.velocity.y, 0);
		if (rigidSpeed.magnitude < 0.1f) {
			myAnimator.SetBool ("isMoving", false);
		}
		else {
			myAnimator.SetBool ("isMoving", true);
		}

		//Attacking animation
		if (myBotManager.isAttacking &&
		    !myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
			myAnimator.SetBool ("isAttacking", true);
			//myBotManager.isAttacking = false;
		}
		else if (myBotManager.isAttacking == false) {
			myAnimator.SetBool ("isAttacking", false);
		}

		//Death animation
		if (myHealthManager.isDead && !myHealthManager.deathAnimationFinished) {
			myAnimator.SetBool ("isDead", true);
			if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Death") &&
			    myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f) {
				myHealthManager.deathAnimationFinished = true;
			}
		}

		//Taking DamageAnimation
		if (myHealthManager.isTakingDamage) {
			if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("TakingDamage")) {
				myAnimator.SetBool ("takingDamage", false);
				myHealthManager.isTakingDamage = false;
			}
			else {
				GetComponent<Animator> ().SetBool ("takingDamage", true);
			}
		}
	}
}
