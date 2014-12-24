﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//The script the controls the projectile
public class Projectile : MonoBehaviour {

	//Fields
	public enum ProjectileType { BULLET, GRENADE, ORB }
	public enum DamageElement { PHYSICAL, FIRE, WATER, ICE, ELECTRIC };
	public enum DamageType { DIRECT, SPLASH }
	public ProjectileType projectileType;
	public ParticleSystem projectParticles, hitParticles;
	public float lifeTime, speed;
	public DamageElement damageElement;
	public DamageType damageType;
	public float damageAmount;
	public float splashDamageRange;
	List<GameObject> damageReceivers;
	NetworkPlayer owner;
	int RPCGroup;
	Vector3 direction;
	float timeAlive;
	bool isHit;

	#region Initialization
	[RPC]
	//RPC calls that sets the variables, this is called when the object is initialized
	void SetVariables (Vector3 givenDirection, NetworkPlayer ownerNP) {
		direction = givenDirection.normalized;
		owner = ownerNP;
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

	
	//RPC Helper: Setup the initial variables for the projectile
	public void InitVariables (Vector3 givenDirection, NetworkPlayer ownerNP) { 
		networkView.RPC ("SetVariables", RPCMode.AllBuffered, givenDirection, ownerNP);
	}


	//Initialized its own vatiables locally when instantiated
	void Awake () {
		projectParticles.Play (); //Play normal particles
		hitParticles.Stop (); //Stop explosion particles
		isHit = false; //Set the isHit flag to false
		timeAlive = 0f; //Initialize the time alive
		damageReceivers = new List<GameObject> ();
	}


	// Use this for initialization
	void Start () {
	}
	#endregion


	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) { //This is here because network.destroy can only be called once
			return;
		}
		//If the projectile hit something and the explosion animation stopped, destoy the projectile
		if (isHit == true && !hitParticles.isPlaying) {
			foreach (GameObject dr in damageReceivers) {
				if (dr != null) {
					HealthManager hpManager = dr.GetComponent<HealthManager>();
					if (hpManager != null) {
						hpManager.ReceiveDamage (damageAmount, damageType, damageElement);
					}
				}
			}
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
				if (hitInfo.collider != this.collider &&
				    (!hitInfo.collider.gameObject.GetComponentInParent<NetworkView>() ||
				 	 hitInfo.collider.gameObject.GetComponentInParent<NetworkView>().owner != owner ||
				 	 hitInfo.collider.tag == "Mobs") &&
				    !isHit && hitInfo.collider.tag != "Projectiles") {
					isHit = true;
					rigidbody.isKinematic = true;
					rigidbody.detectCollisions = false;
					projectParticles.Stop ();
					hitParticles.Play ();
					GetComponent<Collider>().isTrigger = true;
					GetComponent<MeshRenderer>().enabled = false;
					damageReceivers.Add (hitInfo.gameObject);
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
				if (hitInfo.collider != this.collider &&
				    hitInfo.collider.gameObject.GetComponentInParent<NetworkView>() &&
				 	(hitInfo.collider.gameObject.GetComponentInParent<NetworkView>().owner != owner ||
				 	 hitInfo.collider.tag == "Mobs") &&
				    !isHit && hitInfo.collider.tag != "Projectiles") {
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
					if (hitInfo.collider != this.collider &&
					    hitInfo.collider.gameObject.GetComponentInParent<NetworkView>() &&
					    (hitInfo.collider.gameObject.GetComponentInParent<NetworkView>().owner != owner ||
					 	 hitInfo.collider.tag == "Mobs") &&
					    hitInfo.collider.tag != "Projectiles") {
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
				if (hitInfo.collider != this.collider &&
				    (!hitInfo.collider.gameObject.GetComponentInParent<NetworkView>() ||
				 	 hitInfo.collider.gameObject.GetComponentInParent<NetworkView>().owner != owner ||
				 	 hitInfo.collider.tag == "Mobs") &&
				    !isHit && hitInfo.collider.tag != "Projectiles") {
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
					if (hitInfo.collider != this.collider &&
					    hitInfo.collider.gameObject.GetComponentInParent<NetworkView>() &&
					    (hitInfo.collider.gameObject.GetComponentInParent<NetworkView>().owner != owner ||
					 hitInfo.collider.tag == "Mobs") &&
					    hitInfo.collider.tag != "Projectiles") {
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
