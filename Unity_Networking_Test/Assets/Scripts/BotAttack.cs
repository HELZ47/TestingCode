using UnityEngine;
using System.Collections;

public class BotAttack : MonoBehaviour {

	//Fields
	BotManager myBotManager;
	Animator myAnimator;


	[RPC]
	//RPC call that creates the projectile on the server side
	public void CreateProjectile (string source, Vector3 position, Quaternion rotation, Vector3 direction) {
		if (Network.isServer) {
			GameObject projectile = Network.Instantiate (Resources.Load(source), position, rotation, 0) as GameObject;
			int teamNum = 99;
			if (tag == "Team 1") {
				teamNum = 1;
			}
			else if (tag == "Team 2") {
				teamNum = 2;
			}
			projectile.GetComponent<Projectile>().InitVariables (direction.normalized, teamNum);
		}
	}


	// Use this for initialization
	void Awake () {
		myBotManager = GetComponent<BotManager> ();
		myAnimator = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) {
			return;
		}
		if (myBotManager.isAttacking && myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
		    myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f && 
		    myBotManager.hasAttackedThisAnimation == false) {
			myBotManager.isAttacking = false;
			if (myBotManager.TargetTransform != null && 
			    Vector3.Distance(myBotManager.TargetTransform.position, transform.position) < myBotManager.attackingRange &&
			    myBotManager.TargetTransform.gameObject.GetComponent<HealthManager>() != null &&
			    !myBotManager.TargetTransform.gameObject.GetComponent<HealthManager>().isDead) {
				myBotManager.hasAttackedThisAnimation = true;
				if (myBotManager.damageType == Projectile.DamageType.DIRECT) {
					Vector3 startPosition = myBotManager.projectileStartMarker.transform.position;
					Vector3 direction = (myBotManager.TargetTransform.position - startPosition).normalized;
					direction.y = 0f;
					CreateProjectile ("Prefabs/Lightning_Bullet", startPosition, new Quaternion(), direction);
				}
				else if (myBotManager.damageType == Projectile.DamageType.MELEE) {
					myBotManager.TargetTransform.gameObject.GetComponent<HealthManager>().ReceiveDamage (myBotManager.damageAmount, myBotManager.damageType, myBotManager.damageElement);
				}
			}
		}
		else if (myBotManager.hasAttackedThisAnimation && myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
		         myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.5f) {
			myBotManager.hasAttackedThisAnimation = false;
		}
	}
}
