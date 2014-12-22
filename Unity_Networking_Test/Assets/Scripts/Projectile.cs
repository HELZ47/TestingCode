using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	//Fields
	public enum ProjectileType { BULLET, GRENADE, ORB }
	public ProjectileType projectileType;
	public ParticleSystem projectParticles, hitParticles;
	public float bulletSpeed, grenadeSpeed, orbSpeed;
	public float bulletGSpeed, grenadeGSpeed, orbGSpeed;
	public NetworkPlayer owner;
	Vector3 direction;
	float speed, gSpeed;
	bool isHit;

	[RPC]
	void SetVariables (Vector3 givenDirection, NetworkPlayer ownerNP) {
		direction = givenDirection.normalized;
		owner = ownerNP;
		rigidbody.AddForce (direction * speed, ForceMode.VelocityChange);

	}



	//Setup the initial variables for the projectile
	public void InitVariables (Vector3 givenDirection, NetworkPlayer ownerNP) { 
//		direction = givenDirection.normalized;
//		ownerGameObject = ownerGO;
//		rigidbody.AddForce (direction * speed, ForceMode.VelocityChange);
		networkView.RPC ("SetVariables", RPCMode.AllBuffered, givenDirection, ownerNP);
	}


	//Initialized its own vatiables when instantiated
	void Awake () {
		projectParticles.Play ();
		hitParticles.Stop ();
		isHit = false;
		
		switch (projectileType) {
		case ProjectileType.BULLET:
			speed = bulletSpeed;
			gSpeed = bulletGSpeed;
			break;
		case ProjectileType.GRENADE:
			speed = grenadeSpeed;
			gSpeed = grenadeGSpeed;
			break;
		case ProjectileType.ORB:
			speed = orbSpeed;
			gSpeed = orbGSpeed;
			break;
		}
	}


	// Use this for initialization
	void Start () {
	}


	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) {
			return;
		}
		if (isHit == true && projectParticles.isStopped) {
			Network.RemoveRPCs (networkView.viewID);
			Network.Destroy (gameObject);
		}
		else {
			networkView.RPC ("DetectCollision", RPCMode.AllBuffered);
		}
		//DetectCollision ();
//		if (isHit == true && projectParticles.isStopped) {
//			//Network.Destroy (this.gameObject);
//			
//			networkView.RPC ("Destroy", RPCMode.AllBuffered);
//		}
	}


	[RPC]
	void DetectCollision () {
		foreach (RaycastHit hitInfo in Physics.RaycastAll (transform.position, direction.normalized, 1f)) {
			if (hitInfo.collider != this.collider &&
			    (!hitInfo.collider.gameObject.GetComponentInParent<NetworkView>() ||
			 	hitInfo.collider.gameObject.GetComponentInParent<NetworkView>().owner != owner) &&
			    !isHit) {
				isHit = true;
				rigidbody.velocity = Vector3.zero;
				rigidbody.isKinematic = true;
				rigidbody.detectCollisions = false;
				transform.position = hitInfo.point;
				projectParticles.Stop ();
				hitParticles.Play ();
				GetComponent<Collider>().isTrigger = true;
			}
		}
	}


	[RPC]
	void Destroy () {
		Network.Destroy (this.gameObject);
	}


//	void OnCollisionEnter (Collision col) {
//		if (!networkView.isMine) {
//			return;
//		}
//		print ("Collided!!!");
//		if (!col.gameObject.GetComponentInParent<NetworkView>() || 
//			col.gameObject.GetComponentInParent<NetworkView>().owner != owner && !isHit) {
//			isHit = true;
//			rigidbody.velocity = Vector3.zero;
//			rigidbody.isKinematic = true;
//			rigidbody.detectCollisions = false;
//			projectParticles.Stop ();
//			hitParticles.Play ();
//			GetComponent<Collider>().isTrigger = true;
//		}
//	}
}
