using UnityEngine;
using System.Collections;

public class MobAnimationController : MonoBehaviour {

	//Fields
	float speed;
	bool investigate, chase, isAttacking, receiveDamage, isDead;
	Animator myAnimator;
	MobMovement myMobMovement;


	// Use this for initialization
	void Awake () {
		myAnimator = GetComponent<Animator> ();
		myMobMovement = GetComponent<MobMovement> ();
	}

	[RPC]
	void UpdateAnimation (bool investigate, bool chase, bool isAttacking, bool receiveDamage, bool isDead) {
		myAnimator.SetBool ("investigate", true);
//		myAnimator.SetBool ("chase", chase);
//		myAnimator.SetBool ("isAttacking", isAttacking);
//		myAnimator.SetBool ("receiveDamage", receiveDamage);
//		myAnimator.SetBool ("isDead", isDead);
	}

	[RPC]
	void UpdateInvestigate (bool investigate) {
		myAnimator.SetBool ("investigate", investigate);
	}
	[RPC]
	void UpdateChase (bool chase) {
		myAnimator.SetBool ("chase", chase);
	}
	[RPC]
	void UpdateIsAttacking (bool isAttacking) {
		myAnimator.SetBool ("isAttacking", isAttacking);
	}
	[RPC]
	void UpdateReceiveDamage (bool receiveDamage) {
		myAnimator.SetBool ("receiveDamage", receiveDamage);
	}
	[RPC]
	void UpdateIsDead (bool isDead) {
		myAnimator.SetBool ("isDead", isDead);
	}

	
	// Update is called once per frame
	void Update () {

		if (networkView.isMine) {

			if (/*myMobMovement.isAttacking &&*/ myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
				myMobMovement.timerBetweenAttacks = 0f;
			}
			else {
				myMobMovement.timerBetweenAttacks += Time.deltaTime;
			}


			if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Death") &&
			    myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f) {
				GetComponent<HealthManager>().deathAnimationFinished = true;
			}

			if (GetComponent<HealthManager>().isTakingDamage) {
				if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("ReceiveDamage")) {
					GetComponent<Animator> ().SetBool ("receiveDamage", false);
					GetComponent<HealthManager>().isTakingDamage = false;
					receiveDamage = false;
					networkView.RPC ("UpdateReceiveDamage", RPCMode.All, false);
				}
				else {
					GetComponent<Animator>().speed = 1;
					GetComponent<Animator> ().SetBool ("receiveDamage", true);
					receiveDamage = true;
					networkView.RPC ("UpdateReceiveDamage", RPCMode.All, true);
				}
			}

			if (GetComponent<HealthManager>().isDead) {
				GetComponent<Animator> ().SetBool ("isDead", true);
				isDead = true;
				networkView.RPC ("UpdateIsDead", RPCMode.All, true);
			}

			if (GetComponent<MobMovement>().isAttacking &&
			    !GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Attack") && 
			    myMobMovement.timerBetweenAttacks >= myMobMovement.timeBetweenAttacksInSeconds) {
				GetComponent<Animator> ().SetBool ("isAttacking", true);
				GetComponent<MobMovement>().isAttacking = false;
				isAttacking = true;
				networkView.RPC ("UpdateIsAttacking", RPCMode.All, true);
			}
			else if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
				//myMobMovement.isAttacking = false;
				GetComponent<Animator> ().SetBool ("isAttacking", false);
				isAttacking = false;
				networkView.RPC ("UpdateIsAttacking", RPCMode.All, false);
			}

			Animator animator = GetComponent<Animator> ();
			MobMovement mobMovement = GetComponent<MobMovement> ();
			speed = (rigidbody.velocity - new Vector3 (0, rigidbody.velocity.y, 0)).magnitude;
			if (mobMovement.targetAquired) {
				animator.SetBool ("investigate", false);
				animator.SetBool ("chase", true);
				investigate = false;
				chase = true;
				networkView.RPC ("UpdateInvestigate", RPCMode.All, false);
				networkView.RPC ("UpdateChase", RPCMode.All, true);
			}
			else if (mobMovement.isSuspicious) {
				animator.SetBool ("investigate", true);
				animator.SetBool ("chase", false);
				investigate = true;
				chase = false;
				networkView.RPC ("UpdateInvestigate", RPCMode.All, true);
				networkView.RPC ("UpdateChase", RPCMode.All, false);
			}
			else {
				animator.SetBool ("investigate", false);
				animator.SetBool ("chase", false);
				investigate = false;
				chase = false;
				networkView.RPC ("UpdateInvestigate", RPCMode.All, false);
				networkView.RPC ("UpdateChase", RPCMode.All, false);
			}
		}
		else {
			//networkView.RPC ("UpdateAnimation", RPCMode.All, investigate, chase, isAttacking, receiveDamage, isDead);
		}
	}
}
