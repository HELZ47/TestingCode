using UnityEngine;
using System.Collections;

//Manage health and damage receiving calculations
public class HealthManager : MonoBehaviour {

	//Fields
	public float hitPoints;
	public float armourValue;
	float fullHPAmount;


	// Use this for initialization
	void Start () {
		fullHPAmount = hitPoints;
	}


	// Update is called once per frame
	void Update () {
		float remainingHPRatio = hitPoints / fullHPAmount;
		Color hColor = new Color (remainingHPRatio, remainingHPRatio, remainingHPRatio);
		renderer.material.color = hColor;

		if (networkView.isMine) {
			if (hitPoints < 0) {
//				Network.RemoveRPCs (networkView.viewID);
//				Network.Destroy (gameObject);
				networkView.RPC ("DeathForObject", RPCMode.AllBuffered);
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
