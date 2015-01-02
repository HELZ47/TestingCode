using UnityEngine;
using System.Collections;

//Handles the bot's attacking behaviour
public class BotAttack : MonoBehaviour {

	#region Fields
	BotManager myBotManager;
	Animator myAnimator;
	#endregion


	//Creates a projectile on the server side
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


	// Initialize local variables
	void Awake () {
		myBotManager = GetComponent<BotManager> ();
		myAnimator = GetComponent<Animator> ();
	}
	
	
	//If the bot is close to the enemy, start the attack
	void AttackTarget () {
		//If the target is withing strike range and timer's up, set isAttacking to true
		if (myBotManager.timerBetweenAttacks >= myBotManager.timeBetweenAttacksInSeconds && 
		    myBotManager.targetAquired && myBotManager.TargetTransform != null &&
		    Vector3.Distance (transform.position, myBotManager.TargetTransform.position) < myBotManager.attackingRange &&
		    myBotManager.TargetTransform.gameObject.GetComponent<HealthManager>() != null &&
		    !myBotManager.TargetTransform.gameObject.GetComponent<HealthManager>().isDead) {
			myBotManager.timerBetweenAttacks = 0f;
			myBotManager.isAttacking = true;
		}
		//If timer's not up, increase timer
		else if (myBotManager.timerBetweenAttacks < myBotManager.timeBetweenAttacksInSeconds) {
			myBotManager.timerBetweenAttacks += Time.deltaTime;
		}
	}

	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) {
			return;
		}

		//Check if the target is within strike range, if so, attack
		AttackTarget ();

		//If the bot's attack animation reaches half, deal damage
		if (myBotManager.isAttacking && myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
		    myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f && 
		    myBotManager.hasAttackedThisAnimation == false) {
			myBotManager.isAttacking = false;
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
		//If the bot has dealt damage, reset the flag on next attack
		else if (myBotManager.hasAttackedThisAnimation && myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
		         myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.5f) {
			myBotManager.hasAttackedThisAnimation = false;
		}
	}
}
