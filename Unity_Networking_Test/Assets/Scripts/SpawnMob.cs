using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnMob : MonoBehaviour {

	//Fields
	public enum SpawnType { ZOMBIE, ROBOT, ROBOT_RANGED, TOWER };
	public enum SpawnNumber { GLOBAL, LOCAL, SINGLE };
	public SpawnType spawnType;
	public SpawnNumber spawnNumber;
	public int numOfSpawns;
	public Vector2 worldSize;
	public List<Vector3> spawnPositions;

	// Use this for initialization
	void Start () {
		spawnPositions = new List<Vector3> ();
		if (Network.isServer) {
			switch (spawnNumber) {
			case SpawnNumber.SINGLE:
				switch (spawnType) {
				case SpawnType.ROBOT:
					Network.Instantiate (Resources.Load ("Prefabs/Robot"), transform.position, new Quaternion(), 0);
					break;
				case SpawnType.ZOMBIE:
					Network.Instantiate (Resources.Load ("Prefabs/Zombie"), transform.position, new Quaternion(), 0);
					break;
				case SpawnType.TOWER:
					Network.Instantiate (Resources.Load ("Prefabs/Tower"), transform.position, new Quaternion(), 0);
					break;
				case SpawnType.ROBOT_RANGED:
					Network.Instantiate (Resources.Load ("Prefabs/Robot_Ranged"), transform.position, new Quaternion(), 0);
					break;
				}
				break;
			case SpawnNumber.GLOBAL:
				for (int i = 0; i < numOfSpawns; i++) {
					bool tooClose = false;
					Vector3 spawnPosition = Vector3.zero;
					do {
						spawnPosition = new Vector3 (Random.Range(-worldSize.x/2, worldSize.x/2), 0, Random.Range(-worldSize.y/2, worldSize.y/2));
						tooClose = false;
						foreach (Vector3 pos in spawnPositions) {
							if (Vector3.Distance (spawnPosition, pos) < 2) {
								tooClose = true;
							}
						}
						foreach (Collider col in Physics.OverlapSphere (spawnPosition, 2f)) {
							if ((col.gameObject.GetComponent<PlayerManager>() != null ||
							     col.gameObject.GetComponent<HealthManager>() != null) &&
							    col.tag != "Ground" && col.tag != "Environment") {
								tooClose = true;
							}
						}
					} while (tooClose);

					spawnPositions.Add (spawnPosition);

					switch (spawnType) {
					case SpawnType.ROBOT:
						Network.Instantiate (Resources.Load ("Prefabs/Robot"), spawnPosition, new Quaternion(), 0);
						break;
					case SpawnType.ZOMBIE:
						Network.Instantiate (Resources.Load ("Prefabs/Zombie"), spawnPosition, new Quaternion(), 0);
						break;
					case SpawnType.TOWER:
						Network.Instantiate (Resources.Load ("Prefabs/Tower"), spawnPosition, new Quaternion(), 0);
						break;
					case SpawnType.ROBOT_RANGED:
						Network.Instantiate (Resources.Load ("Prefabs/Robot_Ranged"), spawnPosition, new Quaternion(), 0);
						break;
					}

				}

				break;
			case SpawnNumber.LOCAL:
				for (int i = 0; i < numOfSpawns; i++) {
					bool tooClose = false;
					Vector3 spawnPosition = Vector3.zero;
					do {
						spawnPosition = new Vector3 (Random.Range((-worldSize.x/2f) + transform.position.x, (worldSize.x/2f) + transform.position.x), 0, Random.Range((-worldSize.y/2f) + transform.position.z, (worldSize.y/2f) + transform.position.z));
						tooClose = false;
						foreach (Vector3 pos in spawnPositions) {
							if (Vector3.Distance (spawnPosition, pos) < 2) {
								tooClose = true;
							}
						}
						foreach (Collider col in Physics.OverlapSphere (spawnPosition, 2f)) {
							if ((col.gameObject.GetComponent<PlayerManager>() != null ||
							     col.gameObject.GetComponent<HealthManager>() != null) &&
							    col.tag != "Ground" && col.tag != "Environment") {
								tooClose = true;
							}
						}
					} while (tooClose);

					spawnPositions.Add (spawnPosition);

					switch (spawnType) {
					case SpawnType.ROBOT:
						Network.Instantiate (Resources.Load ("Prefabs/Robot"), spawnPosition, new Quaternion(), 0);
						break;
					case SpawnType.ZOMBIE:
						Network.Instantiate (Resources.Load ("Prefabs/Zombie"), spawnPosition, new Quaternion(), 0);
						break;
					case SpawnType.TOWER:
						Network.Instantiate (Resources.Load ("Prefabs/Tower"), spawnPosition, new Quaternion(), 0);
						break;
					case SpawnType.ROBOT_RANGED:
						Network.Instantiate (Resources.Load ("Prefabs/Robot_Ranged"), spawnPosition, new Quaternion(), 0);
						break;
					}
					
				}
				
				break;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmos () {

		switch (spawnType) {
		case SpawnType.ROBOT:
			Gizmos.color = Color.green;
			break;
		case SpawnType.ROBOT_RANGED:
			Gizmos.color = Color.yellow;
			break;
		case SpawnType.ZOMBIE:
			Gizmos.color = Color.red;
			break;
		case SpawnType.TOWER:
			Gizmos.color = Color.blue;
			break;
		}
		if (spawnNumber == SpawnNumber.LOCAL) {
			Gizmos.DrawCube (transform.position, new Vector3 (worldSize.x, 0.2f, worldSize.y));
		}
		else if (spawnNumber == SpawnNumber.SINGLE) {
			Gizmos.DrawSphere (transform.position, 1f);
		}
	}
}
