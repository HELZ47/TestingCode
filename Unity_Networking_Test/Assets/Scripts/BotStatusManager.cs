using UnityEngine;
using System.Collections;

public class BotStatusManager : MonoBehaviour {

	#region Fields
	//Adjustable

	//Not Adjustable
	BotManager myBotManager;
	HealthManager myHPManager;
	float healthEffectTimer, gradualDamageTimer;
	HealthManager.StatusEffect prevStatus;
	float initSpeed, initAcceleration, initFireRate;
	#endregion


	#region Initalization
	// Use this for initialization
	void Awake () {
		myBotManager = GetComponent<BotManager> ();
		myHPManager = GetComponent<HealthManager> ();
		healthEffectTimer = 0f;
	}

	//Initialize external variables
	void Start () {
		initSpeed = myBotManager.movementSpeed;
		initAcceleration = myBotManager.acceleration;
		initFireRate = myBotManager.timeBetweenAttacksInSeconds;
	}
	#endregion


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
			gradualDamageTimer = 0f;
		}

		switch (myHPManager.statusEffect) {
		case HealthManager.StatusEffect.NORMAL:
			myBotManager.burnedP.Stop ();
			myBotManager.frozenP.Stop ();
			myBotManager.shockedP.Stop ();
			myBotManager.slowedP.Stop ();
			myBotManager.confusedP.Stop ();
			healthEffectTimer = 0f;
			gradualDamageTimer = 0f;
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
				gradualDamageTimer = 0f;
			}
			else {
				gradualDamageTimer += Time.deltaTime;
				if (gradualDamageTimer > 0.5f) {
					gradualDamageTimer = 0f;
					myHPManager.ReceiveDamage (10, Projectile.DamageType.STATUS, Projectile.DamageElement.STATUS);
				}
				healthEffectTimer += Time.deltaTime;
			}
			break;
		case HealthManager.StatusEffect.CONFUSED:
			myBotManager.burnedP.Stop ();
			myBotManager.frozenP.Stop ();
			myBotManager.shockedP.Stop ();
			myBotManager.slowedP.Stop ();
			myBotManager.confusedP.Play ();

			if (healthEffectTimer > 3f) {
				myHPManager.statusEffect = HealthManager.StatusEffect.NORMAL;
				myBotManager.movementSpeed = initSpeed;
				myBotManager.acceleration = initAcceleration;
				myBotManager.timeBetweenAttacksInSeconds = initFireRate;
				healthEffectTimer = 0f;
			}
			else {
				myBotManager.movementSpeed = 0f;
				myBotManager.acceleration = 0f;
				myBotManager.timeBetweenAttacksInSeconds = 999f;
				healthEffectTimer += Time.deltaTime;
			}

			break;
		}

		prevStatus = myHPManager.statusEffect;
	}
}
