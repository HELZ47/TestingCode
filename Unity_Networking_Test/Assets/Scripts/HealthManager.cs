﻿using UnityEngine;
using System.Collections;

//Manage health and damage receiving calculations
public class HealthManager : MonoBehaviour {

	#region Fields
	public enum StatusEffect { NORMAL, CONFUSED, BURNED, FROZE, SHOCKED, SLOWED, OTHER };
	//Adjustable
	public float hitPoints;
	public float armorValue, statusRes;
	//Not adjustable
	[HideInInspector]
	public StatusEffect statusEffect;
	[HideInInspector]
	public bool isTakingDamage, isDead, deathAnimationFinished;
	[HideInInspector]
	public float fullHPAmount;
	#endregion


	//Initialize internal variables
	void Awake () {
		fullHPAmount = hitPoints;
	}


	// Update is called once per frame
	void Update () {
		//If the object is dead and its death animation is finished, the server will then remove this object
		if (Network.isServer) {
			if ((isDead && GetComponent<Animator>() == null) ||
			    (isDead && GetComponent<Animator>() != null && deathAnimationFinished)) {
				Network.RemoveRPCs (networkView.viewID);
				Network.Destroy (gameObject);
			}
		}
	}


	//Update all instances of the object with the newest HP value
	[RPC]
	void UpdateHP (float newHP) {
		hitPoints = newHP;
		isTakingDamage = true;
		if (hitPoints <= 0) {
			isDead = true;
		}
	}


	//Receives the dealt damage and perform calculations on how much of it is really going to affect the HP
	public void ReceiveDamage (float damageAmount, Projectile.DamageType damageType, Projectile.DamageElement damageElement) {
		//The server performs the calculation then synchronize it with everyone else through RPC
		if (Network.isServer) {
			//if (damageAmount - armorValue > 
			hitPoints -= damageAmount; //This is the most basic algorithm, not account for armor and whatnot
			networkView.RPC ("UpdateHP", RPCMode.AllBuffered, hitPoints);
		}
	}
}
