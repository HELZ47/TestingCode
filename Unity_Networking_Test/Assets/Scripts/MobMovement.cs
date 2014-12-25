using UnityEngine;
using System.Collections;

public class MobMovement : MonoBehaviour {

	//Fields
	public float suspicionRange, sightRange, investigateSpeed, chaseSpeed, acceleration;
	bool targetAquired, isSuspicious;
	Vector3 targetPosition;

	// Use this for initialization
	void Start () {
	
	}

	//Rotation
	void FixedUpdate () {
		if (GetComponent<HealthManager>().isDead) {
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

	// Update is called once per frame
	void Update () {
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
	}
}
