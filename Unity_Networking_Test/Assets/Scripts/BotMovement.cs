using UnityEngine;
using System.Collections;

public class BotMovement : MonoBehaviour {

	//Fields
	BotManager myBotManager;
	HealthManager myHealthManager;
	
	// Use this for initialization
	void Awake () {
		myBotManager = GetComponent<BotManager> ();
		myHealthManager = GetComponent<HealthManager> ();
	}
	
//	[RPC]
//	void UpdateState (bool isAtt, bool targAqui, bool isSus) {
//		isAttacking = isAtt;
//		targetAquired = targetAquired;
//		isSuspicious = isSus;
//	}
	
	//Rotation
	void FixedUpdate () {
		if (networkView.isMine) {
			if (GetComponent<HealthManager>().isDead) {
				return;
			}
			if (myBotManager.isAttacking && myBotManager.TargetTransform != null) {
				Vector3 direction = (myBotManager.TargetTransform.position - transform.position).normalized;
				direction.y = 0f;
				transform.forward = Vector3.Slerp (transform.forward, direction, 0.1f);
				return;
			}
			if (myBotManager.targetAquired && myBotManager.TargetTransform != null) {
				Vector3 direction = (myBotManager.TargetTransform.position - transform.position).normalized;
				direction.y = 0f;
				transform.forward = Vector3.Slerp (transform.forward, direction, 0.1f);
				rigidbody.AddForce (direction * myBotManager.acceleration, ForceMode.Acceleration);
				Vector3 temp = new Vector3 (rigidbody.velocity.x, 0, rigidbody.velocity.z);
				if (temp.magnitude > myBotManager.movementSpeed) {
					rigidbody.velocity = temp.normalized * myBotManager.movementSpeed;
				}
			}
			else if (myBotManager.VIPFound && myBotManager.VIPTransform != null && 
			         Vector3.Distance(transform.position, myBotManager.VIPTransform.position) > myBotManager.stoppingRange) {
				Vector3 direction = (myBotManager.VIPTransform.position - transform.position).normalized;
				direction.y = 0f;
				transform.forward = Vector3.Slerp (transform.forward, direction, 0.1f);
				rigidbody.AddForce (direction * myBotManager.acceleration, ForceMode.Acceleration);
				Vector3 temp = new Vector3 (rigidbody.velocity.x, 0, rigidbody.velocity.z);
				if (temp.magnitude > myBotManager.movementSpeed) {
					rigidbody.velocity = temp.normalized * myBotManager.movementSpeed;
				}
			}
		}
		else {
			
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (networkView.isMine ) {
			if (myBotManager.VIPFound && Vector3.Distance(transform.position, myBotManager.VIPTransform.position) > myBotManager.VIPDetectionRange) {
				myBotManager.VIPFound = false;
			}
			myBotManager.targetAquired = false;
			Collider[] nColliders = Physics.OverlapSphere (transform.position, myBotManager.enemyDetectionRange);
			Collider[] vColliders = Physics.OverlapSphere (transform.position, myBotManager.VIPDetectionRange);
			float closestEnemyDistance = 9999f;
			foreach (Collider col in nColliders) {
				if (col.gameObject.tag == "Mobs") {
					if (closestEnemyDistance == 9999) {
						myBotManager.targetAquired = true;
						myBotManager.TargetTransform = col.gameObject.transform;
						closestEnemyDistance = Vector3.Distance (transform.position, myBotManager.TargetTransform.position);
					}
					else if (Vector3.Distance (transform.position, col.transform.position) < closestEnemyDistance) {
						myBotManager.targetAquired = true;
						myBotManager.TargetTransform = col.gameObject.transform;
						closestEnemyDistance = Vector3.Distance (transform.position, myBotManager.TargetTransform.position);
					}

				}
			}
			foreach (Collider col in vColliders) {
				if (myBotManager.VIPFound == false && col.gameObject.GetComponent<PlayerManager>() != null) {
					myBotManager.VIPFound = true;
					myBotManager.VIPTransform = col.gameObject.transform;
				}
			}
			if (myBotManager.targetAquired && Vector3.Distance (transform.position, myBotManager.TargetTransform.position) < myBotManager.attackingRange &&
			    myBotManager.TargetTransform.gameObject.GetComponent<HealthManager>() != null &&
			    !myBotManager.TargetTransform.gameObject.GetComponent<HealthManager>().isDead) {
				myBotManager.isAttacking = true;
			}
//			networkView.RPC ("UpdateState", RPCMode.All, isAttacking, targetAquired, isSuspicious);
		}
		else {
			//networkView.RPC ("UpdateState", RPCMode.All, isAttacking, targetAquired, isSuspicious);
		}
	}
}
