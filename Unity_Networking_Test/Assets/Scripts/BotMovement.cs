using UnityEngine;
using System.Collections;

//Controls the bots' movement and target aquisition
public class BotMovement : MonoBehaviour {

	#region Fields
	BotManager myBotManager;
	HealthManager myHealthManager;
	NavMeshAgent myNavMeshAgent;
	bool rbEnabled;
	Vector3 currentWayPoint;
	int currentWayPointIndex;
	#endregion


	#region Initialization
	// Initialize internal variables
	void Awake () {
		myBotManager = GetComponent<BotManager> ();
		myHealthManager = GetComponent<HealthManager> ();
		myNavMeshAgent = GetComponent<NavMeshAgent> ();
		//sync the navmesh agent variables with the bot manager's
		myNavMeshAgent.speed = myBotManager.movementSpeed;
		myNavMeshAgent.acceleration = myBotManager.acceleration;
	}
	#endregion


	#region Functions
	//Aquire VIP target
	void UpdateVIPStatus () {
		//If VIP gets outside of the VIPDetection range or dies, stop tracking this VIP
		if (myBotManager.VIPFound && (myBotManager.VIPTransform == null || 
        	Vector3.Distance(transform.position, myBotManager.VIPTransform.position) > myBotManager.VIPDetectionRange)) {
			myBotManager.VIPFound = false;
			myBotManager.VIPTransform = null;
		}
		//If there's no VIP attached to this bot, find nearest one within its range
		if (!myBotManager.VIPFound) {
			Collider[] vColliders = Physics.OverlapSphere (transform.position, myBotManager.VIPDetectionRange);
			float closestDistance = 9999f;
			foreach (Collider col in vColliders) {
				if (col.gameObject.GetComponent<PlayerManager>() != null
				    && Vector3.Distance(transform.position, col.transform.position) < closestDistance) {
					myBotManager.VIPFound = true;
					myBotManager.VIPTransform = col.gameObject.transform;
					closestDistance = Vector3.Distance(transform.position, col.transform.position);
				}
			}
		}
	}


	//Update on the aquired target
	void UpdateTargetStatus () {
		/*Normal bots will target the closest enemy, whereas the bodyguard bots will target the one
		 closest to the VIP*/
		switch (myBotManager.botType) {
		case BotManager.BotType.NORMAL:
			myBotManager.targetAquired = false;
			myBotManager.TargetTransform = null;
			//Get the colliders within range
			Collider[] nColliders = Physics.OverlapSphere (transform.position, myBotManager.enemyDetectionRange);
			float closestEnemyDistance = 9999f;
			//Detect the closest target within range
			foreach (Collider col in nColliders) {
				if (col.GetComponent<HealthManager>() != null &&
				    !col.GetComponent<HealthManager>().isDead &&
					((tag == "Team 1" && col.tag != "Team 1") || 
				     (tag == "Team 2" && col.tag != "Team 2"))) {
					if (Vector3.Distance (transform.position, col.transform.position) < closestEnemyDistance) {
						myBotManager.targetAquired = true;
						myBotManager.TargetTransform = col.gameObject.transform;
						closestEnemyDistance = Vector3.Distance (transform.position, myBotManager.TargetTransform.position);
					}
				}
			}
			break;
		case BotManager.BotType.BODYGUARD:
			myBotManager.targetAquired = false;
			myBotManager.TargetTransform = null;
			//Get the colliders within range
			Collider[] potentialTargets = Physics.OverlapSphere (transform.position, myBotManager.enemyDetectionRange);
			closestEnemyDistance = 9999f;
			//Detect the closest target either to VIP or to itself
			foreach (Collider col in potentialTargets) {
				if (col.GetComponent<HealthManager>() != null &&
				    ((tag == "Team 1" && col.tag != "Team 1") || 
				 	 (tag == "Team 2" && col.tag != "Team 2"))) {
					if (!myBotManager.VIPFound && !myBotManager.VIPTransform != null) {
						if (Vector3.Distance (myBotManager.VIPTransform.position, col.transform.position) < closestEnemyDistance) {
							myBotManager.targetAquired = true;
							myBotManager.TargetTransform = col.gameObject.transform;
							closestEnemyDistance = Vector3.Distance (myBotManager.VIPTransform.position, myBotManager.TargetTransform.position);
						}
					}
					else {
						if (Vector3.Distance (transform.position, col.transform.position) < closestEnemyDistance) {
							myBotManager.targetAquired = true;
							myBotManager.TargetTransform = col.gameObject.transform;
							closestEnemyDistance = Vector3.Distance (transform.position, myBotManager.TargetTransform.position);
						}
					}
				}
			}
			break;
		}
	}
	#endregion


	//Controls rotation and movement
	void FixedUpdate () {
		//Only server makes these calculations
		if (networkView.isMine) {
			//If the bot is dead, stop its movement motion
			if (GetComponent<HealthManager>().isDead) {
				if (rbEnabled) {
					rigidbody.velocity = new Vector3 (0, rigidbody.velocity.y, 0);
					rigidbody.isKinematic = false;
					rigidbody.detectCollisions = false;
				}
				else {
					myNavMeshAgent.Stop ();
				}
			}
			//If there is a target, move in until it's in the fire range
			else if (myBotManager.targetAquired && myBotManager.TargetTransform != null) {
				if (Vector3.Distance(transform.position, myBotManager.TargetTransform.position) < myBotManager.attackingRange) {
//					if (rbEnabled) {
//						//rigidbody.velocity = new Vector3 (0, rigidbody.velocity.y, 0);
//						rigidbody.isKinematic = false;
//					}
//					else {
//						myNavMeshAgent.Stop ();
//					}
					rigidbody.isKinematic = true;
//					myNavMeshAgent.SetDestination (myBotManager.TargetTransform.position);
//					myNavMeshAgent.stoppingDistance = myBotManager.attackingRange*0.5f;
					myNavMeshAgent.Stop ();
//					myNavMeshAgent.avoidancePriority = 0;
					Vector3 direction = (myBotManager.TargetTransform.position - transform.position).normalized;
					direction.y = 0f;
					transform.forward = Vector3.Slerp (transform.forward, direction, 0.1f);
				}
				else {
//					if (!rbEnabled) {
//						myNavMeshAgent.Stop();
//						rigidbody.isKinematic = true;
//					}
//					else {
//						myNavMeshAgent.Stop ();
//						rigidbody.isKinematic = true;
//					}
					rigidbody.isKinematic = true;
					myNavMeshAgent.SetDestination (myBotManager.TargetTransform.position);
					myNavMeshAgent.stoppingDistance = myBotManager.attackingRange*0.8f;
//					myNavMeshAgent.avoidancePriority = 50;
//					if (myNavMeshAgent.isPathStale) {
//						myNavMeshAgent.ResetPath ();
//					}
					Vector3 direction = (myBotManager.TargetTransform.position - transform.position).normalized;
					direction.y = 0f;
					transform.forward = Vector3.Slerp (transform.forward, direction, 0.1f);
//					rigidbody.AddForce (direction * myBotManager.acceleration, ForceMode.Acceleration);
//					Vector3 temp = new Vector3 (rigidbody.velocity.x, 0, rigidbody.velocity.z);
//					if (temp.magnitude > myBotManager.movementSpeed) {
//						rigidbody.velocity = temp.normalized * myBotManager.movementSpeed;
//					}
				}
			}
			//If there is no targets, bots follow its normal routine
			else {
				//Bodyguard bots follow their VIP using navmesh agent
				if (myBotManager.botType == BotManager.BotType.BODYGUARD) {
					//If there is not target within detection range, follow VIP using navMesh
					if (myBotManager.VIPFound && myBotManager.VIPTransform != null) {
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
					//If there is no target and no VIP, stop the bodyguard bot
					else {
						if (rbEnabled) {
							rigidbody.velocity = new Vector3 (0, rigidbody.velocity.y, 0);
						}
						else {
							myNavMeshAgent.Stop();
						}
					}
				}
				//Normal bots follow the waypoints using navmesh agent
				else if (myBotManager.botType == BotManager.BotType.NORMAL) {
					if (rbEnabled) {
						rigidbody.isKinematic = true;
					}
					if (Vector3.Distance(myBotManager.givenPath.waypoints[currentWayPointIndex], transform.position) <= myBotManager.stoppingRange) {
						currentWayPointIndex++;
						if (currentWayPointIndex > myBotManager.givenPath.waypoints.Count - 1) {
							currentWayPointIndex = 0;
						}
					}
					myNavMeshAgent.SetDestination (myBotManager.givenPath.waypoints [currentWayPointIndex]);
					myNavMeshAgent.stoppingDistance = myBotManager.stoppingRange;
				}
			}
		}
	}


	//Update the bot's VIP and Target aquisition status
	void Update () {
		if (networkView.isMine) {
			//Update rbEnabled based on whether the rigidbody is activated
			rbEnabled = !rigidbody.isKinematic;

			switch (myBotManager.botType) {
			case BotManager.BotType.NORMAL:
				UpdateTargetStatus ();
				break;
			case BotManager.BotType.BODYGUARD:
				UpdateVIPStatus ();
				UpdateTargetStatus ();
				break;
			}
		}
	}

}
