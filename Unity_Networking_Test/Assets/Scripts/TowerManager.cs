using UnityEngine;
using System.Collections;

public class TowerManager : MonoBehaviour {

	//Fields
	//Adjustable
	public float timeBetweenShotsInSecond;
	public float rangeOfAttack;
	public GameObject startPosObject;
	//Not Adjustable
	public bool targetAquired;
	public Transform targetTransform;
	public Vector3 projectileStartPosition;

	// Use this for initialization
	void Awake () {
		projectileStartPosition = startPosObject.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
