namespace uRMSConnector {
	public class uRMSOdometry : uRMSMessage {
		public Vector3_q31 velocityLinear;
		public Vector3_q31 velocityAngular;
		public Vector3_q31 posePoint;
		public Vector3_q31 poseOrientation;

		const int size = 48;

		public uRMSOdometry() {
			this.header = new Header();
			this.velocityLinear = new Vector3_q31();
			this.velocityAngular = new Vector3_q31();
			this.posePoint = new Vector3_q31();
			this.poseOrientation = new Vector3_q31();
		}

		public int Size() {
			return size;
		}

		public static int Serialize(uRMSOdometry src, ref uRMSMessage dst, int offset) {
			int i = 0;
			dst.payload = new byte[size];
			i += Vector3_q31.Serialize(src.velocityLinear, ref dst.payload, offset + i);
			i += Vector3_q31.Serialize(src.velocityAngular, ref dst.payload, offset + i);
			i += Vector3_q31.Serialize(src.posePoint, ref dst.payload, offset + i);
			i += Vector3_q31.Serialize(src.poseOrientation, ref dst.payload, offset + i);
			return i;
		}

		public static int Deserialize(uRMSMessage src, ref uRMSOdometry dst, int offset) {
			int i = 0;
			i += Vector3_q31.Deserialize(src.payload, ref dst.velocityLinear, offset + i);
			i += Vector3_q31.Deserialize(src.payload, ref dst.velocityAngular, offset + i);
			i += Vector3_q31.Deserialize(src.payload, ref dst.posePoint, offset + i);
			i += Vector3_q31.Deserialize(src.payload, ref dst.poseOrientation, offset + i);
			return i;
		}
	}
}