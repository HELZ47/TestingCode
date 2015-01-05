using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//Manages the player's UI elements
public class PlayerUIManager : MonoBehaviour {

	#region Fields
	//Adjustable
	public Slider energySlider;
	//Not Adjustable
	EnergyManager myEnergyManager;
	#endregion

	// Use this for initialization
	void Awake () {
		myEnergyManager = GetComponent<EnergyManager> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (networkView.isMine) {
			energySlider.value = myEnergyManager.energyRatio;
		}
	}
}
