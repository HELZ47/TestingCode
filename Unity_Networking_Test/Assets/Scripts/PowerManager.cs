using UnityEngine;
using System.Collections;

public class PowerManager : MonoBehaviour {

	//Fields
	public PlayerManager playerManager;
	float timer;
	Vector3 prePowerVelocity;

	// Use this for initialization
	void Start () {
		playerManager = GetComponent<PlayerManager> ();
	}


	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) {
			return;
		}
		if ((Input.GetButtonDown ("Mac_B") || Input.GetKeyDown(KeyCode.Q)) && 
		    playerManager.powerState == PlayerManager.PowerState.Normal &&
		    (rigidbody.velocity - new Vector3(0, rigidbody.velocity.y, 0)).magnitude > 1.5f) {
			print ("Power Up");
			playerManager.particleSize = 10f;
			playerManager.powerState = PlayerManager.PowerState.Boost;
			prePowerVelocity = rigidbody.velocity;
			rigidbody.velocity = (rigidbody.velocity.normalized-new Vector3(0, rigidbody.velocity.normalized.y, 0)) * 50f;
			timer = 0f;
		}
		if (playerManager.powerState == PlayerManager.PowerState.Boost && timer < 0.2f) {
			timer += Time.deltaTime;
		}
		else if (playerManager.powerState == PlayerManager.PowerState.Boost && timer >= 0.2f) {
			playerManager.particleSize = 2f;
			playerManager.powerState = PlayerManager.PowerState.Normal;
			rigidbody.velocity = prePowerVelocity;
			timer = 0f;
		}
	}
}
