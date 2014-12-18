using UnityEngine;
using System.Collections;

public class ChangeColor : MonoBehaviour {

//	public void Start () {
//		networkView.viewID = Network.AllocateViewID();
//	}

	[RPC]
	void ChangeColorToBlue() {
		if (Network.isServer) {

		}
		else if (Network.isClient) {

		}
		renderer.material.SetColor ("_Color", Color.blue);
	}

	[RPC]
	void ChangeColorToWhite() {
		if (Network.isServer) {
			
		}
		else if (Network.isClient) {

		}
		renderer.material.SetColor ("_Color", Color.white);
	}

	public void OnTriggerEnter (Collider col) {
		print("Trigger entered!");
		networkView.RPC ("ChangeColorToBlue", RPCMode.AllBuffered); 
	}

	public void OnTriggerExit (Collider col) {
		print("Trigger exited!");
		networkView.RPC ("ChangeColorToWhite", RPCMode.AllBuffered);
	}
}
