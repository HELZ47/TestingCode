using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	//Fields
	public enum ProjectileType { BULLET, GRENADE, ORB }
	public ProjectileType projectileType;
	public ParticleSystem projectParticles, hitParticles;
	public float lifeTime;
	public float bulletSpeed, grenadeSpeed, orbSpeed;
	public float bulletGSpeed, grenadeGSpeed, orbGSpeed;
	public NetworkPlayer owner;
	Vector3 direction;
	float speed, gSpeed, timeAlive;
	bool isHit;

	[RPC]
	void SetVariables (Vector3 givenDirection, NetworkPlayer ownerNP) {
		direction = givenDirection.normalized;
		owner = ownerNP;

		switch (projectileType) {
		case ProjectileType.BULLET:
			rigidbody.AddForce (direction * speed, ForceMode.VelocityChange);
			break;
		case ProjectileType.GRENADE:
			rigidbody.AddForce (direction * speed, ForceMode.Impulse);
			break;
		case ProjectileType.ORB:

			break;
		}
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
		timeAlive = 0f;
		
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
		if (!networkView.isMine) { //This is here because network.destroy can only be called once
			return;
		}
		if (isHit == true && !hitParticles.isPlaying) {
			Network.RemoveRPCs (networkView.viewID);
			Network.Destroy (gameObject);
		}
		else if (isHit != true && timeAlive > lifeTime) {
			Network.RemoveRPCs (networkView.viewID);
			Network.Destroy (gameObject);
		}
		else {
			networkView.RPC ("DetectCollision", RPCMode.AllBuffered);
		}
	}


	[RPC]
	void DetectCollision () {
		switch (projectileType) {
		case ProjectileType.BULLET:
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
			if (!isHit) {
				timeAlive += Time.deltaTime;
			}
			break;
		case ProjectileType.GRENADE:
			foreach (RaycastHit hitInfo in Physics.RaycastAll (transform.position, direction.normalized, 1f)) {
				if (hitInfo.collider != this.collider &&
				    hitInfo.collider.gameObject.GetComponentInParent<NetworkView>() &&
				 	hitInfo.collider.gameObject.GetComponentInParent<NetworkView>().owner != owner &&
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
			if (!isHit) {
				timeAlive += Time.deltaTime;
			}
			if (!isHit && timeAlive > lifeTime) {
				isHit = true;
				rigidbody.velocity = Vector3.zero;
				rigidbody.isKinematic = true;
				rigidbody.detectCollisions = false;
				projectParticles.Stop ();
				hitParticles.Play ();
				GetComponent<Collider>().isTrigger = true;
			}
			break;
		case ProjectileType.ORB:

			break;
		}

	}
	
}
