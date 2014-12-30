﻿using UnityEngine;
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
	public Waypoints givenPath;

	//Not Adjustable
	public bool targetAquired, VIPFound, isAttacking, hasAttackedThisAnimation;
	public Transform VIPTransform;
	public Transform TargetTransform;
	public float timerBetweenAttacks;
	NavMeshAgent myNavMeshAgent;
	BotAnimator myBotAnimator;
	Animator myAnimator;

	//Instantiate local variables
	void Awake () {
		myNavMeshAgent = GetComponent<NavMeshAgent> ();
		myBotAnimator = GetComponent<BotAnimator> ();
		myAnimator = GetComponent<Animator> ();
	}


	//State Sync function
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		bool rbEnabled = false;
		Vector3 position = Vector3.zero;
		Vector3 forwardDir = Vector3.zero;
		Vector3 velocity = Vector3.zero;
		Vector3 nmDestination = Vector3.zero;
		bool isMovingAnim = false, takingDamageAnim = false, isDeadAnim = false, isAttackingAnim = false;
		if (stream.isWriting) {
			rbEnabled = !rigidbody.isKinematic;
			position = transform.position;
			forwardDir = transform.forward;
			if (rbEnabled) {
				velocity = rigidbody.velocity;
			}
			else {
				velocity = myNavMeshAgent.velocity;
			}
			isMovingAnim = myBotAnimator.isMoving;
			takingDamageAnim = myBotAnimator.takingDamage;
			isDeadAnim = myBotAnimator.isDead;
			isAttackingAnim = myBotAnimator.isAttacking;
			stream.Serialize(ref rbEnabled);
			stream.Serialize(ref position);
			stream.Serialize(ref forwardDir);
			stream.Serialize(ref velocity);
			stream.Serialize(ref isMovingAnim);
			stream.Serialize(ref takingDamageAnim);
			stream.Serialize(ref isDeadAnim);
			stream.Serialize(ref isAttackingAnim);
		} 
		else {
			stream.Serialize(ref rbEnabled);
			stream.Serialize(ref position);
			stream.Serialize(ref forwardDir);
			stream.Serialize(ref velocity);
			stream.Serialize(ref isMovingAnim);
			stream.Serialize(ref takingDamageAnim);
			stream.Serialize(ref isDeadAnim);
			stream.Serialize(ref isAttackingAnim);
			transform.position = position;
			transform.forward = forwardDir;
			if (rbEnabled) {
				rigidbody.isKinematic = false;
				rigidbody.velocity = velocity;
				myNavMeshAgent.Stop ();
			}
			else {
				rigidbody.isKinematic = true;
				myNavMeshAgent.velocity = velocity;
			}
			myAnimator.SetBool ("isMoving", isMovingAnim);
			myAnimator.SetBool ("takingDamage", takingDamageAnim);
			myAnimator.SetBool ("isDead", isDeadAnim);
			myAnimator.SetBool ("isAttacking", isAttackingAnim);
		}
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
