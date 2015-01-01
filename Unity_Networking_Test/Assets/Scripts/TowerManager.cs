using UnityEngine;
using System.Collections;

//The main manager for the towers
public class TowerManager : MonoBehaviour {

	#region Fields
	//Adjustable
	public float timeBetweenShotsInSecond; //Fire rate
	public float rangeOfAttack; //Fire range
	public GameObject startPosObject; //Object which position the projectile starts at

	//Not Adjustable
	[HideInInspector]
	public bool targetAquired;
	public Transform targetTransform;
	public Vector3 projectileStartPosition;
	#endregion


	// Initialize internal valriables
	void Awake () {
		//Get the projectile start position from the start object
		projectileStartPosition = startPosObject.transform.position;
	}
}
