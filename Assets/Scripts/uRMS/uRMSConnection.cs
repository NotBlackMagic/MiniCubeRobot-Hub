using System;
using System.Collections;
using System.Collections.Generic;
using NBMSerial;

namespace uRMSConnector {
	public class uRMSConnection {
		//Serial Port Variables
		string port = "COM2";
		int baudrate = 115200;
		NBMSerialPort serialPort = new NBMSerialPort();

		public static uRMSConnection instance = new uRMSConnection();

		List<Action<uRMSMessage>> subscriberCallback = new List<Action<uRMSMessage>>();
		public uRMSConnection() {
			this.Connect();
		}

		~uRMSConnection() {
			serialPort.Disconnect();
		}

		bool Connect() {
			return serialPort.Connect(this.port, this.baudrate, OnReceive);
		}

		bool Disconnect() {
			return serialPort.Disconnect();
		}

		public bool IsConnected() {
			return serialPort.IsConnected();
		}

		private void OnReceive(byte[] data, int dataLength) {
			//Decode packet
			uRMSMessage message = new uRMSMessage();
			int offset = uRMSMessage.Deserialize(data, ref message, 0);
			foreach (Action<uRMSMessage> action in subscriberCallback) {
				action.Invoke(message);
			}
		}

		public void Subscribe<T>(ushort topicID, Action<T> callback) where T : uRMSMessage {
			subscriberCallback.Add((uRMSMessage message) => {
				if(message.header.topicID == topicID) {
					callback((T)message);
				}
			});
		}

		public void Publish(uRMSMessage message) {
			byte[] data = new byte[message.payload.Length + message.header.Size()];
			uRMSMessage.Serialize(message, ref data, 0);
			serialPort.Send(data, data.Length);
		}
	}
}