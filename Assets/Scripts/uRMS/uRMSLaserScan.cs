using System;

namespace uRMSConnector {
	public class uRMSLaserScan : uRMSMessage {
		public byte countV;
		public byte countH;
		public ushort[][] ranges;

		const int size = 2;

		public uRMSLaserScan() {
			this.countV = 0;
			this.countH = 0;
		}
		public int Size() {
			return (size + countV * countH * 2);
		}

		public static int Serialize(uRMSLaserScan src, ref uRMSMessage dst, int offset) {
			int i = 0;
			dst.payload = new byte[(size + src.countV * src.countH * 2)];
			i += Serialize(src.countV, ref dst.payload, offset + i);
			i += Serialize(src.countH, ref dst.payload, offset + i);
			for(int v = 0; v < src.countV; v++) {
				for (int h = 0; h < src.countH; i++) {
					h += Serialize(src.ranges[v][h], ref dst.payload, offset + i);
				}
			}
			return i;
		}

		public static int Deserialize(uRMSMessage src, ref uRMSLaserScan dst, int offset) {
			int i = 0;
			i += Deserialize(src.payload, ref dst.countV, offset + i);
			i += Deserialize(src.payload, ref dst.countH, offset + i);
			if(dst.countV == 0 || dst.countH == 0) {
				dst.ranges = null;
			}
			else {
				dst.ranges = new ushort[dst.countV][];
				for (int v = 0; v < dst.countV; v++) {
					dst.ranges[v] = new ushort[dst.countH];
					for (int h = 0; h < dst.countH; h++) {
						i += Deserialize(src.payload, ref dst.ranges[v][h], offset + i);
					}
				}
			}
			return i;
		}
	}
}