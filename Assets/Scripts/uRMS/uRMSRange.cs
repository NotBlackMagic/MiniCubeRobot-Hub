using System;

namespace uRMSConnector {
	public class uRMSRange : uRMSMessage {
		public byte count;
		public ushort[] ranges;

		const int size = 1;

		public uRMSRange() {
			this.count = 0;
		}
		public int Size() {
			return size + count * 2;
		}

		public static int Serialize(uRMSRange src, ref uRMSMessage dst, int offset) {
			int i = 0;
			dst.payload = new byte[size + src.count * 2];
			i += Serialize(src.count, ref dst.payload, offset + i);
			for (int c = 0; c < src.count; c++) {
				i += Serialize(src.ranges[c], ref dst.payload, offset + i);
			}
			return i;
		}

		public static int Deserialize(uRMSMessage src, ref uRMSRange dst, int offset) {
			int i = 0;
			i += Deserialize(src.payload, ref dst.count, offset + i);
			dst.ranges = new ushort[dst.count];
			for(int c = 0; c < dst.count; c++) {
				i += Deserialize(src.payload, ref dst.ranges[c], offset + i);
			}
			return i;
		}
	}
}