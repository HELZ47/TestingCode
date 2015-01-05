using UnityEngine;
using System.Collections;

public class CameraEnabler : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(GetComponentInParent<NetworkView>().isMine){
			GetComponent<Camera>().enabled = true;
			GetComponent<AudioListener>().enabled = true;
			GetComponentInChildren <Canvas>().enabled = true;
			//gameObject.SetActive(false);
		}
		else{
			//gameObject.SetActive(false);
			GetComponent<Camera>().enabled = false;
			GetComponent<AudioListener>().enabled = false;
			GetComponentInChildren <Canvas>().enabled = false;
		}
	}
}
