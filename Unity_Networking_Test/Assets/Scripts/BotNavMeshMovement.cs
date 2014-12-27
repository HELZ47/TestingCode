using UnityEngine;
using System.Collections;

public class BotNavMeshMovement : MonoBehaviour {

	//Fields
	public Waypoints wayPoints;
	BotManager myBotManager;
	NavMeshAgent myNavMeshAgent;
	Vector3 currentWayPoint;
	int currentWayPointIndex;

	// Use this for initialization
	void Awake () {
		myBotManager = GetComponent<BotManager> ();
		myNavMeshAgent = GetComponent<NavMeshAgent> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Vector3.Distance(wayPoints.waypoints[currentWayPointIndex], transform.position) < 2f) {
			currentWayPointIndex++;
			if (currentWayPointIndex > wayPoints.waypoints.Count - 1) {
				currentWayPointIndex = 0;
			}
		}
		myNavMeshAgent.SetDestination (wayPoints.waypoints [currentWayPointIndex]);
	}
}
