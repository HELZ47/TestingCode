using UnityEngine;
using System.Collections;

public class ReticleSize : MonoBehaviour {

	//Fields
	Vector3 initialScale;
	float initialFOV;
	Camera mainCamera;


	// Use this for initialization
	void Start () {
    	mainCamera = GetComponentInParent<PlayerManager>().mainCamera;
		initialFOV = mainCamera.fieldOfView;
		initialScale = transform.localScale;
	}


	// Update is called once per frame
	void Update () {
		transform.localScale = (initialScale * mainCamera.fieldOfView) / initialFOV;
	}
}
