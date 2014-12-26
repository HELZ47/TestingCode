using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnMob : MonoBehaviour {

	//Fields
	public enum SpawnType { ZOMBIE, ROBOT };
	public enum SpawnNumber { MASSIVE, SINGLE };
	public SpawnType spawnType;
	public SpawnNumber spawnNumber;
	public int numOfSpawns;
	public Vector2 worldSize;
	public List<GameObject> spawns;

	// Use this for initialization
	void Start () {
		spawns = new List<GameObject> ();
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
				}
				break;
			case SpawnNumber.MASSIVE:
				for (int i = 0; i < numOfSpawns; i++) {
					bool tooClose = false;
					Vector3 spawnPosition = Vector3.zero;
					do {
						spawnPosition = new Vector3 (Random.Range(-worldSize.x/2, worldSize.x/2), 0, Random.Range(-worldSize.y/2, worldSize.y/2));
						tooClose = false;
						foreach (GameObject go in spawns) {
							if (Vector3.Distance (spawnPosition, go.transform.position) < 2) {
								tooClose = true;
							}
						}
						foreach (Collider col in Physics.OverlapSphere (spawnPosition, 2f)) {
							if (col.gameObject.GetComponent<PlayerManager>() != null ||
							    col.gameObject.GetComponent<HealthManager>() != null) {
								tooClose = true;
							}
						}
					} while (tooClose);

					switch (spawnType) {
					case SpawnType.ROBOT:
						Network.Instantiate (Resources.Load ("Prefabs/Robot"), spawnPosition, new Quaternion(), 0);
						break;
					case SpawnType.ZOMBIE:
						Network.Instantiate (Resources.Load ("Prefabs/Zombie"), spawnPosition, new Quaternion(), 0);
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
}
