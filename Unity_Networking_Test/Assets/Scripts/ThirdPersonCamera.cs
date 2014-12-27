using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour {

	//Fields
	public Transform cameraTarget;
	public float mouseXSensitivity, mouseYSensitivity;
	float joystickXSensitivity, joystickYSensitivity;
	Vector3 prevMousePosition, currentMousePosition;
	float initialFOV;


	// Use this for initialization
	void Start () {
		currentMousePosition = Input.mousePosition;
		prevMousePosition = currentMousePosition;
		joystickXSensitivity = mouseXSensitivity * 3f;
		joystickYSensitivity = mouseYSensitivity * 3f;
		initialFOV = camera.fieldOfView;
	}


	// Update is called once per frame
	void FixedUpdate () { //maybe the fixed update will fix the framerate/sensitivity issue
		if (!GetComponentInParent<NetworkView>().isMine) {
			//enabled = false;
			return;
		}
//		if (!GetComponentInParent<PlayerManager>().activated) {
//			return;
//		}
		currentMousePosition = Input.mousePosition;
		Vector3 mousePosDifference = currentMousePosition - prevMousePosition;



		float angle = mousePosDifference.x / 30f;
		angle = Input.GetAxis ("Mouse X") * mouseXSensitivity;

		//Debug: Print out the names of the controllers detected -----------------------------
		if (Application.platform == RuntimePlatform.OSXDashboardPlayer ||
		    Application.platform == RuntimePlatform.OSXEditor ||
		    Application.platform == RuntimePlatform.OSXPlayer ||
		    Application.platform == RuntimePlatform.OSXWebPlayer) {
			if (Input.GetAxis ("Mac_RightXAxis") != 0) {
				joystickXSensitivity = (camera.fieldOfView * mouseXSensitivity * 3f) / initialFOV;
				angle = Input.GetAxis ("Mac_RightXAxis") * joystickXSensitivity;
			}
		}
		else if (Application.platform == RuntimePlatform.WindowsEditor ||
		         Application.platform == RuntimePlatform.WindowsPlayer ||
		         Application.platform == RuntimePlatform.WindowsWebPlayer) {
			if (Input.GetAxis ("Windows_RightXAxis") != 0) {
				joystickXSensitivity = (camera.fieldOfView * mouseXSensitivity * 3f) / initialFOV;
				angle = Input.GetAxis ("Windows_RightXAxis") * joystickXSensitivity;
			}
		}
		//------------------------------------------------------------------------------------
		//Debug: implement controller for the camera movement-------------
//		if (Input.GetAxis ("Mac_RightXAxis") != 0) {
//			joystickXSensitivity = (camera.fieldOfView * mouseXSensitivity * 3f) / initialFOV;
//			angle = Input.GetAxis ("Mac_RightXAxis") * joystickXSensitivity;
//		}
		//----------------------------------------------------------------

		//Debug
//		if (angle > 0) {
//			print ("Rotate around X axis by " + angle);
//		}

		transform.RotateAround (cameraTarget.transform.position, new Vector3 (0, 1, 0), angle);

		int YDirection = 1;
		if (transform.localPosition.z > 0) {
			//YDirection = -1;
		}

		float yAxisAngle = Vector3.Angle ((cameraTarget.transform.position - transform.position), Vector3.up);
		angle = Input.GetAxis ("Mouse Y") * mouseYSensitivity;

		//Debug: Print out the names of the controllers detected -----------------------------
		if (Application.platform == RuntimePlatform.OSXDashboardPlayer ||
		    Application.platform == RuntimePlatform.OSXEditor ||
		    Application.platform == RuntimePlatform.OSXPlayer ||
		    Application.platform == RuntimePlatform.OSXWebPlayer) {
			if (Input.GetAxis ("Mac_RightYAxis") != 0) {
				joystickXSensitivity = (camera.fieldOfView * mouseXSensitivity * 3f) / initialFOV;
				angle = Input.GetAxis ("Mac_RightYAxis") * joystickYSensitivity;
			}
		}
		else if (Application.platform == RuntimePlatform.WindowsEditor ||
		         Application.platform == RuntimePlatform.WindowsPlayer ||
		         Application.platform == RuntimePlatform.WindowsWebPlayer) {
			if (Input.GetAxis ("Windows_RightYAxis") != 0) {
				joystickXSensitivity = (camera.fieldOfView * mouseXSensitivity * 3f) / initialFOV;
				angle = Input.GetAxis ("Windows_RightYAxis") * joystickYSensitivity;
			}
		}
		//Debug: implement controller for the camera movement-------------
//		if (Input.GetAxis ("Mac_RightYAxis") != 0) {
//			joystickXSensitivity = (camera.fieldOfView * mouseXSensitivity * 3f) / initialFOV;
//			angle = Input.GetAxis ("Mac_RightYAxis") * joystickYSensitivity;
//		}
		//----------------------------------------------------------------

		if (angle > 0) {
			if (yAxisAngle + angle <= 120) {
				transform.RotateAround (cameraTarget.transform.position, transform.TransformDirection (Vector3.right), angle*YDirection);
			}
		}
		else {
			if (yAxisAngle + angle >= 60) {
				transform.RotateAround (cameraTarget.transform.position, transform.TransformDirection (Vector3.right), angle*YDirection);
			}
		}
		//print ("YDirection: " + YDirection + "    transform.localposition.z: " + transform.localPosition.z + " angle: " + angle);

		//print ("X: " + currentMousePosition.x + "   Y: " + currentMousePosition.y);
		//print ("X axis: " + Input.GetAxis ("Mouse X"));
//		Quaternion tempQuat = transform.rotation;
//		tempQuat.z = 0;
//		transform.rotation = tempQuat;
		Vector3 tempAngles = transform.eulerAngles;
		tempAngles.z = 0;
//		if (tempAngles.x < 180 && tempAngles.x > 30) {
//			tempAngles.x = 60;
//		}
//		else if (tempAngles.x > 180 && tempAngles.x < 330) {
//			tempAngles.x = 330;
//		}
		transform.eulerAngles = tempAngles;
		//print ("rotation z: " + transform.rotation.z);
		//print ("Rotation Y angle: " + Vector3.Angle ((cameraTarget.transform.position - transform.position), Vector3.up));

		prevMousePosition = currentMousePosition;
	}
}
