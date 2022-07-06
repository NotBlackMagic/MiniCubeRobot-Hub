namespace uRMSConnector {
	public class uRMSStdTypes {
		public static int Serialize(byte src, ref byte[] dst, int offset) {
			int i = 0;
			dst[offset + i++] = (byte)(src);
			return i;
		}

		public static int Deserialize(byte[] src, ref byte dst, int offset) {
			int i = 0;
			dst = (src[offset + i++]);
			return i;
		}
		public static int Serialize(ushort src, ref byte[] dst, int offset) {
			int i = 0;
			dst[offset + i++] = (byte)(src >> 8);
			dst[offset + i++] = (byte)(src);
			return i;
		}

		public static int Deserialize(byte[] src, ref ushort dst, int offset) {
			int i = 0;
			dst = (ushort)(src[offset + i++] << 8);
			dst += (src[offset + i++]);
			return i;
		}

		public static int Serialize(int src, ref byte[] dst, int offset) {
			int i = 0;
			dst[offset + i++] = (byte)(src >> 24);
			dst[offset + i++] = (byte)(src >> 16);
			dst[offset + i++] = (byte)(src >> 8);
			dst[offset + i++] = (byte)(src);
			return i;
		}

		public static int Deserialize(byte[] src, ref int dst, int offset) {
			int i = 0;
			dst = (src[offset + i++] << 24);
			dst += (src[offset + i++] << 16);
			dst += (src[offset + i++] << 8);
			dst += (src[offset + i++]);
			return i;
		}
		public class Vector3_q31 : uRMSStdTypes {
			public int x;
			public int y;
			public int z;

			public Vector3_q31() {
				this.x = 0;
				this.y = 0;
				this.z = 0;
			}

			public Vector3_q31(int x, int y, int z) {
				this.x = x;
				this.y = y;
				this.z = z;
			}

			public static int Serialize(Vector3_q31 src, ref byte[] dst, int offset) {
				int i = 0;
				i += Serialize(src.x, ref dst, offset + i);
				i += Serialize(src.y, ref dst, offset + i);
				i += Serialize(src.z, ref dst, offset + i);
				return i;
			}

			public static int Deserialize(byte[] src, ref Vector3_q31 dst, int offset) {
				int i = 0;
				i += Deserialize(src, ref dst.x, offset + i);
				i += Deserialize(src, ref dst.y, offset + i);
				i += Deserialize(src, ref dst.z, offset + i);
				return i;
			}
		}
	}
}
