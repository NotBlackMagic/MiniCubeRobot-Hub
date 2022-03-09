using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotMovementController : MonoBehaviour {
	//Robot Message Interaface
	public RobotCOMInterface robotCOMInterface;

	public float forwardSpeed = 0.01f;   //In m/s
	public float turnSpeed = 45.0f;		//In deg/s

	RobotCOMInterface.DataPacket twistCmdPkt = new RobotCOMInterface.DataPacket();
	public RobotMsgs.RobotTwist twistCmd = new RobotMsgs.RobotTwist();

	// Start is called before the first frame update
	void Start() {
		twistCmdPkt.srcID = 0;
		twistCmdPkt.msgsID = (int)RobotMsgs.RobotCmdMsgs.twist;

		twistCmd.velocityLinear = new Vector3(0.0f, 0.0f, 0.0f);
		twistCmd.velocityAngular = new Vector3(0.0f, 0.0f, 0.0f);
	}

	// Update is called once per frame
	void Update() {
		//Forward Command (Up Arrow)
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			twistCmd.velocityLinear = new Vector3(forwardSpeed, 0.0f, 0.0f);
			twistCmd.velocityAngular = new Vector3(0.0f, 0.0f, 0.0f);

			twistCmdPkt.payload = twistCmd.Encode();

			robotCOMInterface.RobotMessageSend(twistCmdPkt);
		}
		if (Input.GetKeyUp(KeyCode.UpArrow)) {
			twistCmd.velocityLinear = new Vector3(0.0f, 0.0f, 0.0f);
			twistCmd.velocityAngular = new Vector3(0.0f, 0.0f, 0.0f);

			twistCmdPkt.payload = twistCmd.Encode();

			robotCOMInterface.RobotMessageSend(twistCmdPkt);
		}

		//Backwards Command (Down Arrow)
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			twistCmd.velocityAngular = new Vector3(0.0f, 0.0f, 0.0f);
			twistCmd.velocityLinear = new Vector3(-forwardSpeed, 0.0f, 0.0f);

			twistCmdPkt.payload = twistCmd.Encode();

			robotCOMInterface.RobotMessageSend(twistCmdPkt);
		}
		if (Input.GetKeyUp(KeyCode.DownArrow)) {
			twistCmd.velocityLinear = new Vector3(0.0f, 0.0f, 0.0f);
			twistCmd.velocityAngular = new Vector3(0.0f, 0.0f, 0.0f);

			twistCmdPkt.payload = twistCmd.Encode();

			robotCOMInterface.RobotMessageSend(twistCmdPkt);
		}

		//Turn Left Command (Left Arrow)
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			twistCmd.velocityLinear = new Vector3(0.0f, 0.0f, 0.0f);
			twistCmd.velocityAngular = new Vector3(0.0f, 0.0f, (turnSpeed * Mathf.Deg2Rad));

			twistCmdPkt.payload = twistCmd.Encode();

			robotCOMInterface.RobotMessageSend(twistCmdPkt);
		}
		if (Input.GetKeyUp(KeyCode.LeftArrow)) {
			twistCmd.velocityLinear = new Vector3(0.0f, 0.0f, 0.0f);
			twistCmd.velocityAngular = new Vector3(0.0f, 0.0f, 0.0f);

			twistCmdPkt.payload = twistCmd.Encode();

			robotCOMInterface.RobotMessageSend(twistCmdPkt);
		}

		//Turn Right Command (Left Arrow)
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			twistCmd.velocityLinear = new Vector3(0.0f, 0.0f, 0.0f);
			twistCmd.velocityAngular = new Vector3(0.0f, 0.0f, -(turnSpeed * Mathf.Deg2Rad));

			twistCmdPkt.payload = twistCmd.Encode();

			robotCOMInterface.RobotMessageSend(twistCmdPkt);
		}
		if (Input.GetKeyUp(KeyCode.RightArrow)) {
			twistCmd.velocityLinear = new Vector3(0.0f, 0.0f, 0.0f);
			twistCmd.velocityAngular = new Vector3(0.0f, 0.0f, 0.0f);

			twistCmdPkt.payload = twistCmd.Encode();

			robotCOMInterface.RobotMessageSend(twistCmdPkt);
		}
	}
}
