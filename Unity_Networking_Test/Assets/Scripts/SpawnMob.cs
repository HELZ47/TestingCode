using UnityEngine;
using System.Collections;

public class SpawnMob : MonoBehaviour {

	//Fields
	public enum SpawnType { ZOMBIE, ROBOT };
	public SpawnType spawnType;

	// Use this for initialization
	void Start () {
		if (Network.isServer) {
			switch (spawnType) {
			case SpawnType.ROBOT:
				Network.Instantiate (Resources.Load ("Prefabs/Robot"), transform.position, new Quaternion(), 0);
				break;
			case SpawnType.ZOMBIE:
				Network.Instantiate (Resources.Load ("Prefabs/Zombie"), transform.position, new Quaternion(), 0);
				break;
			}

		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
