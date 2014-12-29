using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnMob : MonoBehaviour {

	//Fields
	public enum SpawnType { ZOMBIE, ROBOT, ROBOT_RANGED, TOWER, TEST };
	public enum SpawnNumber { SINGLE, AREA };
	public enum TeamNumber { ONE, TWO, OTHER };
	public enum RobotType { ASSAULT, BODYGUARD };
	public SpawnType spawnType;
	public SpawnNumber spawnNumber;
	public TeamNumber teamNumber;
	public RobotType robotType;
	public bool spawnOnce;
	public float spawnInterval;
	public int numOfSpawns;
	public Waypoints path;
	public Vector2 worldSize;
	public List<Vector3> spawnPositions;
	float spawnTimer;

	// Use this for initialization
	void Start () {
		//if (spawnOnce) {
			spawnPositions = new List<Vector3> ();
			if (Network.isServer) {
				GameObject robot = null;
				switch (spawnNumber) {
				case SpawnNumber.SINGLE:
					switch (spawnType) {
					case SpawnType.ROBOT:
						if (teamNumber == TeamNumber.ONE) {
							robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/Robot_T1"), transform.position, new Quaternion(), 0) as GameObject;
						}
						else if (teamNumber == TeamNumber.TWO) {
							robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/Robot_T2"), transform.position, new Quaternion(), 0) as GameObject;
						}
						if (robotType == RobotType.ASSAULT) {
							robot.GetComponent<BotManager>().botType = BotManager.BotType.PATROL;
							robot.GetComponent<BotManager>().givenPath = path;
						}
						break;
					case SpawnType.ZOMBIE:
						Network.Instantiate (Resources.Load ("Prefabs/Zombie"), transform.position, new Quaternion(), 0);
						break;
					case SpawnType.TOWER:
						if (teamNumber == TeamNumber.ONE) {
							Network.Instantiate (Resources.Load ("Prefabs/Towers/Tower_T1"), transform.position, new Quaternion(), 0);
						}
						else if (teamNumber == TeamNumber.TWO) {
							Network.Instantiate (Resources.Load ("Prefabs/Towers/Tower_T2"), transform.position, new Quaternion(), 0);
						}
						break;
					case SpawnType.ROBOT_RANGED:
						if (teamNumber == TeamNumber.ONE) {
							robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/RangedRobot_T1"), transform.position, new Quaternion(), 0) as GameObject;
						}
						else if (teamNumber == TeamNumber.TWO) {
							robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/RangedRobot_T2"), transform.position, new Quaternion(), 0) as GameObject;
						}
						if (robotType == RobotType.ASSAULT) {
							robot.GetComponent<BotManager>().botType = BotManager.BotType.PATROL;
							robot.GetComponent<BotManager>().givenPath = path;
						}
						break;
					case SpawnType.TEST:
						Network.Instantiate (Resources.Load ("Prefabs/Dummy"), transform.position, new Quaternion(), 0);
						break;
					}
					break;
				case SpawnNumber.AREA:
					for (int i = 0; i < numOfSpawns; i++) {
						bool tooClose = false;
						Vector3 spawnPosition = Vector3.zero;
						do {
							spawnPosition = new Vector3 (Random.Range((-worldSize.x/2f) + transform.position.x, (worldSize.x/2f) + transform.position.x), 0, Random.Range((-worldSize.y/2f) + transform.position.z, (worldSize.y/2f) + transform.position.z));
							tooClose = false;
							foreach (Vector3 pos in spawnPositions) {
								if (Vector3.Distance (spawnPosition, pos) < 2) {
									tooClose = true;
								}
							}
							foreach (Collider col in Physics.OverlapSphere (spawnPosition, 2f)) {
								if ((col.gameObject.GetComponent<PlayerManager>() != null ||
								     col.gameObject.GetComponent<HealthManager>() != null) &&
								    col.tag != "Ground" && col.tag != "Environment") {
									tooClose = true;
								}
							}
						} while (tooClose);

						spawnPositions.Add (spawnPosition);

						switch (spawnType) {
						case SpawnType.ROBOT:
							if (teamNumber == TeamNumber.ONE) {
								robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/Robot_T1"), spawnPosition, new Quaternion(), 0) as GameObject;
							}
							else if (teamNumber == TeamNumber.TWO) {
								robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/Robot_T2"), spawnPosition, new Quaternion(), 0) as GameObject;
							}
							if (robotType == RobotType.ASSAULT) {
								robot.GetComponent<BotManager>().botType = BotManager.BotType.PATROL;
								robot.GetComponent<BotManager>().givenPath = path;
							}
							break;
						case SpawnType.ZOMBIE:
							Network.Instantiate (Resources.Load ("Prefabs/Zombie"), spawnPosition, new Quaternion(), 0);
							break;
						case SpawnType.TOWER:
								if (teamNumber == TeamNumber.ONE) {
									Network.Instantiate (Resources.Load ("Prefabs/Towers/Tower_T1"), spawnPosition, new Quaternion(), 0);
								}
								else if (teamNumber == TeamNumber.TWO) {
									Network.Instantiate (Resources.Load ("Prefabs/Towers/Tower_T2"), spawnPosition, new Quaternion(), 0);
								}
							break;
						case SpawnType.ROBOT_RANGED:
							if (teamNumber == TeamNumber.ONE) {
								robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/RangedRobot_T1"), spawnPosition, new Quaternion(), 0) as GameObject;
							}
							else if (teamNumber == TeamNumber.TWO) {
								robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/RangedRobot_T2"), spawnPosition, new Quaternion(), 0) as GameObject;
							}
							if (robotType == RobotType.ASSAULT) {
								robot.GetComponent<BotManager>().botType = BotManager.BotType.PATROL;
								robot.GetComponent<BotManager>().givenPath = path;
							}
							break;
						case SpawnType.TEST:
							Network.Instantiate (Resources.Load ("Prefabs/Dummy"), spawnPosition, new Quaternion(), 0);
							break;
						}
						
					}
					
					break;
				}
			}
		//}
	}

	void SpawnObjects () {
		spawnPositions = new List<Vector3> ();
		if (Network.isServer) {
			GameObject robot = null;
			switch (spawnNumber) {
			case SpawnNumber.SINGLE:
				switch (spawnType) {
				case SpawnType.ROBOT:
					if (teamNumber == TeamNumber.ONE) {
						robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/Robot_T1"), transform.position, new Quaternion(), 0) as GameObject;
					}
					else if (teamNumber == TeamNumber.TWO) {
						robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/Robot_T2"), transform.position, new Quaternion(), 0) as GameObject;
					}
					if (robotType == RobotType.ASSAULT) {
						robot.GetComponent<BotManager>().botType = BotManager.BotType.PATROL;
						robot.GetComponent<BotManager>().givenPath = path;
					}
					break;
				case SpawnType.ZOMBIE:
					Network.Instantiate (Resources.Load ("Prefabs/Zombie"), transform.position, new Quaternion(), 0);
					break;
				case SpawnType.TOWER:
					if (teamNumber == TeamNumber.ONE) {
						Network.Instantiate (Resources.Load ("Prefabs/Towers/Tower_T1"), transform.position, new Quaternion(), 0);
					}
					else if (teamNumber == TeamNumber.TWO) {
						Network.Instantiate (Resources.Load ("Prefabs/Towers/Tower_T2"), transform.position, new Quaternion(), 0);
					}
					break;
				case SpawnType.ROBOT_RANGED:
					if (teamNumber == TeamNumber.ONE) {
						robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/RangedRobot_T1"), transform.position, new Quaternion(), 0) as GameObject;
					}
					else if (teamNumber == TeamNumber.TWO) {
						robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/RangedRobot_T2"), transform.position, new Quaternion(), 0) as GameObject;
					}
					if (robotType == RobotType.ASSAULT) {
						robot.GetComponent<BotManager>().botType = BotManager.BotType.PATROL;
						robot.GetComponent<BotManager>().givenPath = path;
					}
					break;
				case SpawnType.TEST:
					Network.Instantiate (Resources.Load ("Prefabs/Dummy"), transform.position, new Quaternion(), 0);
					break;
				}
				break;
			case SpawnNumber.AREA:
				for (int i = 0; i < numOfSpawns; i++) {
					bool tooClose = false;
					Vector3 spawnPosition = Vector3.zero;
					do {
						spawnPosition = new Vector3 (Random.Range((-worldSize.x/2f) + transform.position.x, (worldSize.x/2f) + transform.position.x), 0, Random.Range((-worldSize.y/2f) + transform.position.z, (worldSize.y/2f) + transform.position.z));
						tooClose = false;
						foreach (Vector3 pos in spawnPositions) {
							if (Vector3.Distance (spawnPosition, pos) < 2) {
								tooClose = true;
							}
						}
						foreach (Collider col in Physics.OverlapSphere (spawnPosition, 2f)) {
							if ((col.gameObject.GetComponent<PlayerManager>() != null ||
							     col.gameObject.GetComponent<HealthManager>() != null) &&
							    col.tag != "Ground" && col.tag != "Environment") {
								tooClose = true;
							}
						}
					} while (tooClose);
					
					spawnPositions.Add (spawnPosition);
					
					switch (spawnType) {
					case SpawnType.ROBOT:
						if (teamNumber == TeamNumber.ONE) {
							robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/Robot_T1"), spawnPosition, new Quaternion(), 0) as GameObject;
						}
						else if (teamNumber == TeamNumber.TWO) {
							robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/Robot_T2"), spawnPosition, new Quaternion(), 0) as GameObject;
						}
						if (robotType == RobotType.ASSAULT) {
							robot.GetComponent<BotManager>().botType = BotManager.BotType.PATROL;
							robot.GetComponent<BotManager>().givenPath = path;
						}
						break;
					case SpawnType.ZOMBIE:
						Network.Instantiate (Resources.Load ("Prefabs/Zombie"), spawnPosition, new Quaternion(), 0);
						break;
					case SpawnType.TOWER:
						if (teamNumber == TeamNumber.ONE) {
							Network.Instantiate (Resources.Load ("Prefabs/Towers/Tower_T1"), spawnPosition, new Quaternion(), 0);
						}
						else if (teamNumber == TeamNumber.TWO) {
							Network.Instantiate (Resources.Load ("Prefabs/Towers/Tower_T2"), spawnPosition, new Quaternion(), 0);
						}
						break;
					case SpawnType.ROBOT_RANGED:
						if (teamNumber == TeamNumber.ONE) {
							robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/RangedRobot_T1"), spawnPosition, new Quaternion(), 0) as GameObject;
						}
						else if (teamNumber == TeamNumber.TWO) {
							robot = Network.Instantiate (Resources.Load ("Prefabs/Robots/RangedRobot_T2"), spawnPosition, new Quaternion(), 0) as GameObject;
						}
						if (robotType == RobotType.ASSAULT) {
							robot.GetComponent<BotManager>().botType = BotManager.BotType.PATROL;
							robot.GetComponent<BotManager>().givenPath = path;
						}
						break;
					case SpawnType.TEST:
						Network.Instantiate (Resources.Load ("Prefabs/Dummy"), spawnPosition, new Quaternion(), 0);
						break;
					}
					
				}
				
				break;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Network.isServer && !spawnOnce) {
			if (spawnTimer > spawnInterval) {
				spawnTimer = 0f;
				SpawnObjects ();
			}
			else {
				spawnTimer += Time.deltaTime;
			}
		}
	}

	void OnDrawGizmos () {

		switch (spawnType) {
		case SpawnType.ROBOT:
			Gizmos.color = Color.green;
			break;
		case SpawnType.ROBOT_RANGED:
			Gizmos.color = Color.yellow;
			break;
		case SpawnType.ZOMBIE:
			Gizmos.color = Color.red;
			break;
		case SpawnType.TOWER:
			Gizmos.color = Color.blue;
			break;
		}
		if (spawnNumber == SpawnNumber.AREA) {
			Gizmos.DrawCube (transform.position, new Vector3 (worldSize.x, 0.2f, worldSize.y));
		}
		else if (spawnNumber == SpawnNumber.SINGLE) {
			Gizmos.DrawSphere (transform.position, 1f);
		}
	}
}
