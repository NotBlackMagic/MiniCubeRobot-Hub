using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class RobotCOMInterface : MonoBehaviour {
	//Serial Port Variables
	public string comInterface = "COM3";
	public int baudrate = 115200;
	bool rxUSBThreadRun = true;
	Thread rxUSBThread;
	SerialPort serialPort = new SerialPort();

	//File Write Variables
	public bool logDriveDebug = false;
	System.IO.StreamWriter streamWriter;

	//Received Info
	public RobotMsgs.RobotBattery robotBattery;
	public RobotMsgs.RobotOdom robotOdom;
	public RobotMsgs.RobotRange robotRange;
	public RobotMsgs.RobotContact robotContact;
	public RobotMsgs.RobotDrive robotDrive;
	public RobotMsgs.RobotLaserScan robotLaserScan = new RobotMsgs.RobotLaserScan(8, 64);
	public RobotMsgs.RobotDbgDrive robotDbgDrive;

	//Update Call
	public float refreshRate = 1.0f;
	private float timestamp = 0;

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
		bool connected = Connect(comInterface, baudrate);

		if(connected) {
			Debug.Log("COM Connected");
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
		if((timestamp + refreshRate) < Time.time) {
			//Debug.Log("Battery Info: " + robotBattery.percentage.ToString() + "% @ " + robotBattery.voltage.ToString() + "V; Consumption: " + robotBattery.current.ToString() + "mA");

			robotEnergyUI.SetBatteryLevel(robotBattery.percentage);
			robotEnergyUI.SetBatteryVoltage(robotBattery.voltage);
			robotEnergyUI.SetBatteryCurrent(robotBattery.current);
			robotEnergyUI.SetBatteryStatus(robotBattery.status);

			if(robotOdom.posePoint != null) {
				//Debug.Log("Odom Info: X: " + robotOdom.posePoint.x + "m @ " + robotOdom.velocityLinear.x + "m/s | Y: " + robotOdom.posePoint.y + "m @ " + robotOdom.velocityLinear.y + "m/s | rZ: "
				//						+ robotOdom.poseOrientation.z + "rad @ " + robotOdom.velocityAngular.z + "rad/s");

				//miniCubeRobotTransform.position = new Vector3(robotOdom.posePoint.x, robotOdom.posePoint.z, robotOdom.posePoint.y);
				//miniCubeRobotTransform.eulerAngles = new Vector3(robotOdom.poseOrientation.x * Mathf.Rad2Deg, robotOdom.poseOrientation.z * Mathf.Rad2Deg, robotOdom.poseOrientation.y * Mathf.Rad2Deg);
			}
			
			//Update Collision Sensors Lines
			for (int i = 0; i < collisionSensors.Length; i++) {
				if (robotRange.ranges != null && robotRange.ranges.Length > i) {
					LineRenderer lineRendererUpdate = collisionSensors[i].GetComponent<LineRenderer>();

					//Set start and end position
					lineRendererUpdate.SetPosition(0, Vector3.zero);
					Vector3 lineEnd = Vector3.zero + Vector3.forward * robotRange.ranges[i];
					lineRendererUpdate.SetPosition(1, lineEnd);

					//Set start and end width, shape as triangle
					lineRendererUpdate.startWidth = 0.0f;
					lineRendererUpdate.endWidth = robotRange.ranges[i] * Mathf.Tan(collisionSensorAngle * Mathf.Deg2Rad) * 2.0f;
				}
			}

			if(robotLaserScan.ranges != null) {
				robotLaserScanner.UpdateLaserScan(robotLaserScan.ranges);
			}

			timestamp = Time.time;
		}

		//Turn Wheel
		float wheelDPSLeft = (robotDrive.wheelRPMLeft / 89.0f) * 360.0f * Time.deltaTime;
		float wheelDPSRight = (robotDrive.wheelRPMRight / 89.0f) * 360.0f * Time.deltaTime;
		robotWheelLeft.transform.Rotate(Vector3.right, wheelDPSLeft);
		robotWheelRight.transform.Rotate(Vector3.right, wheelDPSRight);
	}

	void OnDestroy() {
		Disconnect();
	}

	public class DataPacket {
		public int srcID;
		public int msgsID;
		public byte[] payload;
		public int payloadLength;
		public int crc;

		public byte[] Encode() {
			payloadLength = payload.Length;

			byte[] bytes = new byte[payloadLength + 6];

			//Packet Structure:
			//[ u8 ][  u8   ][  u8  ][u8*LENGTH][u16]
			//[SRC ][COMMAND][LENGTH][ PAYLOAD ][CRC]
			int index = 0;
			bytes[index++] = (byte)srcID;
			bytes[index++] = (byte)msgsID;
			bytes[index++] = (byte)(payloadLength >> 8);
			bytes[index++] = (byte)(payloadLength);
			Array.Copy(payload, 0, bytes, index, payloadLength);
			index += payloadLength;

			int crc = 0;
			bytes[index++] = (byte)(crc >> 8);   //CRC-16
			bytes[index++] = (byte)(crc);        //CRC-16

			return bytes;
		}

		public void Decode(byte[] packet) {
			//Packet Structure:
			//[ u8 ][  u8   ][  u8  ][u8*LENGTH][u16]
			//[SRC ][COMMAND][LENGTH][ PAYLOAD ][CRC]
			int index = 0;
			srcID = packet[index++];
			msgsID = packet[index++];
			payloadLength = (packet[index++] << 8);
			payloadLength += packet[index++];
			payload = new byte[payloadLength];
			Array.Copy(packet, 4, payload, 0, payloadLength);
			index += payloadLength;
			crc = (packet[index++] << 8);
			crc += packet[index++];
		}
	}

	public bool Connect(string port, int baudrate) {
		if (serialPort.IsOpen == false) {
			try {
				serialPort.PortName = port;
				serialPort.BaudRate = baudrate;
				serialPort.DataBits = 8;
				serialPort.StopBits = StopBits.One;
				serialPort.Parity = Parity.None;
				serialPort.NewLine = "\n";
				serialPort.Open();

				//USBWrite(Opcodes.connect, null);

				//usbRXStopwatch.Start();

				rxUSBThreadRun = true;
				rxUSBThread = new Thread(USBRXThread);
				rxUSBThread.Start();
			}
			catch (System.Exception ex) {
				return false;
			}
		}
		else {
			return false;
		}
		return true;
	}

	public bool Disconnect() {
		if (serialPort.IsOpen == true) {
			//USBWrite(Opcodes.disconnect, null);

			rxUSBThreadRun = false;

			//This delay gives time for the USBWrite to complete the write before destroying/closing the connection
			//Thread.Sleep(100);

			serialPort.Close();

			//Close file
			streamWriter.Close();
		}
		else {
			return false;
		}
		return true;
	}

	public bool IsConnected() {
		return serialPort.IsOpen;
	}

	private void USBRXThread() {
		DataPacket rxPacket = new DataPacket();

		int index = 0;
		int rxDataPacketLength = 0;
		byte[] rxDataArray = new byte[2048];

		byte[] rxBuffer = new byte[2048];
		while (rxUSBThreadRun) {
			if (serialPort.IsOpen) {
				int rxLength = 0;
				try {
					//rxLength = await serialPort.BaseStream.ReadAsync(rxBuffer, 0, rxBuffer.Length);
					rxLength = serialPort.BaseStream.Read(rxBuffer, 0, rxBuffer.Length);
				}
				catch {
					System.Diagnostics.Debug.WriteLine("ERROR: serialPort.BaseStream.Read()");
					return;
				}

				for (int i = 0; i < rxLength; i++) {
					rxDataArray[index] = rxBuffer[i];

					if (index == 2) {
						//This is the MSB packet length field, byte 2
						rxDataPacketLength = (rxBuffer[i] << 8);

						index += 1;
					}
					else if(index == 3) {
						//This is the LSB packet length field, byte 3
						rxDataPacketLength += rxBuffer[i];

						if (rxDataPacketLength > rxDataArray.Length) {
							//RX Buffer overflow
							index = 0;
							//usbRXErrorCount += 1;
							System.Diagnostics.Debug.WriteLine("ERROR: RX Buffer overflow");
							break;
						}
						else if (rxDataPacketLength == 0) {
							//Invalid payload length
							index = 0;
							System.Diagnostics.Debug.WriteLine("ERROR: Invalid Payload Length");
							break;
						}
						else {
							index += 1;
						}
					}
					else if (index == (rxDataPacketLength + 5)) {
						//Full packet received
						rxPacket.Decode(rxDataArray);

						if(rxPacket.crc != 0) {
							//Packet CRC failed
							index = 0;
							System.Diagnostics.Debug.WriteLine("ERROR: RX Packet CRC Error");
							break;
						}

						//Process Packet
						if (RobotMessageRXProcessing(rxPacket) != 0) {
							//Packet Processing Error
							index = 0;
							System.Diagnostics.Debug.WriteLine("ERROR: RX Packet Processing Error");
							break;
						}
						else {
							//Successful packet processing
							index = 0;
						}
					}
					else {
						index += 1;
					}
				}

				//usbRXByteCount += rxLength;
				//if (usbRXStopwatch.ElapsedMilliseconds > 1000) {
				//	usbRXDatarate = (float)usbRXByteCount;
				//	usbRXByteCount = 0;

				//	usbRXErrorRate = (float)usbRXErrorCount;
				//	usbRXErrorCount = 0;

				//	usbRXStopwatch.Restart();
				//}
			}
		}
	}

	public void RobotMessageSend(DataPacket packet) {
		if (serialPort.IsOpen) {
			byte[] data = packet.Encode();
			serialPort.Write(data, 0, data.Length);
		}
	}

	private int RobotMessageRXProcessing(DataPacket packet) {
		switch(packet.msgsID) {
			case (int)RobotMsgs.RobotInfoMsgs.battery:
				if(packet.payloadLength == 8) {
					robotBattery.Decode(packet.payload, 0);
				}
				break;
			case (int)RobotMsgs.RobotInfoMsgs.odom:
				if (packet.payloadLength == 48) {
					robotOdom.Decode(packet.payload, 0);
				}
				break;
			case (int)RobotMsgs.RobotInfoMsgs.range:
				if (packet.payloadLength == 13) {
					robotRange.Decode(packet.payload, 0);
				}
				break;
			case (int)RobotMsgs.RobotInfoMsgs.contact:
				if (packet.payloadLength == 4) {
					robotContact.Decode(packet.payload, 0);
				}
				break;
			case (int)RobotMsgs.RobotInfoMsgs.drive:
				if (packet.payloadLength == 13) {
					robotDrive.Decode(packet.payload, 0);
				}
				break;
			case (int)RobotMsgs.RobotDbgMsgs.drive:
				if (packet.payloadLength == 28) {
					robotDbgDrive.Decode(packet.payload, 0);

					if (logDriveDebug == true) {
						//Write to data file
						streamWriter.Write(robotMoveCtrl.twistCmd.velocityLinear.x.ToString() + ",");
						streamWriter.Write(robotDbgDrive.pwmLeft.ToString() + ",");
						streamWriter.Write(robotDbgDrive.pidPLeft.ToString() + ",");
						streamWriter.Write(robotDbgDrive.pidILeft.ToString() + ",");
						streamWriter.Write(robotDbgDrive.pidDLeft.ToString() + ",");
						streamWriter.Write(robotDbgDrive.pwmRight.ToString() + ",");
						streamWriter.Write(robotDbgDrive.pidPRight.ToString() + ",");
						streamWriter.Write(robotDbgDrive.pidIRight.ToString() + ",");
						streamWriter.Write(robotDbgDrive.pidDRight.ToString() + ",");
						streamWriter.Write(Environment.NewLine);
					}
				}
				break;
			case (int)RobotMsgs.RobotInfoMsgs.laserScan:
				if (packet.payloadLength == (8 * 64 * 2)) {
					robotLaserScan.Decode(packet.payload, 0);
				}
				else {
					Debug.Log("ERROR: Invalid Packet Length");
				}
				break;
			default:
				Debug.Log("ERROR: Invalid Message ID (Opcode)");
				return 1;
		}
		return 0;
	}
}
