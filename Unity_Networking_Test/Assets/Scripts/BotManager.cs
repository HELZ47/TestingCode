using UnityEngine;
using System.Collections;

public class BotManager : MonoBehaviour {

	//Fields

	//Adjustable
	public enum BotType { GENERAL_BODY_GUARD, SPECIFIC_BODY_GUARD, PATROL };
	public BotType botType;
	public float damageAmount;
	public float timeBetweenAttacksInSeconds;
	public Projectile.DamageElement damageElement;
	public Projectile.DamageType damageType;
	public float enemyDetectionRange, VIPDetectionRange, movementSpeed, acceleration, stoppingRange, attackingRange;
	public GameObject projectileStartMarker;

	//Not Adjustable
	public bool targetAquired, VIPFound, isAttacking, hasAttackedThisAnimation;
	public Transform VIPTransform;
	public Transform TargetTransform;
	public float timerBetweenAttacks;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (networkView.isMine) {
			if (/*isAttacking &&*/ GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
				timerBetweenAttacks = 0f;
			}
			else {
				timerBetweenAttacks += Time.deltaTime;
			}
		}
	}
}
