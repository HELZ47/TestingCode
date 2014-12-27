using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//The script the controls the projectile
public class Projectile : MonoBehaviour {

	//Fields
	public enum ProjectileType { BULLET, GRENADE, ORB }
	public enum DamageElement { PHYSICAL, FIRE, WATER, ICE, ELECTRIC };
	public enum DamageType { DIRECT, SPLASH }
	public enum TeamNumber { TEAM_ONE, TEAM_TWO }
	//Adjustable
	public ProjectileType projectileType;
	public DamageElement damageElement;
	public DamageType damageType;
	public float lifeTime, speed;
	public float damageAmount;
	public float splashDamageRange;
	public ParticleSystem projectParticles, hitParticles;
	//Not Adjustable
	TeamNumber teamNumber;
	List<GameObject> damageReceivers;
	NetworkPlayer owner;
	int RPCGroup;
	Vector3 direction;
	float timeAlive;
	bool isHit;
	bool damageDealt;

	#region Initialization
	[RPC]
	//RPC calls that sets the variables, this is called when the object is initialized
	void SetVariables (Vector3 givenDirection, int teamNum) {
		direction = givenDirection.normalized;
		teamNumber = (teamNum == 1) ? TeamNumber.TEAM_ONE : TeamNumber.TEAM_TWO;
		//Propels the projectile according to its projectile type
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
	}

//	[RPC]
//	//RPC calls that sets the variables, this is called when the object is initialized
//	void SetDirection (Vector3 givenDirection) {
//		direction = givenDirection.normalized;
//		//Propels the projectile according to its projectile type
//		switch (projectileType) {
//		case ProjectileType.BULLET:
//			rigidbody.AddForce (direction * speed, ForceMode.VelocityChange);
//			break;
//		case ProjectileType.GRENADE:
//			rigidbody.AddForce (direction * speed, ForceMode.Impulse);
//			break;
//		case ProjectileType.ORB:
//			rigidbody.AddForce (direction * speed, ForceMode.VelocityChange);
//			break;
//		}
//	}

	
	//RPC Helper: Setup the initial variables for the projectile
	public void InitVariables (Vector3 givenDirection, int teamNum) { 
		networkView.RPC ("SetVariables", RPCMode.AllBuffered, givenDirection, teamNum);
	}

//	//RPC Helper: Setup the initial variables for the projectile
//	public void InitDirection (Vector3 givenDirection) { 
//		networkView.RPC ("SetDirection", RPCMode.AllBuffered, givenDirection);
//	}


	//Initialized its own vatiables locally when instantiated
	void Awake () {
		projectParticles.Play (); //Play normal particles
		hitParticles.Stop (); //Stop explosion particles
		isHit = false; //Set the isHit flag to false
		timeAlive = 0f; //Initialize the time alive
		damageReceivers = new List<GameObject> ();
		damageDealt = false;
	}
	#endregion


	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) { //This is here because network.destroy can only be called once
			return;
		}
		//If the projectile hit something and the explosion animation stopped, destoy the projectile
		if (isHit == true && damageDealt == false) {
			foreach (GameObject dr in damageReceivers) {
				if (dr != null) {
					HealthManager hpManager = dr.GetComponent<HealthManager>();
					if (hpManager != null) {
						hpManager.ReceiveDamage (damageAmount, damageType, damageElement);
					}
				}
			}
			damageDealt = true;
		}
		//Destroy the projectile after it has it and its hit animation has finished
		if (isHit == true && !hitParticles.isPlaying) {
			Network.RemoveRPCs (networkView.viewID);
			Network.Destroy (gameObject);
		}
		//If the projectile did not hit anything, but it has outlived its lifetime
		else if (isHit != true && timeAlive > lifeTime) {
			foreach (GameObject dr in damageReceivers) {
				HealthManager hpManager = dr.GetComponent<HealthManager>();
				if (hpManager != null) {
					hpManager.ReceiveDamage (damageAmount, damageType, damageElement);
				}
			}
			Network.RemoveRPCs (networkView.viewID);
			Network.Destroy (gameObject);
		}
		//Check if the projectiles has hit anything
		else {
			networkView.RPC ("DetectCollision", RPCMode.AllBuffered);
		}
	}
	

	[RPC]
	//RPC call that detect collisions
	void DetectCollision () {
		switch (projectileType) {
		case ProjectileType.BULLET:
			foreach (Collider hitInfo in Physics.OverlapSphere (transform.position, GetComponent<SphereCollider>().radius+0.1f)) {
				if (!isHit && hitInfo.collider != this.collider &&
				    hitInfo.tag != "Projectiles") {
					isHit = true;
					rigidbody.isKinematic = true;
					rigidbody.detectCollisions = false;
					projectParticles.Stop ();
					hitParticles.Play ();
					GetComponent<Collider>().isTrigger = true;
					GetComponent<MeshRenderer>().enabled = false;
					if ((teamNumber == TeamNumber.TEAM_ONE && hitInfo.tag != "Team 1") ||
					    (teamNumber == TeamNumber.TEAM_TWO && hitInfo.tag != "Team 2")) {
						damageReceivers.Add (hitInfo.gameObject);
					}
				}
			}
			if (!isHit) {
				timeAlive += Time.deltaTime;
				//Debug: change the direction of the particles ----------------------------
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
				if (!isHit && hitInfo.collider != this.collider &&
				    hitInfo.collider.tag != "Projectiles" &&
				    hitInfo.collider.tag != "Ground" &&
				    hitInfo.collider.tag != "Environment" &&
				    ((teamNumber == TeamNumber.TEAM_ONE && hitInfo.tag != "Team 1") ||
				 	 (teamNumber == TeamNumber.TEAM_TWO && hitInfo.tag != "Team 2"))) {
					isHit = true;
					rigidbody.isKinematic = true;
					rigidbody.detectCollisions = false;
					projectParticles.Stop ();
					hitParticles.Play ();
					GetComponent<Collider>().isTrigger = true;
					GetComponent<MeshRenderer>().enabled = false;
				}
			}
			if (isHit) {
				damageReceivers.Clear();
				foreach (Collider hitInfo in Physics.OverlapSphere (transform.position, splashDamageRange)) {
					if (hitInfo.gameObject.GetComponent<HealthManager>() != null &&
					    ((teamNumber == TeamNumber.TEAM_ONE && hitInfo.tag != "Team 1") ||
					     (teamNumber == TeamNumber.TEAM_TWO && hitInfo.tag != "Team 2"))) {
						damageReceivers.Add (hitInfo.gameObject);
					}
				}
			}
			if (!isHit) {
				timeAlive += Time.deltaTime;
				//Debug: change the direction of the particles----------------------------
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
				if (!isHit && hitInfo.collider != this.collider &&
				    hitInfo.tag != "Projectiles") {
					isHit = true;
					rigidbody.isKinematic = true;
					rigidbody.detectCollisions = false;
					projectParticles.Stop ();
					hitParticles.Play ();
					GetComponent<Collider>().isTrigger = true;
					GetComponent<MeshRenderer>().enabled = false;
				}
			}
			if (isHit) {
				damageReceivers.Clear();
				foreach (Collider hitInfo in Physics.OverlapSphere (transform.position, splashDamageRange)) {
					if (hitInfo.gameObject.GetComponent<HealthManager>() != null &&
					    ((teamNumber == TeamNumber.TEAM_ONE && hitInfo.tag != "Team 1") ||
					 	 (teamNumber == TeamNumber.TEAM_TWO && hitInfo.tag != "Team 2"))) {
						damageReceivers.Add (hitInfo.gameObject);
					}
				}
			}
			if (!isHit) {
				timeAlive += Time.deltaTime;
				//Debug: change the direction of the particles----------------------------
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


	
}
