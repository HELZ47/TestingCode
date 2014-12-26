using UnityEngine;
using System.Collections;

public class BotAttack : MonoBehaviour {

	//Fields
	BotManager myBotManager;
	Animator myAnimator;

	// Use this for initialization
	void Awake () {
		myBotManager = GetComponent<BotManager> ();
		myAnimator = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (myBotManager.isAttacking && myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
		    myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f && 
		    myBotManager.hasAttackedThisAnimation == false) {
			myBotManager.isAttacking = false;
			if (myBotManager.TargetTransform != null && 
			    Vector3.Distance(myBotManager.TargetTransform.position, transform.position) < myBotManager.attackingRange) {
				myBotManager.hasAttackedThisAnimation = true;
				myBotManager.TargetTransform.gameObject.GetComponent<HealthManager>().ReceiveDamage (myBotManager.damageAmount, myBotManager.damageType, myBotManager.damageElement);
			}
		}
		else if (myBotManager.hasAttackedThisAnimation && myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
		         myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.5f) {
			myBotManager.hasAttackedThisAnimation = false;
		}
	}
}
