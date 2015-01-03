using UnityEngine;
using System.Collections;

public class BotStatusManager : MonoBehaviour {

	#region Fields
	//Adjustable

	//Not Adjustable
	BotManager myBotManager;
	HealthManager myHPManager;
	float healthEffectTimer;
	HealthManager.StatusEffect prevStatus;
	#endregion


	// Use this for initialization
	void Awake () {
		myBotManager = GetComponent<BotManager> ();
		myHPManager = GetComponent<HealthManager> ();
		healthEffectTimer = 0f;
	}


	//RPC call that plays a certain particle effect that reflect on the status
	[RPC]
	void UpdateStatusEffect (int status) {
		HealthManager.StatusEffect sEffect = (HealthManager.StatusEffect)status;
		myBotManager.burnedP.Stop ();
		myBotManager.frozenP.Stop ();
		myBotManager.shockedP.Stop ();
		myBotManager.slowedP.Stop ();
		myBotManager.confusedP.Stop ();
		switch (sEffect) {
		case HealthManager.StatusEffect.BURNED:
			myBotManager.burnedP.Play();
			break;
		}
	}


	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) {
			return;
		}

		//If the status effect has changed since last update, reset the timer
		if (myHPManager.statusEffect != prevStatus) {
			healthEffectTimer = 0f;
		}

		switch (myHPManager.statusEffect) {
		case HealthManager.StatusEffect.NORMAL:
			myBotManager.burnedP.Stop ();
			myBotManager.frozenP.Stop ();
			myBotManager.shockedP.Stop ();
			myBotManager.slowedP.Stop ();
			myBotManager.confusedP.Stop ();
			healthEffectTimer = 0f;
			break;
		case HealthManager.StatusEffect.BURNED:
			myBotManager.burnedP.Play ();
			myBotManager.frozenP.Stop ();
			myBotManager.shockedP.Stop ();
			myBotManager.slowedP.Stop ();
			myBotManager.confusedP.Stop ();
			if (healthEffectTimer > 3f) {
				myHPManager.statusEffect = HealthManager.StatusEffect.NORMAL;
				healthEffectTimer = 0f;
			}
			else {
				healthEffectTimer += Time.deltaTime;
			}
			break;
		}

		prevStatus = myHPManager.statusEffect;
	}
}
