using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uRMSConnector;

public class RobotMovementController : MonoBehaviour {
	//Robot Message Interaface
	public RobotCOMInterface robotCOMInterface;

	public float forwardSpeed = 0.01f;   //In m/s
	public float turnSpeed = 45.0f;     //In deg/s

	uRMSMessage message = new uRMSMessage();
	uRMSTwist robotTwist = new uRMSTwist();

	uRMSConnection uRMSConnection;

	// Start is called before the first frame update
	void Start() {
		uRMSConnection = uRMSConnection.instance;

		robotTwist.header.frameID = 0;
		robotTwist.header.topicID = 4;

		robotTwist.velocityLinear.x = 0;
		robotTwist.velocityLinear.y = 0;
		robotTwist.velocityLinear.z = 0;
		robotTwist.velocityAngular.x = 0;
		robotTwist.velocityAngular.y = 0;
		robotTwist.velocityAngular.z = 0;
	}

	// Update is called once per frame
	void Update() {
		//Forward Command (Up Arrow)
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			robotTwist.velocityLinear.x = (int)(forwardSpeed * Mathf.Pow(2, 15));
			robotTwist.velocityLinear.y = 0;
			robotTwist.velocityLinear.z = 0;
			robotTwist.velocityAngular.x = 0;
			robotTwist.velocityAngular.y = 0;
			robotTwist.velocityAngular.z = 0;

			uRMSTwist.Serialize(robotTwist, ref message, 0);
			uRMSConnection.Publish(message);
		}
		if (Input.GetKeyUp(KeyCode.UpArrow)) {
			robotTwist.velocityLinear.x = 0;
			robotTwist.velocityLinear.y = 0;
			robotTwist.velocityLinear.z = 0;
			robotTwist.velocityAngular.x = 0;
			robotTwist.velocityAngular.y = 0;
			robotTwist.velocityAngular.z = 0;

			uRMSTwist.Serialize(robotTwist, ref message, 0);
			uRMSConnection.Publish(message);
		}

		//Backwards Command (Down Arrow)
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			robotTwist.velocityLinear.x = (int)(-forwardSpeed * Mathf.Pow(2, 15));
			robotTwist.velocityLinear.y = 0;
			robotTwist.velocityLinear.z = 0;
			robotTwist.velocityAngular.x = 0;
			robotTwist.velocityAngular.y = 0;
			robotTwist.velocityAngular.z = 0;

			uRMSTwist.Serialize(robotTwist, ref message, 0);
			uRMSConnection.Publish(message);
		}
		if (Input.GetKeyUp(KeyCode.DownArrow)) {
			robotTwist.velocityLinear.x = 0;
			robotTwist.velocityLinear.y = 0;
			robotTwist.velocityLinear.z = 0;
			robotTwist.velocityAngular.x = 0;
			robotTwist.velocityAngular.y = 0;
			robotTwist.velocityAngular.z = 0;

			uRMSTwist.Serialize(robotTwist, ref message, 0);
			uRMSConnection.Publish(message);
		}

		//Turn Left Command (Left Arrow)
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			robotTwist.velocityLinear.x = 0;
			robotTwist.velocityLinear.y = 0;
			robotTwist.velocityLinear.z = 0;
			robotTwist.velocityAngular.x = 0;
			robotTwist.velocityAngular.y = 0;
			robotTwist.velocityAngular.z = (int)(turnSpeed * Mathf.Deg2Rad * Mathf.Pow(2, 15));

			uRMSTwist.Serialize(robotTwist, ref message, 0);
			uRMSConnection.Publish(message);
		}
		if (Input.GetKeyUp(KeyCode.LeftArrow)) {
			robotTwist.velocityLinear.x = 0;
			robotTwist.velocityLinear.y = 0;
			robotTwist.velocityLinear.z = 0;
			robotTwist.velocityAngular.x = 0;
			robotTwist.velocityAngular.y = 0;
			robotTwist.velocityAngular.z = 0;

			uRMSTwist.Serialize(robotTwist, ref message, 0);
			uRMSConnection.Publish(message);
		}

		//Turn Right Command (Left Arrow)
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			robotTwist.velocityLinear.x = 0;
			robotTwist.velocityLinear.y = 0;
			robotTwist.velocityLinear.z = 0;
			robotTwist.velocityAngular.x = 0;
			robotTwist.velocityAngular.y = 0;
			robotTwist.velocityAngular.z = (int)(-turnSpeed * Mathf.Deg2Rad * Mathf.Pow(2, 15));

			uRMSTwist.Serialize(robotTwist, ref message, 0);
			uRMSConnection.Publish(message);
		}
		if (Input.GetKeyUp(KeyCode.RightArrow)) {
			robotTwist.velocityLinear.x = 0;
			robotTwist.velocityLinear.y = 0;
			robotTwist.velocityLinear.z = 0;
			robotTwist.velocityAngular.x = 0;
			robotTwist.velocityAngular.y = 0;
			robotTwist.velocityAngular.z = 0;

			uRMSTwist.Serialize(robotTwist, ref message, 0);
			uRMSConnection.Publish(message);
		}
	}
}
