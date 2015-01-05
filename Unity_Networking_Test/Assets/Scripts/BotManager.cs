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
	public ParticleSystem burnedP, frozenP, shockedP, slowedP, confusedP;
	//Not Adjustable
	[HideInInspector]
	public Waypoints givenPath;
	[HideInInspector]
	public bool targetAquired, VIPFound, isAttacking, hasAttackedThisAnimation;
	[HideInInspector]
	public Transform VIPTransform;
	[HideInInspector]
	public Transform TargetTransform;
	[HideInInspector]
	public float timerBetweenAttacks;
	NavMeshAgent myNavMeshAgent;
	BotAnimator myBotAnimator;
	Animator myAnimator;
	HealthManager myHPManager;
	#endregion


	//Instantiate local variables
	void Awake () {
		myNavMeshAgent = GetComponent<NavMeshAgent> ();
		myBotAnimator = GetComponent<BotAnimator> ();
		myAnimator = GetComponent<Animator> ();
		myHPManager = GetComponent<HealthManager> ();
		burnedP.Stop ();
		frozenP.Stop ();
		shockedP.Stop ();
		slowedP.Stop ();
		confusedP.Stop ();
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
		int status = 99;
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
			status = (int)myHPManager.statusEffect;
			//stream.Serialize(ref rbEnabled);
			stream.Serialize(ref position);
			stream.Serialize(ref forwardDir);
			stream.Serialize(ref velocity);
			stream.Serialize(ref isMovingAnim);
			stream.Serialize(ref takingDamageAnim);
			stream.Serialize(ref isDeadAnim);
			stream.Serialize(ref isAttackingAnim);
			stream.Serialize(ref status);
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
			stream.Serialize(ref status);
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
			HealthManager.StatusEffect prevEffect = myHPManager.statusEffect;
			myHPManager.statusEffect = (HealthManager.StatusEffect)status;
			if (prevEffect != myHPManager.statusEffect) {
				burnedP.Stop ();
				frozenP.Stop ();
				shockedP.Stop ();
				slowedP.Stop ();
				confusedP.Stop ();
				switch (myHPManager.statusEffect) {
				case HealthManager.StatusEffect.BURNED:
					burnedP.Play();
					break;
				case HealthManager.StatusEffect.CONFUSED:
					confusedP.Play ();
					break;
				}
			}
		}
	}
	#endregion

}
