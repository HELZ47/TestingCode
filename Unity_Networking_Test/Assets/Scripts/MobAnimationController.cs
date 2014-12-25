using UnityEngine;
using System.Collections;

public class MobAnimationController : MonoBehaviour {

	//Fields
	float speed;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (GetComponent<HealthManager>().isTakingDamage) {
			if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("ReceiveDamage")) {
				GetComponent<Animator> ().SetBool ("receiveDamage", false);
				GetComponent<HealthManager>().isTakingDamage = false;
			}
			else {
				GetComponent<Animator>().speed = 1;
				GetComponent<Animator> ().SetBool ("receiveDamage", true);
			}
		}

		if (GetComponent<HealthManager>().isDead) {
			GetComponent<Animator> ().SetBool ("isDead", true);
		}

		Animator animator = GetComponent<Animator> ();
		MobMovement mobMovement = GetComponent<MobMovement> ();
		speed = (rigidbody.velocity - new Vector3 (0, rigidbody.velocity.y, 0)).magnitude;
		if (speed > mobMovement.investigateSpeed) {
			animator.SetBool ("investigate", false);
			animator.SetBool ("chase", true);
		}
		else if (speed > 0.1f) {
			animator.SetBool ("investigate", true);
			animator.SetBool ("chase", false);
		}
		else {
			animator.SetBool ("investigate", false);
			animator.SetBool ("chase", false);
		}
	}
}
