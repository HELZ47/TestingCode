using UnityEngine;
using System.Collections;

//Manage health and damage receiving calculations
public class HealthManager : MonoBehaviour {

	//Fields
	public float hitPoints;
	public float armourValue;
	public float fullHPAmount;

	void Awake () {
		fullHPAmount = hitPoints;
	}

	// Use this for initialization
	void Start () {
	}


	// Update is called once per frame
	void Update () {
		float remainingHPRatio = hitPoints / fullHPAmount;
		Color hColor = new Color (remainingHPRatio, remainingHPRatio, remainingHPRatio);
		if (GetComponent<Renderer>() != null) {
			renderer.material.color = hColor;
		}
		else if (GetComponentInChildren<Renderer>() != null) {
			GetComponentInChildren<Renderer>().material.color = hColor;
		}

		if (Network.isServer) {
			if (hitPoints <= 0) {
				Network.RemoveRPCs (networkView.viewID);
				Network.Destroy (gameObject);
				//networkView.RPC ("DeathForObject", RPCMode.AllBuffered); //Not fully working
			}
		}
	}

	[RPC]
	void ReduceHitpoint (float damage) {
		hitPoints -= damage;
	}

	[RPC]
	void DeathForObject () {
		Network.Destroy (gameObject);
	}

	//Receives damage
	public void ReceiveDamage (float damageAmount, Projectile.DamageType damageType, Projectile.DamageElement damageElement) {
		//hitPoints -= damageAmount;
		networkView.RPC ("ReduceHitpoint", RPCMode.AllBuffered, damageAmount);
	}
}
