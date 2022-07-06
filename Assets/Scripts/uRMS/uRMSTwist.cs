namespace uRMSConnector {
	public class uRMSTwist : uRMSMessage {
		public Vector3_q31 velocityLinear;
		public Vector3_q31 velocityAngular;

		const int size = 24;

		public uRMSTwist() {
			this.header = new Header();
			this.velocityLinear = new Vector3_q31();
			this.velocityAngular = new Vector3_q31();
		}

		public int Size() {
			return size;
		}

		public static int Serialize(uRMSTwist src, ref uRMSMessage dst, int offset) {
			int i = 0;
			dst.header = src.header;
			dst.payload = new byte[size];
			i += Vector3_q31.Serialize(src.velocityLinear, ref dst.payload, offset + i);
			i += Vector3_q31.Serialize(src.velocityAngular, ref dst.payload, offset + i);
			return i;
		}

		public static int Deserialize(uRMSMessage src, ref uRMSTwist dst, int offset) {
			int i = 0;
			dst.header = src.header;
			i += Vector3_q31.Deserialize(src.payload, ref dst.velocityLinear, offset + i);
			i += Vector3_q31.Deserialize(src.payload, ref dst.velocityAngular, offset + i);
			return i;
		}
	}
}