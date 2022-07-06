using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using uRMSConnector;

public class RobotCOMInterface : MonoBehaviour {
	//Robot Communication Interface
	uRMSConnection uRMSConnection;

	//File Write Variables
	public bool logDriveDebug = false;
	System.IO.StreamWriter streamWriter;

	//Received Info
	public uRMSBattery robotBattery = new uRMSBattery();
	public uRMSOdometry robotOdom = new uRMSOdometry();
	public uRMSRange robotRange = new uRMSRange();
	public uRMSContact robotContact = new uRMSContact();
	public uRMSDrive robotDrive = new uRMSDrive();
	public uRMSLaserScan robotLaserScan = new uRMSLaserScan();

	bool newBatteryData = false;
	bool newOdomData = false;
	bool newRangeData = false;
	bool newLaserScanData = false;

	//Mini Cube Robot Transform
	Transform miniCubeRobotTransform;
	public GameObject robotWheelLeft;
	public GameObject robotWheelRight;
	public float collisionSensorAngle;
	public GameObject[] collisionSensors;
	public RobotMovementController robotMoveCtrl;
	public RobotLaserScan robotLaserScanner;

	//UI Holders
	public RobotEnergyUI robotEnergyUI;

	// Start is called before the first frame update
	void Start() {
		uRMSConnection = uRMSConnection.instance;

		if (uRMSConnection.IsConnected()) {
			Debug.Log("COM Connected");

			uRMSConnection.Subscribe<uRMSMessage>(0, OnBatteryReceive);
			uRMSConnection.Subscribe<uRMSMessage>(1, OnOdometryReceive);
			uRMSConnection.Subscribe<uRMSMessage>(2, OnRangeReceive);
			uRMSConnection.Subscribe<uRMSMessage>(3, OnContactReceive);
			uRMSConnection.Subscribe<uRMSMessage>(5, OnLaserScanReceive);
			uRMSConnection.Subscribe<uRMSMessage>(6, OnDriveReceive);
		}
		else {
			Debug.Log("COM Connection Failed");
		}

		//Create .csv info file (if existing overwrite)
		streamWriter = new System.IO.StreamWriter(@".\data.csv");

		miniCubeRobotTransform = this.transform.parent;
	}

	// Update is called once per frame
	void Update() {
		if(newBatteryData == true) {
			//Debug.Log("Battery Info: " + robotBattery.percentage.ToString() + "% @ " + robotBattery.voltage.ToString() + "V; Consumption: " + robotBattery.current.ToString() + "mA");

			robotEnergyUI.SetBatteryLevel(robotBattery.percentage);
			robotEnergyUI.SetBatteryVoltage(robotBattery.voltage * 0.001f);
			robotEnergyUI.SetBatteryCurrent(robotBattery.current);
			robotEnergyUI.SetBatteryStatus(robotBattery.status);

			newBatteryData = false;
		}

		if(newOdomData == true) {
			//Debug.Log("Odom Info: X: " + robotOdom.posePoint.x + "m @ " + robotOdom.velocityLinear.x + "m/s | Y: " + robotOdom.posePoint.y + "m @ " + robotOdom.velocityLinear.y + "m/s | rZ: "
			//						+ robotOdom.poseOrientation.z + "rad @ " + robotOdom.velocityAngular.z + "rad/s");

			miniCubeRobotTransform.position = new Vector3(robotOdom.posePoint.x * (1.0f / Mathf.Pow(2, 15)),
															robotOdom.posePoint.z * (1.0f / Mathf.Pow(2, 15)),
															robotOdom.posePoint.y * (1.0f / Mathf.Pow(2, 15)));
			miniCubeRobotTransform.eulerAngles = new Vector3(robotOdom.poseOrientation.x * (1.0f / Mathf.Pow(2, 29)) * Mathf.Rad2Deg,
																robotOdom.poseOrientation.z * (1.0f / Mathf.Pow(2, 29)) * Mathf.Rad2Deg,
																robotOdom.poseOrientation.y * (1.0f / Mathf.Pow(2, 29)) * Mathf.Rad2Deg);

			newOdomData = false;
		}

		if(newRangeData == true) {
			//Update Collision Sensors Lines
			for (int i = 0; i < collisionSensors.Length; i++) {
				if (robotRange.ranges != null && robotRange.ranges.Length > i) {
					LineRenderer lineRendererUpdate = collisionSensors[i].GetComponent<LineRenderer>();

					//Set start and end position
					lineRendererUpdate.SetPosition(0, Vector3.zero);
					Vector3 lineEnd = Vector3.zero + Vector3.forward * robotRange.ranges[i] * 0.001f;
					lineRendererUpdate.SetPosition(1, lineEnd);

					//Set start and end width, shape as triangle
					lineRendererUpdate.startWidth = 0.0f;
					lineRendererUpdate.endWidth = robotRange.ranges[i] * 0.001f * Mathf.Tan(collisionSensorAngle * Mathf.Deg2Rad) * 2.0f;
				}
			}

			newRangeData = false;
		}

		if(newLaserScanData == true) {
			float[][] laserRanges = new float[robotLaserScan.countV][];
			for (int v = 0; v < robotLaserScan.countV; v++) {
				laserRanges[v] = new float[robotLaserScan.countH];
				for (int h = 0; h < robotLaserScan.countH; h++) {
					laserRanges[v][h] = robotLaserScan.ranges[v][h] * 0.001f;       //Convert from mm to m
				}
			}
			robotLaserScanner.UpdateLaserScan(laserRanges);

			newLaserScanData = false;
		}

		//Turn Wheel
		float wheelDPSLeft = (robotDrive.wheelRPMLeft / 89.0f) * 360.0f * Time.deltaTime;
		float wheelDPSRight = (robotDrive.wheelRPMRight / 89.0f) * 360.0f * Time.deltaTime;
		robotWheelLeft.transform.Rotate(Vector3.right, wheelDPSLeft);
		robotWheelRight.transform.Rotate(Vector3.right, wheelDPSRight);
	}

	void OnBatteryReceive(uRMSMessage message) {
		uRMSBattery.Deserialize(message, ref robotBattery, 0);
		newBatteryData = true;
	}

	void OnOdometryReceive(uRMSMessage message) {
		uRMSOdometry.Deserialize(message, ref robotOdom, 0);
		newOdomData = true;
	}

	void OnRangeReceive(uRMSMessage message) {
		uRMSRange.Deserialize(message, ref robotRange, 0);
		newRangeData = true;
	}

	void OnContactReceive(uRMSMessage message) {
		uRMSContact.Deserialize(message, ref robotContact, 0);
	}

	void OnDriveReceive(uRMSMessage message) {
		uRMSDrive.Deserialize(message, ref robotDrive, 0);
	}

	void OnLaserScanReceive(uRMSMessage message) {
		uRMSLaserScan.Deserialize(message, ref robotLaserScan, 0);
		newLaserScanData = true;
	}
}
