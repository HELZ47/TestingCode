using UnityEngine;
using System.Collections;

public class MobMovement : MonoBehaviour {

	//Fields
	public float sightRange, speed;
	public bool targetAquired;
	public Vector3 targetPosition;

	// Use this for initialization
	void Start () {
	
	}

	//Rotation
	void FixedUpdate () {
		if (targetAquired) {
			Vector3 direction = (targetPosition - transform.position).normalized;
			direction = direction - new Vector3 (0, direction.y, 0);
			transform.forward = Vector3.Slerp (transform.forward, direction, 0.1f);
			rigidbody.AddForce (direction * speed, ForceMode.Force);
			print (rigidbody.velocity);
		}
		else {
			rigidbody.velocity = Vector3.zero;
		}
		if ((rigidbody.velocity - new Vector3 (0, rigidbody.velocity.y, 0)).magnitude > 0) {
			GetComponent<Animator>().speed = 1;
		}
		else {
			GetComponent<Animator>().speed = 0;
		}
		//if ((rigidbody.velocity - new Vector3 (0, rigidbody.velocity.y, 0)).magnitude > 0)
		//print ((rigidbody.velocity - new Vector3 (0, rigidbody.velocity.y, 0)).magnitude);
		//GetComponent<Animator> ().speed = (rigidbody.velocity - new Vector3 (0, rigidbody.velocity.y, 0)).magnitude;
	}

	// Update is called once per frame
	void Update () {
		targetAquired = false;
		Collider[] colliders = Physics.OverlapSphere (transform.position, sightRange);
		foreach (Collider col in colliders) {
			if (col.gameObject.GetComponent<PlayerManager>() != null) {
				targetAquired = true;
				targetPosition = col.gameObject.transform.position;
			}
		}

//		if (targetAquired) {
//			Vector3 direction = (targetPosition - transform.position).normalized;
//			direction.y = 0;
//			rigidbody.AddForce (direction * speed, ForceMode.Force);
//			//transform.Translate (direction * speed * Time.deltaTime, Space.World);
//			//transform.position += (direction * speed * Time.deltaTime);
//		}
	}
}
