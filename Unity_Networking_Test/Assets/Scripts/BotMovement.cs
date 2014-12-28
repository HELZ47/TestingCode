using UnityEngine;
using System.Collections;

public class BotMovement : MonoBehaviour {

	//Fields
	BotManager myBotManager;
	HealthManager myHealthManager;
	NavMeshAgent myNavMeshAgent;
	bool rbEnabled;
	Vector3 currentWayPoint;
	int currentWayPointIndex;
	
	// Use this for initialization
	void Awake () {
		myBotManager = GetComponent<BotManager> ();
		myHealthManager = GetComponent<HealthManager> ();
		myNavMeshAgent = GetComponent<NavMeshAgent> ();

		myNavMeshAgent.speed = myBotManager.movementSpeed;
		myNavMeshAgent.acceleration = myBotManager.acceleration;
	}

	
	//Rotation
	void FixedUpdate () {
		if (networkView.isMine) {
			//If the bot is dead, stop its movement motion
			if (GetComponent<HealthManager>().isDead) {
				if (rbEnabled) {
					rigidbody.velocity = new Vector3 (0, rigidbody.velocity.y, 0);
				}
				else {
					myNavMeshAgent.Stop ();
				}
				return;
			}
			//If the bot is attacking, stop its motion, but make it turn towards it's target
			if (myBotManager.isAttacking && myBotManager.TargetTransform != null) {
				if (rbEnabled) {
					rigidbody.velocity = new Vector3 (0, rigidbody.velocity.y, 0);
				}
				else {
					myNavMeshAgent.Stop ();
				}
				Vector3 direction = (myBotManager.TargetTransform.position - transform.position).normalized;
				direction.y = 0f;
				transform.forward = Vector3.Slerp (transform.forward, direction, 0.1f);
				return;
			}
			//If the target is not within striking distance, move towards it using rigidBody
			else if (myBotManager.targetAquired && myBotManager.TargetTransform != null &&
			         myBotManager.TargetTransform.gameObject.GetComponent<HealthManager>() != null &&
			         !myBotManager.TargetTransform.gameObject.GetComponent<HealthManager>().isDead) {
				if (!rbEnabled) {
					myNavMeshAgent.Stop();
					rigidbody.isKinematic = false;
				}
				Vector3 direction = (myBotManager.TargetTransform.position - transform.position).normalized;
				direction.y = 0f;
				transform.forward = Vector3.Slerp (transform.forward, direction, 0.1f);
				rigidbody.AddForce (direction * myBotManager.acceleration, ForceMode.Acceleration);
				Vector3 temp = new Vector3 (rigidbody.velocity.x, 0, rigidbody.velocity.z);
				if (temp.magnitude > myBotManager.movementSpeed) {
					rigidbody.velocity = temp.normalized * myBotManager.movementSpeed;
				}
			}
			else {
				if (myBotManager.botType == BotManager.BotType.GENERAL_BODY_GUARD) {
					//If there is not target within detection range, follow VIP using navMesh
					if (myBotManager.VIPFound && myBotManager.VIPTransform != null && 
					         Vector3.Distance(transform.position, myBotManager.VIPTransform.position) > myBotManager.stoppingRange) {
						Vector3 direction = (myBotManager.VIPTransform.position - transform.position).normalized;
						direction.y = 0f;
						transform.forward = Vector3.Slerp (transform.forward, direction, 0.1f);
		//				rigidbody.AddForce (direction * myBotManager.acceleration, ForceMode.Acceleration);
		//				Vector3 temp = new Vector3 (rigidbody.velocity.x, 0, rigidbody.velocity.z);
		//				if (temp.magnitude > myBotManager.movementSpeed) {
		//					rigidbody.velocity = temp.normalized * myBotManager.movementSpeed;
		//				}
						if (rbEnabled) {
							rigidbody.isKinematic = true;
						}
						myNavMeshAgent.SetDestination (myBotManager.VIPTransform.position);
						myNavMeshAgent.stoppingDistance = myBotManager.stoppingRange;
					}
					//If there is no target and no VIP, stop the bot
					else {
						if (rbEnabled) {
							rigidbody.velocity = new Vector3 (0, rigidbody.velocity.y, 0);
						}
						else {
							myNavMeshAgent.Stop();
						}
					}
				}
				else if (myBotManager.botType == BotManager.BotType.PATROL) {
					if (rbEnabled) {
						rigidbody.isKinematic = true;
					}
					if (Vector3.Distance(myBotManager.givenPath.waypoints[currentWayPointIndex], transform.position) < 2f) {
						currentWayPointIndex++;
						if (currentWayPointIndex > myBotManager.givenPath.waypoints.Count - 1) {
							currentWayPointIndex = 0;
						}
					}
					myNavMeshAgent.SetDestination (myBotManager.givenPath.waypoints [currentWayPointIndex]);
				}
			}
		}
		else {

		}
	}
	
	// Update is called once per frame
	void Update () {
		if (networkView.isMine ) {
			rbEnabled = !rigidbody.isKinematic;

			//If VIP gets outside of the VIPDetection range, VIP found becomes false
			if (myBotManager.VIPFound && Vector3.Distance(transform.position, myBotManager.VIPTransform.position) > myBotManager.VIPDetectionRange) {
				myBotManager.VIPFound = false;
			}
			//Reset targetAquired
			myBotManager.targetAquired = false;
			//Get the colliders within range
			Collider[] nColliders = Physics.OverlapSphere (transform.position, myBotManager.enemyDetectionRange);
			Collider[] vColliders = Physics.OverlapSphere (transform.position, myBotManager.VIPDetectionRange);
			float closestEnemyDistance = 9999f;
			//Detect if any target is within range
			foreach (Collider col in nColliders) {
				if ((tag == "Team 1" && col.tag != "Team 1") || 
				    (tag == "Team 2" && col.tag != "Team 2")) {
					if (Vector3.Distance (transform.position, col.transform.position) < closestEnemyDistance) {
						myBotManager.targetAquired = true;
						myBotManager.TargetTransform = col.gameObject.transform;
						closestEnemyDistance = Vector3.Distance (transform.position, myBotManager.TargetTransform.position);
					}

				}
			}
			//If a VIP hasn't been found, check if any VIP is within range
			if (!myBotManager.VIPFound) {
				foreach (Collider col in vColliders) {
					if (col.gameObject.GetComponent<PlayerManager>() != null) {
						myBotManager.VIPFound = true;
						myBotManager.VIPTransform = col.gameObject.transform;
					}
				}
			}
			//If the target is withing strike range, set isAttacking to true
			if (myBotManager.targetAquired && Vector3.Distance (transform.position, myBotManager.TargetTransform.position) < myBotManager.attackingRange &&
			    myBotManager.TargetTransform.gameObject.GetComponent<HealthManager>() != null &&
			    !myBotManager.TargetTransform.gameObject.GetComponent<HealthManager>().isDead) {
				myBotManager.isAttacking = true;
			}
		}
		else {
			//networkView.RPC ("UpdateState", RPCMode.All, isAttacking, targetAquired, isSuspicious);
		}
	}
}
