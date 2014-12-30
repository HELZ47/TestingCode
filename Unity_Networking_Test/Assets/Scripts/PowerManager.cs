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

	[RPC]
	void ChangeParticleSize (float pSize) {
		playerManager.particleSize = pSize;
	}

	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) {
			return;
		}

		//Debug: Print out the names of the controllers detected -----------------------------
		bool activatePower = false;
		if (Application.platform == RuntimePlatform.OSXDashboardPlayer ||
		    Application.platform == RuntimePlatform.OSXEditor ||
		    Application.platform == RuntimePlatform.OSXPlayer ||
		    Application.platform == RuntimePlatform.OSXWebPlayer) {
			activatePower = Input.GetButtonDown ("Mac_B");
		}
		else if (Application.platform == RuntimePlatform.WindowsEditor ||
		         Application.platform == RuntimePlatform.WindowsPlayer ||
		         Application.platform == RuntimePlatform.WindowsWebPlayer) {
			activatePower = Input.GetButtonDown ("Windows_B");
		}
		//--------------------------------------------------------------------------------------

		if ((activatePower || Input.GetKeyDown(KeyCode.LeftControl)) && 
		    playerManager.powerState == PlayerManager.PowerState.Normal &&
		    (rigidbody.velocity - new Vector3(0, rigidbody.velocity.y, 0)).magnitude > 1.5f) {
			//print ("Power Up");
			playerManager.particleSize = 30f;
			//networkView.RPC ("ChangeParticleSize", RPCMode.All, 30f);
			//playerManager.particleSize = 30f;
			playerManager.powerState = PlayerManager.PowerState.Boost;
			prePowerVelocity = rigidbody.velocity;
			rigidbody.velocity = (rigidbody.velocity.normalized-new Vector3(0, rigidbody.velocity.normalized.y, 0)) * 20f;
			timer = 0f;
		}
		if (playerManager.powerState == PlayerManager.PowerState.Boost && timer < 0.2f) {
			timer += Time.deltaTime;
		}
		else if (playerManager.powerState == PlayerManager.PowerState.Boost && timer >= 0.2f) {
			playerManager.particleSize = 2f;
			//networkView.RPC ("ChangeParticleSize", RPCMode.All, 2f);
			//playerManager.particleSize = 2f;
			playerManager.powerState = PlayerManager.PowerState.Normal;
			rigidbody.velocity = prePowerVelocity;
			timer = 0f;
		}
	}
}
