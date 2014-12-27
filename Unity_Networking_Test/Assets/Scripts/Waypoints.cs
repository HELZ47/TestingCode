using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Waypoints : MonoBehaviour {

	//Fields

	//Adjustable
	public List<Vector3> waypoints;

	//Non Adjustable
	public Vector3 startPoint, endPoint;

	// Use this for initialization
	void Awake () {
		startPoint = waypoints [0];
		endPoint = waypoints [waypoints.Count - 1];
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmos () {
		Gizmos.color = Color.magenta;
		foreach (Vector3 pos in waypoints) {
			Gizmos.DrawSphere (pos, 1f);
		}
	}
}
