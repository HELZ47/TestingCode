using UnityEngine;
using System.Collections;

public class BotManager : MonoBehaviour {

	#region Fields
	public enum BotType { NORMAL, BODYGUARD };
	public enum BodyGuardType { GENERAL, SPECIFIC };
	//Adjustable
	public BotType botType;
	public BodyGuardType bodyguardType;
	public float damageAmount;
	public float timeBetweenAttacksInSeconds;
	public Projectile.DamageElement damageElement;
	public Projectile.DamageType damageType;
	public float enemyDetectionRange, VIPDetectionRange, movementSpeed, acceleration, stoppingRange, attackingRange;
	public GameObject projectileStartMarker;
	public Waypoints givenPath;
	//Not Adjustable
	//[HideInInspector]
	public bool targetAquired, VIPFound, isAttacking, hasAttackedThisAnimation;
	//[HideInInspector]
	public Transform VIPTransform;
	//[HideInInspector]
	public Transform TargetTransform;
	//[HideInInspector]
	public float timerBetweenAttacks;
	NavMeshAgent myNavMeshAgent;
	BotAnimator myBotAnimator;
	Animator myAnimator;
	#endregion


	//Instantiate local variables
	void Awake () {
		myNavMeshAgent = GetComponent<NavMeshAgent> ();
		myBotAnimator = GetComponent<BotAnimator> ();
		myAnimator = GetComponent<Animator> ();
	}

	#region State Synchronization function
	//State Sync function
	//Although the real calculations are done with navmesh agent, the clients are using rb to navigate
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
			//stream.Serialize(ref rbEnabled);
			stream.Serialize(ref position);
			stream.Serialize(ref forwardDir);
			stream.Serialize(ref velocity);
			stream.Serialize(ref isMovingAnim);
			stream.Serialize(ref takingDamageAnim);
			stream.Serialize(ref isDeadAnim);
			stream.Serialize(ref isAttackingAnim);
		} 
		else {
			//stream.Serialize(ref rbEnabled);
			stream.Serialize(ref position);
			stream.Serialize(ref forwardDir);
			stream.Serialize(ref velocity);
			stream.Serialize(ref isMovingAnim);
			stream.Serialize(ref takingDamageAnim);
			stream.Serialize(ref isDeadAnim);
			stream.Serialize(ref isAttackingAnim);
			transform.position = position;
			transform.forward = forwardDir;
			//if (rbEnabled) {
				rigidbody.isKinematic = false;
				rigidbody.velocity = velocity;
				myNavMeshAgent.Stop ();
//			}
//			else {
//				rigidbody.isKinematic = true;
//				myNavMeshAgent.velocity = velocity;
//			}
			myAnimator.SetBool ("isMoving", isMovingAnim);
			myAnimator.SetBool ("takingDamage", takingDamageAnim);
			myAnimator.SetBool ("isDead", isDeadAnim);
			myAnimator.SetBool ("isAttacking", isAttackingAnim);
		}
	}
	#endregion

}
