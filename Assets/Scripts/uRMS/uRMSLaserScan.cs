using System;

namespace uRMSConnector {
	public class uRMSLaserScan : uRMSMessage {
		public short angleStartV;
		public short angleStopV;
		public short angleStepV;
		public short angleStartH;
		public short angleStopH;
		public short angleStepH;
		public byte countV;
		public byte countH;
		public ushort[][] ranges;
		public byte[][] quality;

		const int size = 6;

		public uRMSLaserScan() {
			this.countV = 0;
			this.countH = 0;
		}
		public int Size() {
			return (size + countV * countH * 3);
		}

		public static int Serialize(uRMSLaserScan src, ref uRMSMessage dst, int offset) {
			int i = 0;
			dst.payload = new byte[(size + src.countV * src.countH * 3)];
			i += Serialize(src.angleStartV, ref dst.payload, offset + i);
			i += Serialize(src.angleStopV, ref dst.payload, offset + i);
			i += Serialize(src.angleStepV, ref dst.payload, offset + i);
			i += Serialize(src.angleStartH, ref dst.payload, offset + i);
			i += Serialize(src.angleStopH, ref dst.payload, offset + i);
			i += Serialize(src.angleStepH, ref dst.payload, offset + i);

			if(src.angleStepV == 0 || src.angleStepH == 0) {
				src.countV = 0;
				src.countH = 0;
			}
			else {
				src.countV = (byte)(((src.angleStopV - src.angleStartV) / src.angleStepV) + 1);
				src.countH = (byte)(((src.angleStopH - src.angleStartH) / src.angleStepH) + 1);
			}
			for (int v = 0; v < src.countV; v++) {
				for (int h = 0; h < src.countH; i++) {
					h += Serialize(src.ranges[v][h], ref dst.payload, offset + i);
					h += Serialize(src.quality[v][h], ref dst.payload, offset + i);
				}
			}
			return i;
		}

		public static int Deserialize(uRMSMessage src, ref uRMSLaserScan dst, int offset) {
			int i = 0;
			i += Deserialize(src.payload, ref dst.angleStartV, offset + i);
			i += Deserialize(src.payload, ref dst.angleStopV, offset + i);
			i += Deserialize(src.payload, ref dst.angleStepV, offset + i);
			i += Deserialize(src.payload, ref dst.angleStartH, offset + i);
			i += Deserialize(src.payload, ref dst.angleStopH, offset + i);
			i += Deserialize(src.payload, ref dst.angleStepH, offset + i);

			if (dst.angleStepV == 0 || dst.angleStepH == 0) {
				dst.countV = 0;
				dst.countH = 0;
			}
			else {
				dst.countV = (byte)(((dst.angleStopV - dst.angleStartV) / dst.angleStepV) + 1);
				dst.countH = (byte)(((dst.angleStopH - dst.angleStartH) / dst.angleStepH) + 1);
			}
			if (dst.countV == 0 || dst.countH == 0) {
				dst.ranges = null;
			}
			else {
				dst.ranges = new ushort[dst.countV][];
				dst.quality = new byte[dst.countV][];
				for (int v = 0; v < dst.countV; v++) {
					dst.ranges[v] = new ushort[dst.countH];
					dst.quality[v] = new byte[dst.countH];
					for (int h = 0; h < dst.countH; h++) {
						i += Deserialize(src.payload, ref dst.ranges[v][h], offset + i);
						i += Deserialize(src.payload, ref dst.quality[v][h], offset + i);
					}
				}
			}
			return i;
		}
	}
}