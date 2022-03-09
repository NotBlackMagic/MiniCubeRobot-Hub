using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotMsgs {
	public enum RobotCmdMsgs {
		reboot = 0x00,
		abort = 0x01,
		twist = 0x02,
		transform = 0x03
	}

	public enum RobotInfoMsgs {
		battery = 0x00,
		odom = 0x01,
		range = 0x02,
		contact = 0x03,
		drive = 0x04,
		time = 0x05,
		laserScan = 0x06
	}

	public enum RobotDbgMsgs {
		drive = 0x44
	}

	public struct Vector3_q31 {
		public int x;
		public int y;
		public int z;

		public byte[] Encode() {
			int index = 0;
			byte[] bytes = new byte[12];

			bytes[index++] = (byte)(x >> 24);
			bytes[index++] = (byte)(x >> 16);
			bytes[index++] = (byte)(x >> 8);
			bytes[index++] = (byte)(x);
			bytes[index++] = (byte)(y >> 24);
			bytes[index++] = (byte)(y >> 16);
			bytes[index++] = (byte)(y >> 8);
			bytes[index++] = (byte)(y);
			bytes[index++] = (byte)(z >> 24);
			bytes[index++] = (byte)(z >> 16);
			bytes[index++] = (byte)(z >> 8);
			bytes[index++] = (byte)(z);

			return bytes;
		}

		public int Decode(byte[] data, int offset) {
			int index = 0;
			x = (data[offset + index++] << 24);
			x += (data[offset + index++] << 16);
			x += (data[offset + index++] << 8);
			x += (data[offset + index++]);
			y = (data[offset + index++] << 24);
			y += (data[offset + index++] << 16);
			y += (data[offset + index++] << 8);
			y += (data[offset + index++]);
			z = (data[offset + index++] << 24);
			z += (data[offset + index++] << 16);
			z += (data[offset + index++] << 8);
			z += (data[offset + index++]);
			return index;
		}
	}

	public struct RobotBattery {
		public int current;
		public float voltage;
		public int charge;
		public int percentage;
		public int status;

		public byte[] Encode() {
			byte[] bytes = new byte[100];
			return bytes;
		}

		public int Decode(byte[] data, int offset) {
			int index = 0;

			int value = (data[offset + index++] << 8);
			value += (data[offset + index++]);
			voltage = value * 0.001f;	//Convert from received mV to V

			current = (data[offset + index++] << 8);
			current += (data[offset + index++]);
			charge = (data[offset + index++] << 8);
			charge += (data[offset + index++]);
			percentage = (data[offset + index++]);
			status = (data[offset + index++]);
			return index;
		}
	}

	public struct RobotOdom {
		public Vector3 velocityLinear;		//In m/s
		public Vector3 velocityAngular;     //In rad/s
		public Vector3 posePoint;           //In m
		public Vector3 poseOrientation;		//In rad

		public byte[] Encode() {
			byte[] bytes = new byte[100];
			return bytes;
		}

		public int Decode(byte[] data, int offset) {
			int index = 0;
			Vector3_q31 value = new Vector3_q31();

			index += value.Decode(data, (offset + index));
			velocityLinear = new Vector3(	value.x * 0.001f,
											value.y * 0.001f,
											value.z * 0.001f);
			index += value.Decode(data, (offset + index));
			velocityAngular = new Vector3(	value.x * 0.001f,
											value.y * 0.001f,
											value.z * 0.001f);
			index += value.Decode(data, (offset + index));
			posePoint = new Vector3(value.x * 0.001f * (1.0f / 128.0f),
									value.y * 0.001f * (1.0f / 128.0f),
									value.z * 0.001f * (1.0f / 128.0f));
			index += value.Decode(data, (offset + index));
			poseOrientation = new Vector3(	value.x * 0.001f * (1.0f / 128.0f),
											value.y * 0.001f * (1.0f / 128.0f),
											value.z * 0.001f * (1.0f / 128.0f));
			return index;
		}
	}

	public struct RobotRange {
		public int count;
		public float[] ranges;	//In meters

		public byte[] Encode() {
			byte[] bytes = new byte[100];
			return bytes;
		}

		public int Decode(byte[] data, int offset) {
			int index = 0;
			count = data[offset + index++];

			ranges = new float[count];
			for(int i = 0; i < count; i++) {
				int value;
				value = (data[offset + index++] << 8);
				value += (data[offset + index++]);
				ranges[i] = value * 0.001f;
			}

			return index;
		}
	}

	public struct RobotContact {
		public int count;
		public bool[] contact;

		public byte[] Encode() {
			byte[] bytes = new byte[100];
			return bytes;
		}

		public int Decode(byte[] data, int offset) {
			int index = 0;
			count = 4;

			contact = new bool[count];
			for (int i = 0; i < count; i++) {
				int value = (data[offset + index++]);
				if (value == 0x01) {
					contact[i] = true;
				}
				else {
					contact[i] = false;
				}
			}

			return index;
		}
	}

	public struct RobotDrive {
		public int wheelRPMLeft;
		public int motorCurrentLeft;
		public int wheelRPMRight;
		public int motorCurrentRight;
		public int motorDriveStatus;

		public byte[] Encode() {
			byte[] bytes = new byte[100];
			return bytes;
		}

		public int Decode(byte[] data, int offset) {
			int index = 0;

			wheelRPMLeft = (data[offset + index++] << 24);
			wheelRPMLeft += (data[offset + index++] << 16);
			wheelRPMLeft += (data[offset + index++] << 8);
			wheelRPMLeft += (data[offset + index++]);

			wheelRPMRight = (data[offset + index++] << 24);
			wheelRPMRight += (data[offset + index++] << 16);
			wheelRPMRight += (data[offset + index++] << 8);
			wheelRPMRight += (data[offset + index++]);

			motorCurrentLeft = (data[offset + index++] << 8);
			motorCurrentLeft += (data[offset + index++]);

			motorCurrentRight = (data[offset + index++] << 8);
			motorCurrentRight += (data[offset + index++]);

			motorDriveStatus = (data[offset + index++]);

			return index;
		}
	}

	public struct RobotTwist {
		public Vector3 velocityLinear;
		public Vector3 velocityAngular;

		public byte[] Encode() {
			int index = 0;
			byte[] bytes = new byte[24];

			Vector3_q31 value = new Vector3_q31();
			value.x = (int)(velocityLinear.x * Mathf.Pow(2, 15));
			value.y = (int)(velocityLinear.y * Mathf.Pow(2, 15));
			value.z = (int)(velocityLinear.z * Mathf.Pow(2, 15));
			System.Array.Copy(value.Encode(), 0, bytes, index, 12);
			index += 12;
			value.x = (int)(velocityAngular.x * Mathf.Pow(2, 15));
			value.y = (int)(velocityAngular.y * Mathf.Pow(2, 15));
			value.z = (int)(velocityAngular.z * Mathf.Pow(2, 15));
			System.Array.Copy(value.Encode(), 0, bytes, index, 12);
			index += 12;

			return bytes;
		}

		public int Decode(byte[] data, int offset) {
			return 0;
		}
	}

	public struct RobotDbgDrive {
		public int pidPLeft;
		public int pidILeft;
		public int pidDLeft;
		public int pidPRight;
		public int pidIRight;
		public int pidDRight;
		public int pwmLeft;
		public int pwmRight;

		public byte[] Encode() {
			byte[] bytes = new byte[100];
			return bytes;
		}

		public int Decode(byte[] data, int offset) {
			int index = 0;

			pidPLeft = (data[offset + index++] << 24);
			pidPLeft += (data[offset + index++] << 16);
			pidPLeft += (data[offset + index++] << 8);
			pidPLeft += (data[offset + index++]);

			pidILeft = (data[offset + index++] << 24);
			pidILeft += (data[offset + index++] << 16);
			pidILeft += (data[offset + index++] << 8);
			pidILeft += (data[offset + index++]);

			pidDLeft = (data[offset + index++] << 24);
			pidDLeft += (data[offset + index++] << 16);
			pidDLeft += (data[offset + index++] << 8);
			pidDLeft += (data[offset + index++]);

			pidPRight = (data[offset + index++] << 24);
			pidPRight += (data[offset + index++] << 16);
			pidPRight += (data[offset + index++] << 8);
			pidPRight += (data[offset + index++]);

			pidIRight = (data[offset + index++] << 24);
			pidIRight += (data[offset + index++] << 16);
			pidIRight += (data[offset + index++] << 8);
			pidIRight += (data[offset + index++]);

			pidDRight = (data[offset + index++] << 24);
			pidDRight += (data[offset + index++] << 16);
			pidDRight += (data[offset + index++] << 8);
			pidDRight += (data[offset + index++]);

			pwmLeft = (data[offset + index++] << 24);
			pwmLeft += (data[offset + index++] << 16);
			pwmLeft = pwmLeft >> 16;

			pwmRight = (data[offset + index++] << 24);
			pwmRight += (data[offset + index++] << 16);
			pwmRight = pwmRight >> 16;

			return index;
		}
	}

	public struct RobotLaserScan {
		public float[][] ranges;

		public RobotLaserScan(int v, int h) {
			ranges = new float[8][];
			ranges[0] = new float[64];
			ranges[1] = new float[64];
			ranges[2] = new float[64];
			ranges[3] = new float[64];
			ranges[4] = new float[64];
			ranges[5] = new float[64];
			ranges[6] = new float[64];
			ranges[7] = new float[64];
		}

		public byte[] Encode() {
			byte[] bytes = new byte[100];
			return bytes;
		}

		public int Decode(byte[] data, int offset) {
			int index = 0;

			for(int v = 0; v < 8; v++) {
				for (int h = 0; h < 64; h++) {
					int value;
					value = (data[offset + index++] << 8);
					value += (data[offset + index++]);

					if(ranges.Length > v && ranges[v].Length > h) {
						ranges[v][h] = (float)(value * 0.001f);     //Convert from mm to m
					}
				}
			}

			return index;
		}
	}
}
