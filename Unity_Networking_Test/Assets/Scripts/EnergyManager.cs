using UnityEngine;
using System.Collections;

//Manage and handles the energy of a player
public class EnergyManager : MonoBehaviour {

	#region Fields
	//Adjustable
	public float energyAmount, maxEnergyRefillRate, minEnergyRefillRate;
	//Not Adjustable
	float fullEnergy;
	#endregion


	#region Initialization
	// Initializa internal variables
	void Awake () {
		fullEnergy = energyAmount;
	}
	#endregion


	#region Functions
	public bool SpendEnergy (float e) {
		if (energyAmount >= e) {
			energyAmount -= e;
			return true;
		}
		else {
			return false;
		}
	}
	#endregion

	
	// Update is called once per frame
	void Update () {
		if (!networkView.isMine) {
			return;
		}
		print ("Energy: " + energyAmount);
		//Get the current energy refill rate (per second) based on the current amount of energy
		//More energy you have, the faster it refills
		float currentERefillRate = minEnergyRefillRate + ((maxEnergyRefillRate-minEnergyRefillRate) * (energyAmount/fullEnergy));
		print ("Current refill Rate: " + currentERefillRate);

		//Refill the energy by the current refill rate
		energyAmount += currentERefillRate * Time.deltaTime;
		if (energyAmount > fullEnergy) {
			energyAmount = fullEnergy;
		}
	}
}
