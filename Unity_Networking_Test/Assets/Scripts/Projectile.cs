using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//The script the controls the projectile (everything)
public class Projectile : MonoBehaviour {

	#region Fields
	//Public Types as ENUMs
	public enum ProjectileType { BULLET, GRENADE, ORB }
	public enum DamageElement { PHYSICAL, FIRE, WATER, ICE, ELECTRIC };
	public enum DamageType { DIRECT, SPLASH, MELEE }
	public enum TeamNumber { TEAM_ONE, TEAM_TWO, OTHER }
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
	#endregion


	#region Initialization
	[RPC]
	//RPC calls the server to set the variables, this is called when the object is initialized
	void SetVariables (Vector3 givenDirection, int teamNum) {
		if (Network.isServer) {
			//Set the direction and the team number of the projectile
			direction = givenDirection.normalized;
			switch (teamNum) {
			case 1:
				teamNumber = TeamNumber.TEAM_ONE;
				break;
			case 2:
				teamNumber = TeamNumber.TEAM_TWO;
				break;
			default:
				teamNumber = TeamNumber.OTHER;
				break;
			}
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
	}
	
	//RPC Helper: Setup the initial variables for the projectile
	public void InitVariables (Vector3 givenDirection, int teamNum) { 
		networkView.RPC ("SetVariables", RPCMode.AllBuffered, givenDirection, teamNum);
	}
	
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


	#region State Synchronization function
	//State synchronize function, synchronize the position, velocity and particle info
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		bool hasHit = false;
		Vector3 position = Vector3.zero;
		Vector3 velocity = Vector3.zero;
		if (stream.isWriting) {
			hasHit = rigidbody.isKinematic;
			position = transform.position;
			if (!hasHit) {
				velocity = rigidbody.velocity;
			}
			stream.Serialize(ref hasHit);
			stream.Serialize(ref position);
			stream.Serialize(ref velocity);
		} 
		else {
			stream.Serialize(ref hasHit);
			stream.Serialize(ref position);
			stream.Serialize(ref velocity);
			transform.position = position;
			if (hasHit) {
				rigidbody.isKinematic = true;
				rigidbody.detectCollisions = false;
				projectParticles.Stop ();
				hitParticles.Play ();
				GetComponent<Collider>().isTrigger = true;
				GetComponent<MeshRenderer>().enabled = false;
			}
			else {
				rigidbody.velocity = velocity;
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
		}
	}
	#endregion

	
	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) { //This function is for server only
			return;
		}
		//If the projectile hit something, deals damage to the damage recievers
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
		//Destroy the projectile after it has hit, its hit animation has finished and damages dealt
		if (isHit == true && !hitParticles.isPlaying && damageDealt) {
			Network.RemoveRPCs (networkView.viewID);
			Network.Destroy (gameObject);
		}
		//If the projectile did not hit anything, but it has outlived its lifetime, destroy it and deals splash damage
		else if (isHit != true && timeAlive > lifeTime) {
			foreach (GameObject dr in damageReceivers) {
				if (dr!= null) {
					HealthManager hpManager = dr.GetComponent<HealthManager>();
					if (hpManager != null) {
						hpManager.ReceiveDamage (damageAmount, damageType, damageElement);
					}
				}
			}
			Network.RemoveRPCs (networkView.viewID);
			Network.Destroy (gameObject);
		}
		//If the projectile is still live, check whether it has targetted anything
		else {
			DetectCollision ();
		}
	}
	

	/*The update function that check whether the projectile has hit anything, every
	 kind of projectiles have different behaviors*/
	void DetectCollision () {
		switch (projectileType) {
		//Bullets explode on contact, but affect only the contacted object
		case ProjectileType.BULLET:
			foreach (Collider hitInfo in Physics.OverlapSphere (transform.position, GetComponent<SphereCollider>().radius+0.1f)) {
				//Bullets hit anything except itself and friendlies
				if (!isHit && hitInfo.collider != this.collider &&
				    hitInfo.tag != "Projectiles" &&
				    ((teamNumber == TeamNumber.TEAM_ONE && hitInfo.tag != "Team 1") ||
				     (teamNumber == TeamNumber.TEAM_TWO && hitInfo.tag != "Team 2"))) {
					isHit = true;
					rigidbody.isKinematic = true; //Stop rigidbody from functioning
					rigidbody.detectCollisions = false; //Stop rigidbody from detecting collisions
					projectParticles.Stop (); //Stop the normal particles
					hitParticles.Play (); //Start the explosion particles
					GetComponent<Collider>().isTrigger = true; //Turn off the collider
					GetComponent<MeshRenderer>().enabled = false; //Turn off the mesh
					//If the contacted object is not on the same team, damage it
					if (hitInfo.GetComponent <HealthManager>() != null &&
						(teamNumber == TeamNumber.TEAM_ONE && hitInfo.tag != "Team 1") ||
					    (teamNumber == TeamNumber.TEAM_TWO && hitInfo.tag != "Team 2")) {
						damageReceivers.Add (hitInfo.gameObject);
					}
				}
			}
			//If the projectile hasn't hit anything, increase timer and update particles
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
		//Grenades explode only when contacting the enemie, or when timer expires
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

			//Update the list of damage receivers regardless of isHit, since grenade can explode at anytime
			damageReceivers.Clear();
			foreach (Collider hitInfo in Physics.OverlapSphere (transform.position, splashDamageRange)) {
				if (hitInfo.gameObject.GetComponent<HealthManager>() != null &&
				    ((teamNumber == TeamNumber.TEAM_ONE && hitInfo.tag != "Team 1") ||
				     (teamNumber == TeamNumber.TEAM_TWO && hitInfo.tag != "Team 2"))) {
					damageReceivers.Add (hitInfo.gameObject);
				}
			}

			//If the projectile hasn't hit anything, increase timer and update the particles
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
			
				//If the timer expires, the grenade explodes
				if (timeAlive > lifeTime) {
					isHit = true;
					rigidbody.isKinematic = true;
					rigidbody.detectCollisions = false;
					projectParticles.Stop ();
					hitParticles.Play ();
					GetComponent<Collider>().isTrigger = true;
					GetComponent<MeshRenderer>().enabled = false;
				}
			}
			break;
		//Orb travels at slower speed, but cause high splash damage
		case ProjectileType.ORB:
			//Orb only connects with anything except itself and friendlies
			foreach (Collider hitInfo in Physics.OverlapSphere (transform.position, GetComponent<SphereCollider>().radius+0.1f)) {
				if (!isHit && hitInfo.collider != this.collider &&
				    hitInfo.tag != "Projectiles" && 
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

			//Calculate the potential targets that are within range
			damageReceivers.Clear();
			foreach (Collider hitInfo in Physics.OverlapSphere (transform.position, splashDamageRange)) {
				if (hitInfo.gameObject.GetComponent<HealthManager>() != null &&
				    ((teamNumber == TeamNumber.TEAM_ONE && hitInfo.tag != "Team 1") ||
				 	 (teamNumber == TeamNumber.TEAM_TWO && hitInfo.tag != "Team 2"))) {
					damageReceivers.Add (hitInfo.gameObject);
				}
			}

			//If the projectile hasn't hit anything, increase timer and update projectiles
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
