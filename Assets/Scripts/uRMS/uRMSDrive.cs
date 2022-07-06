namespace uRMSConnector {
	public class uRMSDrive : uRMSMessage {
		public int wheelRPMLeft;        //In mm/s
		public int wheelRPMRight;       //In mm/s
		public ushort motorCurrentLeft;    //In mA/s
		public ushort motorCurrentRight;   //In mA/s
		public byte motorDriveStatus;

		const int size = 13;

		public uRMSDrive() {
			this.header = new Header();
			this.wheelRPMLeft = 0;
			this.wheelRPMRight = 0;
			this.motorCurrentLeft = 0;
			this.motorCurrentRight = 0;
			this.motorDriveStatus = 0;
		}

		public int Size() {
			return size;
		}

		public static int Serialize(uRMSDrive src, ref uRMSMessage dst, int offset) {
			int i = 0;
			dst.payload = new byte[size];
			i += Serialize(src.wheelRPMLeft, ref dst.payload, offset + i);
			i += Serialize(src.wheelRPMRight, ref dst.payload, offset + i);
			i += Serialize(src.motorCurrentLeft, ref dst.payload, offset + i);
			i += Serialize(src.motorCurrentRight, ref dst.payload, offset + i);
			i += Serialize(src.motorDriveStatus, ref dst.payload, offset + i);
			return i;
		}

		public static int Deserialize(uRMSMessage src, ref uRMSDrive dst, int offset) {
			int i = 0;
			i += Deserialize(src.payload, ref dst.wheelRPMLeft, offset + i);
			i += Deserialize(src.payload, ref dst.wheelRPMRight, offset + i);
			i += Deserialize(src.payload, ref dst.motorCurrentLeft, offset + i);
			i += Deserialize(src.payload, ref dst.motorCurrentRight, offset + i);
			i += Deserialize(src.payload, ref dst.motorDriveStatus, offset + i);
			return i;
		}
	}
}
