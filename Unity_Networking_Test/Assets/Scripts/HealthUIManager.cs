using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HealthUIManager : MonoBehaviour {

	//Fields
	//public Canvas UICanvas;
	public Image healthForground;
	public Image healthBackground;
	public float UIDisplayTime;
	Camera mainCamera;
	HealthManager myHPManager;
	float UITimer;

	// Use this for initialization
	void Awake () {
		myHPManager = GetComponent<HealthManager> ();
	}

	// Initialize variables
	void Start () {
		foreach (Camera c in GameObject.FindObjectsOfType<Camera>()) {
			if (c.GetComponentInParent<NetworkView>().isMine) {
				mainCamera = c;
			}
		}
		UITimer = 9999f;
	}
	
	// Update is called once per frame
	void Update () {
		if (!myHPManager.isDead) {
			if (myHPManager.isTakingDamage) {
				myHPManager.isTakingDamage = false;
				UITimer = 0f;
				healthForground.enabled = true;
				healthBackground.enabled = true;
				healthForground.fillAmount = myHPManager.hitPoints / myHPManager.fullHPAmount;
				healthForground.rectTransform.LookAt (mainCamera.transform.position);
				healthBackground.rectTransform.LookAt (mainCamera.transform.position);
			}
			else {
				if (UITimer < UIDisplayTime) {
					UITimer += Time.deltaTime;
					healthForground.enabled = true;
					healthBackground.enabled = true;
					healthForground.fillAmount = myHPManager.hitPoints / myHPManager.fullHPAmount;
					healthForground.rectTransform.LookAt (mainCamera.transform.position);
					healthBackground.rectTransform.LookAt (mainCamera.transform.position);
				}
				else {
					healthForground.enabled = false;
					healthBackground.enabled = false;
				}
			}
		}
		else {
			healthForground.enabled = false;
			healthBackground.enabled = false;
		}
	}
}
