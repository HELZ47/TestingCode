using UnityEngine;
using System.Collections;

//Handles when/where the tower attacks
public class TowerAttack : MonoBehaviour {

	#region Fields
	//Adjustable
	public Vector3 direction; //Projectile direction

	//Not Adjustable
	float timerBetweenShots;
	TowerManager myTowerManager;
	#endregion

	
	//RPC call that creates the projectile on the server side
	[RPC] //In the script right now, RPC is not called, this function is rather called as a normal function
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


	// Initialize internal variables
	void Awake () {
		myTowerManager = GetComponent<TowerManager> ();
	}


	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) {
			return;
		}
		/*Targeting Behaviour: target the closest enemy, but keep attacking that target until
		 either it dies or it leaves the attack range. It is only then that the tower recalculates
		 its target.*/
		if (myTowerManager.targetAquired && myTowerManager.targetTransform != null &&
		    !myTowerManager.targetTransform.gameObject.GetComponent<HealthManager>().isDead &&
		    Vector3.Distance (transform.position, myTowerManager.targetTransform.position) < myTowerManager.rangeOfAttack) {
			if (timerBetweenShots > myTowerManager.timeBetweenShotsInSecond) {
				timerBetweenShots = 0f;
				direction = (myTowerManager.targetTransform.position - myTowerManager.projectileStartPosition).normalized;
				CreateProjectile ("Prefabs/Lightning_Orb", myTowerManager.projectileStartPosition + direction*2f, new Quaternion(), direction);
			}
			else {
				timerBetweenShots += Time.deltaTime;
			}
		}
		else {
			myTowerManager.targetAquired = false;
			myTowerManager.targetTransform = null;
			Collider[] potentialTargets = Physics.OverlapSphere (transform.position, myTowerManager.rangeOfAttack);
			float shortestDistance = 9999f;
			foreach (Collider col in potentialTargets) {
				if ((tag == "Team 1" && col.tag == "Team 2") ||
				    (tag == "Team 2" && col.tag == "Team 1") ||
				    col.tag == "Mobs") {
					if (Vector3.Distance (col.gameObject.transform.position, transform.position) < shortestDistance) {
						myTowerManager.targetAquired = true;
						myTowerManager.targetTransform = col.gameObject.transform;
						shortestDistance = Vector3.Distance (col.gameObject.transform.position, transform.position);
					}
				}
			}
		}
//		myTowerManager.targetAquired = false;
//		Collider[] potentialTargets = Physics.OverlapSphere (transform.position, myTowerManager.rangeOfAttack);
//		float shortestDistance = 9999f;
//		foreach (Collider col in potentialTargets) {
//			if ((tag == "Team 1" && col.tag == "Team 2") ||
//			    (tag == "Team 2" && col.tag == "Team 1") ||
//			    col.tag == "Mobs") {
//				if (Vector3.Distance (col.gameObject.transform.position, transform.position) < shortestDistance) {
//					myTowerManager.targetAquired = true;
//					myTowerManager.targetTransform = col.gameObject.transform;
//					shortestDistance = Vector3.Distance (col.gameObject.transform.position, transform.position);
//				}
//			}
//		}
//		if (myTowerManager.targetAquired && timerBetweenShots > myTowerManager.timeBetweenShotsInSecond &&
//		    myTowerManager.targetTransform != null) {
//			timerBetweenShots = 0f;
//			direction = (myTowerManager.targetTransform.position - myTowerManager.projectileStartPosition).normalized;
//			//direction = Vector3.right;
//			CreateProjectile ("Prefabs/Lightning_Orb", myTowerManager.projectileStartPosition + direction*2f, new Quaternion(), direction);
//		}
//		else {
//			timerBetweenShots += Time.deltaTime;
//		}
	}
}
