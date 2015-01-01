using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//Handles enemy/Bots' health display
public class HealthUIManager : MonoBehaviour {

	//Fields
	public Image healthForground; //The one on top
	public Image healthBackground; //The on on the bottom
	public float UIDisplayTime; //Period of time where health is displayed after taking damage
	Camera mainCamera; //The player's camera that healthforground/background are facing
	HealthManager myHPManager; //The health manager of the current object that this display reflect on
	float UITimer; //Counts the amound of time the UI is displayed on screed

	// Use this for initialization
	void Awake () {
		myHPManager = GetComponent<HealthManager> ();
	}

	// Initialize external variables
	void Start () {
		foreach (Camera c in GameObject.FindObjectsOfType<Camera>()) {
			if (c.GetComponentInParent<NetworkView>().isMine) {
				mainCamera = c;
			}
		}
		UITimer = 9999f; //Now the UI is by default disabled
	}
	
	// Update is called once per frame
	void Update () {
		if (!myHPManager.isDead) {
			//If not dead -> display UI and reset timer
			if (myHPManager.isTakingDamage) {
				myHPManager.isTakingDamage = false; //Set health manager's flag to false
				UITimer = 0f;
				healthForground.enabled = true;
				healthBackground.enabled = true;
				healthForground.fillAmount = myHPManager.hitPoints / myHPManager.fullHPAmount;
				healthForground.rectTransform.LookAt (mainCamera.transform.position);
				healthBackground.rectTransform.LookAt (mainCamera.transform.position);
			}
			//If timer < time, display UI
			else {
				if (UITimer < UIDisplayTime) {
					UITimer += Time.deltaTime;
					healthForground.enabled = true;
					healthBackground.enabled = true;
					healthForground.fillAmount = myHPManager.hitPoints / myHPManager.fullHPAmount;
					healthForground.rectTransform.LookAt (mainCamera.transform.position);
					healthBackground.rectTransform.LookAt (mainCamera.transform.position);
				}
				//If timer>time, hide UI
				else {
					healthForground.enabled = false;
					healthBackground.enabled = false;
				}
			}
		}
		//Hide UI when object is dead, so that it won't show during the death sequence
		else {
			healthForground.enabled = false;
			healthBackground.enabled = false;
		}
	}
}
