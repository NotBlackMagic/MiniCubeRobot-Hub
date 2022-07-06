using System;
using System.IO.Ports;
using System.Threading;

public class LiDARConnection {
	//Serial Port Variables
	string port = "COM3";
	int baudrate = 115200;
	bool rxUSBThreadRun = true;
	Thread rxUSBThread;
	SerialPort serialPort = new SerialPort();

	//Scan variables
	bool scanOverflow;
	int scanSamplesIndex;
	double[] scanSamplesSignalQuality;
	double[] scanSamplesRange;

	Action<double[], double[], int> onNewScanReceiveCallback;
	Action<int> onNewRPMReceiveCallback;

	public LiDARConnection() {

	}

	~LiDARConnection() {
		this.Disconnect();
	}

	public bool Connect(string port, int baudrate, Action<double[], double[], int> scanCallback, Action<int> rpmCallback) {
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
				this.onNewScanReceiveCallback = scanCallback;
				this.onNewRPMReceiveCallback = rpmCallback;

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
					if(index == 0) {
						//Get 1st Sync Flag
						if (rxBuffer[i] == 0xAA) {
							//1st Sync Flag received
							rxDataArray[index++] = rxBuffer[i];
						}
					}
					else if(index == 1) {
						//Get 2nd Sync Flag
						if (rxBuffer[i] == 0x00) {
							//2nd Sync Flag received
							rxDataArray[index++] = rxBuffer[i];
						}
						else {
							//Sync failed, restart search
							index = 0;
						}
					}
					else if(index == 2) {
						//Get length field
						rxDataPacketLength = rxBuffer[i] + 2;
						rxDataArray[index++] = rxBuffer[i];
					}
					else if(index < rxDataPacketLength) {
						//Get rest of the packet bytes
						rxDataArray[index++] = rxBuffer[i];
						//Check for full packet received
						if(index == rxDataPacketLength) {
							//Full packet received, get crc from packege (last two bytes)
							//Calcualte and check CRC
							int pktCRC = (rxDataArray[index - 2] << 8);
							pktCRC += rxDataArray[index - 1];

							LiDARPacketProcessing(rxDataArray, index);

							index = 0;
						}
					}
				}
			}
		}
	}

	private void LiDARPacketProcessing(byte[] packet, int packetLength) {
		byte cmdCode = packet[5];
		switch(cmdCode) {
			case 0xAE: {
				//Get RPM data
				int rpm = packet[8] * 3;
				onNewRPMReceiveCallback(rpm);
				break;
			}
			case 0xAD: {
				//Get RPM data
				int rpm = packet[8] * 3;
				onNewRPMReceiveCallback(rpm);

				//Get LiDAR data length
				int samplesCnt = (packet[7] - 5) / 3;
				//Get LiDAR packet start angle
				int startAngleInt = (packet[11] << 8);
				startAngleInt += packet[12];
				double startAngle = startAngleInt * 0.01;
				int angleIndex = (int)(startAngle / (360.0 / 15.0));

				if (angleIndex == 0) {
					//New scan started
					scanSamplesRange = new double[samplesCnt * 15];
					scanSamplesSignalQuality = new double[samplesCnt * 15];
					scanSamplesIndex = 0;
				}

				//Get LiDAR samples
				for(int i = 0; i < samplesCnt; i++) {
					int signalQuality = packet[13 + (i * 3)];
					int distance = (packet[13 + (i * 3) + 1] << 8);
					distance += packet[13 + (i * 3) + 2];
					if(scanSamplesRange != null && scanSamplesIndex < scanSamplesRange.Length) {
						scanSamplesSignalQuality[scanSamplesIndex] = signalQuality;
						scanSamplesRange[scanSamplesIndex++] = distance * 0.01; //Convert from cm to m
					}
				}

				if(angleIndex == 14) {
					//Scan complete
					if(scanSamplesRange != null && scanSamplesIndex == scanSamplesRange.Length) {
						//Valid scan array
						onNewScanReceiveCallback(scanSamplesRange, scanSamplesSignalQuality, scanSamplesRange.Length);
					}
				}
				break;
			}
		}
	}
}
