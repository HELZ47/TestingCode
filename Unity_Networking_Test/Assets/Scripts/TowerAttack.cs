﻿using UnityEngine;
using System.Collections;

public class TowerAttack : MonoBehaviour {

	//Fields
	//Adjustable
	public Vector3 direction;
	//Not Adjustable
	float timerBetweenShots;
	TowerManager myTowerManager;

	[RPC]
	//RPC call that creates the projectile on the server side
	public void CreateProjectile (string source, Vector3 position, Quaternion rotation, Vector3 direction) {
		if (Network.isServer) {
			GameObject projectile = Network.Instantiate (Resources.Load(source), position, rotation, 0) as GameObject;
			int teamNum = 99;
			if (tag == "Team 1") {
				teamNum = 1;
			}
			else if (tag == "Team 2") {
				teamNum = 2;
			}
			projectile.GetComponent<Projectile>().InitVariables (direction.normalized, teamNum);
		}
	}

	// Use this for initialization
	void Awake () {
		myTowerManager = GetComponent<TowerManager> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) {
			return;
		}
		Collider[] potentialTargets = Physics.OverlapSphere (transform.position, myTowerManager.rangeOfAttack);
		float shortestDistance = 9999f;
		foreach (Collider col in potentialTargets) {
			if (col.tag == "Mobs") {
				if (shortestDistance == 9999f) {
					myTowerManager.targetAquired = true;
					myTowerManager.targetTransform = col.gameObject.transform;
					shortestDistance = Vector3.Distance (col.gameObject.transform.position, transform.position);
				}
				else if (Vector3.Distance (col.gameObject.transform.position, transform.position) < shortestDistance) {
					myTowerManager.targetAquired = true;
					myTowerManager.targetTransform = col.gameObject.transform;
					shortestDistance = Vector3.Distance (col.gameObject.transform.position, transform.position);
				}
			}
		}
		if (myTowerManager.targetAquired && timerBetweenShots > myTowerManager.timeBetweenShotsInSecond &&
		    myTowerManager.targetTransform != null) {
			timerBetweenShots = 0f;
			direction = (myTowerManager.targetTransform.position - myTowerManager.projectileStartPosition).normalized;
			//direction = Vector3.right;
			CreateProjectile ("Prefabs/Lightning_Orb", myTowerManager.projectileStartPosition + direction*2f, new Quaternion(), direction);
		}
		else {
			timerBetweenShots += Time.deltaTime;
		}
	}
}
