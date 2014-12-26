using UnityEngine;
using System.Collections;

public class BotManager : MonoBehaviour {

	//Fields
	//Adjustable
	public enum BotType { GENERAL_BODY_GUARD, SPECIFIC_BODY_GUARD, PATROL };
	public BotType botType;
	public float damageAmount;
	public Projectile.DamageElement damageElement;
	public Projectile.DamageType damageType;
	public float enemyDetectionRange, VIPDetectionRange, movementSpeed, acceleration, stoppingRange, attackingRange;
	//Not Adjustable
	public bool targetAquired, VIPFound, isAttacking, hasAttackedThisAnimation;
	public Transform VIPTransform;
	public Transform TargetTransform;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
