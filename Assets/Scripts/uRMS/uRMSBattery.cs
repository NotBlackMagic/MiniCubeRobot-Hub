namespace uRMSConnector {
	public class uRMSBattery : uRMSMessage {
		public ushort voltage;
		public ushort current;
		public ushort charge;
		public byte percentage;
		public byte status;

		const int size = 8;

		public uRMSBattery() {
			this.header = new Header();
			this.voltage = 0;
			this.current = 0;
			this.charge = 0;
			this.percentage = 0;
			this.status = 0;
		}

		public uRMSBattery(Header header, ushort current, ushort voltage, ushort charge, byte percentage, byte status) {
			this.header = header;
			this.voltage = voltage;
			this.current = current;
			this.charge = charge;
			this.percentage = percentage;
			this.status = status;
		}

		public int Size() {
			return size;
		}

		public static int Serialize(uRMSBattery src, ref uRMSMessage dst, int offset) {
			int i = 0;
			dst.payload = new byte[size];
			i += Serialize(src.voltage, ref dst.payload, offset + i);
			i += Serialize(src.current, ref dst.payload, offset + i);
			i += Serialize(src.charge, ref dst.payload, offset + i);
			i += Serialize(src.percentage, ref dst.payload, offset + i);
			i += Serialize(src.status, ref dst.payload, offset + i);
			return i;
		}

		public static int Deserialize(uRMSMessage src, ref uRMSBattery dst, int offset) {
			int i = 0;
			i += Deserialize(src.payload, ref dst.voltage, offset + i);
			i += Deserialize(src.payload, ref dst.current, offset + i);
			i += Deserialize(src.payload, ref dst.charge, offset + i);
			i += Deserialize(src.payload, ref dst.percentage, offset + i);
			i += Deserialize(src.payload, ref dst.status, offset + i);
			return i;
		}
	}
}
