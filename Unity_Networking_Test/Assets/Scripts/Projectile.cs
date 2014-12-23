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
	public bool removeRPCs;
	int RPCGroup;
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
			rigidbody.AddForce (direction * speed, ForceMode.VelocityChange);
			break;
		}
//		if (owner == Network.player) {
//			print ("same player!");
//		}
//		else {
//			print ("different player!");
//		}
//		if (networkView.group == RPCGroup) {
//			print ("Same group!");
//		}
//		else {
//			print ("Different group!");
//		}
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
		removeRPCs = false;
		
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
//		if (!networkView.isMine) { //This is here because network.destroy can only be called once
//			rigidbody.isKinematic = true;
//			rigidbody.detectCollisions = false;
//			GetComponent<Collider>().isTrigger = true;
//		}
	}
	
	
	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) { //This is here because network.destroy can only be called once
			return;
		}
		if (isHit == true && !hitParticles.isPlaying) {
			Network.RemoveRPCs (networkView.viewID);
//			Network.RemoveRPCsInGroup (RPCGroup);
			//print ("removing group " + RPCGroup);
			//Network.RemoveRPCs (networkView.viewID);
//			ownerLaunchProjectile.RemoveRPCs (networkView.viewID);
//			DestroyProjectile (networkView.viewID);
//			networkView.RPC ("DestroyProjectile", RPCMode.AllBuffered,networkView.viewID);
			//removeRPCs = true;
			Network.Destroy (gameObject);
			//networkView.RPC ("DestroyProjectile", RPCMode.AllBuffered, networkView.viewID);
		}
		else if (isHit != true && timeAlive > lifeTime) {
			Network.RemoveRPCs (networkView.viewID);
//			Network.RemoveRPCsInGroup (RPCGroup);
			//print ("removing group " + RPCGroup);
			//Network.RemoveRPCs (networkView.viewID);
//			ownerLaunchProjectile.RemoveRPCs (networkView.viewID);
//			DestroyProjectile (networkView.viewID);
			//networkView.RPC ("DestroyProjectile", RPCMode.AllBuffered, networkView.viewID);
//			removeRPCs = true;
			Network.Destroy (gameObject);
			//networkView.RPC ("DestroyProjectile", RPCMode.AllBuffered, networkView.viewID);
		}
		else {
			networkView.RPC ("DetectCollision", RPCMode.AllBuffered);
		}
	}

	[RPC]
	void DestroyProjectile (NetworkViewID id) {
		if (networkView.isMine) {
			Network.RemoveRPCs (id);
			Network.Destroy (gameObject);
		}
//		else {
//			networkView.RPC ("DestroyProjectile", RPCMode.Server, id);
//		}
	}

	[RPC]
	void DetectCollision () {
		switch (projectileType) {
		case ProjectileType.BULLET:
			foreach (Collider hitInfo in Physics.OverlapSphere (transform.position, GetComponent<SphereCollider>().radius+0.1f)) {
				if (hitInfo.collider != this.collider &&
				    (!hitInfo.collider.gameObject.GetComponentInParent<NetworkView>() ||
				 	 hitInfo.collider.gameObject.GetComponentInParent<NetworkView>().owner != owner) &&
				    !isHit) {
					isHit = true;
					//rigidbody.velocity = Vector3.zero;
					rigidbody.isKinematic = true;
					rigidbody.detectCollisions = false;
					//transform.position = hitInfo.point;
					projectParticles.Stop ();
					hitParticles.Play ();
					GetComponent<Collider>().isTrigger = true;
					GetComponent<MeshRenderer>().enabled = false;
				}
			}
			if (!isHit) {
				timeAlive += Time.deltaTime;
				//Debug----------------------------
				ParticleSystem.Particle[] particles = new ParticleSystem.Particle[projectParticles.particleCount+1];
				int numOfParticles = projectParticles.GetParticles (particles);
				Vector3 pVelocity = rigidbody.velocity * -1f;
				if (pVelocity.magnitude > 5f) {
					pVelocity = pVelocity.normalized * 5f;
				}
				int i = 0;
				while (i<numOfParticles) {
					particles[i].velocity = pVelocity;
					i++;
				}
				projectParticles.SetParticles (particles, numOfParticles);
				//---------------------------------
			}
			break;
		case ProjectileType.GRENADE:
			foreach (Collider hitInfo in Physics.OverlapSphere (transform.position, GetComponent<SphereCollider>().radius+0.1f)) {
				if (hitInfo.collider != this.collider &&
				    hitInfo.collider.gameObject.GetComponentInParent<NetworkView>() &&
				 	hitInfo.collider.gameObject.GetComponentInParent<NetworkView>().owner != owner &&
				    !isHit) {
					isHit = true;
					//rigidbody.velocity = Vector3.zero;
					rigidbody.isKinematic = true;
					rigidbody.detectCollisions = false;
					//transform.position = hitInfo.point;
					projectParticles.Stop ();
					hitParticles.Play ();
					GetComponent<Collider>().isTrigger = true;
					GetComponent<MeshRenderer>().enabled = false;
				}
			}
			if (!isHit) {
				timeAlive += Time.deltaTime;
				//Debug----------------------------
				ParticleSystem.Particle[] particles = new ParticleSystem.Particle[projectParticles.particleCount+1];
				int numOfParticles = projectParticles.GetParticles (particles);
				Vector3 pVelocity = rigidbody.velocity * -1f;
				if (pVelocity.magnitude > 5f) {
					pVelocity = pVelocity.normalized * 5f;
				}
				int i = 0;
				while (i<numOfParticles) {
					particles[i].velocity = pVelocity;
					i++;
				}
				projectParticles.SetParticles (particles, numOfParticles);
				//---------------------------------
			}
			if (!isHit && timeAlive > lifeTime) {
				isHit = true;
				//rigidbody.velocity = Vector3.zero;
				rigidbody.isKinematic = true;
				rigidbody.detectCollisions = false;
				projectParticles.Stop ();
				hitParticles.Play ();
				GetComponent<Collider>().isTrigger = true;
				GetComponent<MeshRenderer>().enabled = false;
			}
			break;
		case ProjectileType.ORB:
			foreach (Collider hitInfo in Physics.OverlapSphere (transform.position, GetComponent<SphereCollider>().radius+0.1f)) {
				if (hitInfo.collider != this.collider &&
				    (!hitInfo.collider.gameObject.GetComponentInParent<NetworkView>() ||
				 	 hitInfo.collider.gameObject.GetComponentInParent<NetworkView>().owner != owner) &&
				    !isHit && hitInfo.collider.tag != "Projectiles") {
					isHit = true;
					//rigidbody.velocity = Vector3.zero;
					rigidbody.isKinematic = true;
					rigidbody.detectCollisions = false;
					//transform.position = hitInfo.point;
					projectParticles.Stop ();
					hitParticles.Play ();
					GetComponent<Collider>().isTrigger = true;
					GetComponent<MeshRenderer>().enabled = false;
				}
			}
			if (!isHit) {
				timeAlive += Time.deltaTime;
				//Debug----------------------------
				ParticleSystem.Particle[] particles = new ParticleSystem.Particle[projectParticles.particleCount+1];
				int numOfParticles = projectParticles.GetParticles (particles);
				Vector3 pVelocity = rigidbody.velocity * -1f;
				if (pVelocity.magnitude > 5f) {
					pVelocity = pVelocity.normalized * 5f;
				}
				int i = 0;
				while (i<numOfParticles) {
					particles[i].velocity = pVelocity;
					i++;
				}
				projectParticles.SetParticles (particles, numOfParticles);
				//---------------------------------
			}
			break;
		}

	}


	[RPC]
	void DetonateProjectile () {
		isHit = true;
		if (!rigidbody.isKinematic) {
			rigidbody.velocity = Vector3.zero;
		}
		rigidbody.isKinematic = true;
		rigidbody.detectCollisions = false;
		projectParticles.Stop ();
		hitParticles.Play ();
		GetComponent<Collider>().isTrigger = true;
		GetComponent<MeshRenderer>().enabled = false;
	}

//	void OnCollisionEnter (Collision col) {
//		if (!networkView.isMine) {
//			return;
//		}
////		Network.RemoveRPCs (networkView.viewID);
////		Network.Destroy (gameObject);
//		if (!col.collider.gameObject.GetComponentInParent<NetworkView>() ||
//		    col.collider.gameObject.GetComponentInParent<NetworkView>().owner != owner) {
//			networkView.RPC ("DetonateProjectile", RPCMode.AllBuffered);
//		}
//	}
	
}
