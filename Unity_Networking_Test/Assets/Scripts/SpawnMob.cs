using UnityEngine;
using System.Collections;

public class SpawnMob : MonoBehaviour {

	//Fields

	// Use this for initialization
	void Start () {
		if (Network.isServer) {
			Network.Instantiate (Resources.Load ("Prefabs/Zombie"), transform.position, new Quaternion(), 0);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
