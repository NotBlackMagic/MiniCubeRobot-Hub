using System;
using System.IO.Ports;
using System.Threading;

namespace NBMSerial {
	public class CCITTCRC {
		int crc;
		static readonly int[] ccittTable = {	0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7, 0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF,
												0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6, 0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE,
												0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485, 0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D,
												0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4, 0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC,
												0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823, 0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B,
												0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12, 0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A,
												0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41, 0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49,
												0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70, 0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78,
												0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F, 0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067,
												0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E, 0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256,
												0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D, 0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
												0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C, 0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634,
												0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB, 0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3,
												0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A, 0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92,
												0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9, 0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1,
												0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8, 0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0 };

		public CCITTCRC() {
			crc = 0xFFFF;
		}

		public void Write(byte data) {
			int index = (data ^ (crc >> 8)) & 0xFF;
			crc = ccittTable[index] ^ (crc << 8);
		}

		public int Read() {
			crc = ((crc ^ 0xFFFF) & 0xFFFF);
			return crc;
		}

		public void Reset() {
			crc = 0xFFFF;
		}
	}
	public class NBMSerialPort {
		//Serial Port Variables
		string port = "COM3";
		int baudrate = 115200;
		bool rxUSBThreadRun = true;
		Thread rxUSBThread;
		SerialPort serialPort = new SerialPort();

		Action<byte[], int> onReceiveCallback;

		public NBMSerialPort() {

		}

		~NBMSerialPort() {
			this.Disconnect();
		}

		public bool Connect(string port, int baudrate, Action<byte[], int> callback) {
			if (serialPort.IsOpen == false) {
				try {
					serialPort.PortName = port;
					serialPort.BaudRate = baudrate;
					serialPort.DataBits = 8;
					serialPort.StopBits = StopBits.One;
					serialPort.Parity = Parity.None;
					serialPort.NewLine = "\n";
					serialPort.Open();

					this.port = port;
					this.baudrate = baudrate;
					this.onReceiveCallback = callback;

					//USBWrite(Opcodes.connect, null);

					//usbRXStopwatch.Start();

					rxUSBThreadRun = true;
					rxUSBThread = new Thread(USBRXThread);
					rxUSBThread.Start();
				}
				catch (System.Exception ex) {
					Console.WriteLine("ERROR: Serial Connect Error" + ex.Message);
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
			int index = 0;
			int rxDataPacketLength = 0;
			byte[] rxDataArray = new byte[2048];
			CCITTCRC crc = new CCITTCRC();

			byte[] rxBuffer = new byte[2048];
			while (rxUSBThreadRun) {
				if (serialPort.IsOpen) {
					int rxLength = 0;
					try {
						rxLength = serialPort.BaseStream.Read(rxBuffer, 0, rxBuffer.Length);
					}
					catch {
						Console.WriteLine("ERROR: serialPort.BaseStream.Read()");
						return;
					}

					for (int i = 0; i < rxLength; i++) {
						//**********************//
						//		UART Frame		//
						//**********************//
						//|  u16   | n*u8 | u16 |
						//|--------|------|-----|
						//| Length | Data | CRC |
						if (index == 0) {
							//This is the MSB packet length field, byte 0
							rxDataPacketLength = (rxBuffer[i] << 8);
							index += 1;
							crc.Reset();
							crc.Write(rxBuffer[i]);
						}
						else if (index == 1) {
							//This is the LSB packet length field, byte 1
							rxDataPacketLength += rxBuffer[i];
							crc.Write(rxBuffer[i]);
							if (rxDataPacketLength > rxDataArray.Length) {
								//RX Buffer overflow
								index = 0;
								Console.WriteLine("ERROR: RX Buffer overflow");
								break;
							}
							else if (rxDataPacketLength <= 4) {
								//Invalid payload length
								index = 0;
								Console.WriteLine("ERROR: Invalid Payload Length");
								break;
							}
							else {
								index += 1;
							}
						}
						else {
							rxDataArray[index - 2] = rxBuffer[i];
							index += 1;

							if (index == (rxDataPacketLength + 4)) {
								//Full packet received, get crc from packege (last two bytes)
								//Calcualte and check CRC
								for (int j = 0; j < rxDataPacketLength; j++) {
									crc.Write(rxDataArray[j]);
								}

								int pktCRC = rxDataArray[index - 4] << 8;
								pktCRC += rxDataArray[index - 3];

								if (pktCRC != crc.Read()) {
									//Packet CRC failed
									index = 0;
									Console.WriteLine("ERROR: RX Packet CRC Error");
									break;
								}

								onReceiveCallback(rxDataArray, index);

								index = 0;
							}
						}
					}
				}
			}
		}

		public void Send(byte[] data, int dataLength) {
			if (serialPort.IsOpen) {
				//**********************//
				//		UART Frame		//
				//**********************//
				//|  u16   | n*u8 | u16 |
				//|--------|------|-----|
				//| Length | Data | CRC |
				int index = 0;
				byte[] buffer = new byte[dataLength + 4];

				//Add length field
				buffer[index++] = (byte)((dataLength >> 8) & 0xFF);
				buffer[index++] = (byte)((dataLength) & 0xFF);

				//Add payload/data
				Array.Copy(data, 0, buffer, index, dataLength);
				index = index + dataLength;

				//Calculate and add CRC
				CCITTCRC crc = new CCITTCRC();
				for(int i = 0; i < (dataLength + 2); i++) {
					crc.Write(buffer[i]);
				}
				int pktCRC = crc.Read();
				buffer[index++] = (byte)((pktCRC >> 8) & 0xFF);
				buffer[index++] = (byte)((pktCRC) & 0xFF);

				serialPort.Write(buffer, 0, buffer.Length);
			}
		}
	}
}

