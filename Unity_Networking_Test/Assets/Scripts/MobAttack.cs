using UnityEngine;
using System.Collections;

public class MobAttack : MonoBehaviour {

	//Fields
	MobMovement myMobMovement;
	Animator myAnimator;

	// Use this for initialization
	void Awake () {
		myMobMovement = GetComponent<MobMovement> ();
		myAnimator = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) {
			return;
		}
		if (myMobMovement.isAttacking && myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
		    myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f && 
		    myMobMovement.hasAttackedThisAnimation == false) {
			myMobMovement.isAttacking = false;
			if (myMobMovement.targetTransform != null && 
			    Vector3.Distance(myMobMovement.targetTransform.position, transform.position) < myMobMovement.attackRange &&
			    myMobMovement.targetTransform.gameObject.GetComponent<HealthManager>() != null &&
			    !myMobMovement.targetTransform.gameObject.GetComponent<HealthManager>().isDead) {
				myMobMovement.hasAttackedThisAnimation = true;
				myMobMovement.targetTransform.gameObject.GetComponent<HealthManager>().ReceiveDamage (myMobMovement.damageAmount, myMobMovement.damageType, myMobMovement.damageElement);
			}
		}
		else if (myMobMovement.hasAttackedThisAnimation && myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
		         myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.5f) {
			myMobMovement.hasAttackedThisAnimation = false;
		}
	}
}
