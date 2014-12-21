using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	//Fields
	public ParticleSystem projectParticles, hitParticles;
	public enum ProjectileType { BULLET, GRENADE, ORB }
	public ProjectileType projectileType;
	public float bulletSpeed, grenadeSpeed, orbSpeed;
	public float bulletGSpeed, grenadeGSpeed, orbGSpeed;
	Vector3 direction;
	float speed, gSpeed;
	bool isHit;

	//Move the projectile
	public void MoveProjectile (Vector3 givenDirection) {
		direction = givenDirection;
		direction = direction.normalized * speed;
	}

	void Awake () {
		projectParticles.Play ();
		hitParticles.Stop ();
		isHit = false;
		
		switch (projectileType) {
		case ProjectileType.BULLET:
			speed = bulletSpeed;
			gSpeed = bulletGSpeed;
			break;
		case ProjectileType.GRENADE:
			speed = grenadeSpeed;
			gSpeed = grenadeGSpeed;
			break;
		case ProjectileType.ORB:
			speed = orbSpeed;
			gSpeed = orbGSpeed;
			break;
		}
	}

	// Use this for initialization
	void Start () {
//		projectParticles.Play ();
//		hitParticles.Stop ();
//		isHit = false;
//
//		switch (projectileType) {
//		case ProjectileType.BULLET:
//			speed = bulletSpeed;
//			gSpeed = bulletGSpeed;
//			break;
//		case ProjectileType.GRENADE:
//			speed = grenadeSpeed;
//			gSpeed = grenadeGSpeed;
//			break;
//		case ProjectileType.ORB:
//			speed = orbSpeed;
//			gSpeed = orbGSpeed;
//			break;
//		}
	}


	// Update is called once per frame
	void Update () {
//		transform.Translate (direction.normalized * Time.deltaTime);
		if (!isHit) {
			transform.Translate (direction * Time.deltaTime);
			direction.y -= gSpeed * Time.deltaTime;
			//transform.Translate (new Vector3 (0, -1f, 0) * gSpeed * Time.deltaTime);
//			transform.Translate (direction.normalized);
//			transform.Translate (direction.normalized * speed * Time.deltaTime);
//			transform.Translate (new Vector3 (0, -1f, 0) * gSpeed * Time.deltaTime);
//			gSpeed *= 1.05f;
		}
		if (Physics.OverlapSphere (transform.position, GetComponent<SphereCollider> ().radius).Length > 1 && !isHit) {
			//Destroy ();
			isHit = true;
			projectParticles.Stop ();
			hitParticles.Play ();
			//hitParticles.Emit (1);
			GetComponent<SphereCollider>().isTrigger = true;
		}
		if (isHit == true && projectParticles.isStopped) {
			Destroy();
		}
	}

	[RPC]
	void Destroy () {
		Network.Destroy (this.gameObject);
	}

//	void OnCollisionEnter (Collision col) {
//		print ("Collided!!!");
//		//Network.Destroy (this.gameObject);
//		networkView.RPC ("Destroy", RPCMode.AllBuffered);
//	}
}
