using UnityEngine;
using System.Collections;

public class MobMovement : MonoBehaviour {

	//Fields
	public float suspicionRange, sightRange, investigateSpeed, chaseSpeed, acceleration;
	public float attackRange;
	public bool isAttacking;
	public bool targetAquired, isSuspicious;
	Vector3 targetPosition;

	// Use this for initialization
	void Start () {
	
	}

	[RPC]
	void UpdateState (bool isAtt, bool targAqui, bool isSus) {
		isAttacking = isAtt;
		targetAquired = targetAquired;
		isSuspicious = isSus;
	}

	//Rotation
	void FixedUpdate () {
		if (networkView.isMine) {
			if (GetComponent<HealthManager>().isDead || isAttacking) {
				return;
			}
			if (targetAquired) {
				Vector3 direction = (targetPosition - transform.position).normalized;
				direction = direction - new Vector3 (0, direction.y, 0);
				transform.forward = Vector3.Slerp (transform.forward, direction, 0.1f);
				rigidbody.AddForce (direction * acceleration, ForceMode.Acceleration);
				Vector3 temp = new Vector3 (rigidbody.velocity.x, 0, rigidbody.velocity.z);
				if (temp.magnitude > chaseSpeed) {
					rigidbody.velocity = temp.normalized * chaseSpeed;
				}
			}
			else if (isSuspicious) {
				Vector3 direction = (targetPosition - transform.position).normalized;
				direction = direction - new Vector3 (0, direction.y, 0);
				transform.forward = Vector3.Slerp (transform.forward, direction, 0.1f);
				rigidbody.AddForce (direction * acceleration, ForceMode.Acceleration);
				Vector3 temp = new Vector3 (rigidbody.velocity.x, 0, rigidbody.velocity.z);
				if (temp.magnitude > investigateSpeed) {
					rigidbody.velocity = temp.normalized * investigateSpeed;
				}
			}
		}
		else {

		}
	}

	// Update is called once per frame
	void Update () {
		if (networkView.isMine ) {
			targetAquired = false;
			isSuspicious = false;
			Collider[] colliders = Physics.OverlapSphere (transform.position, sightRange);
			Collider[] sColliders = Physics.OverlapSphere (transform.position, suspicionRange);
			foreach (Collider col in colliders) {
				if (col.gameObject.GetComponent<PlayerManager>() != null) {
					targetAquired = true;
					targetPosition = col.gameObject.transform.position;
				}
			}
			if (!targetAquired) {
				foreach (Collider col in sColliders) {
					if (col.gameObject.GetComponent<PlayerManager>() != null) {
						isSuspicious = true;
						targetPosition = col.gameObject.transform.position;
					}
				}
			}
			if (targetAquired && Vector3.Distance (transform.position, targetPosition) < attackRange) {
				isAttacking = true;
			}
			networkView.RPC ("UpdateState", RPCMode.All, isAttacking, targetAquired, isSuspicious);
		}
		else {
			//networkView.RPC ("UpdateState", RPCMode.All, isAttacking, targetAquired, isSuspicious);
		}
	}
}
