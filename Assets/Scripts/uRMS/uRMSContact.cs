using System;

namespace uRMSConnector {
	public class uRMSContact : uRMSMessage {
		public byte count;
		public byte[] contact;

		const int size = 1;

		public uRMSContact() {
			this.count = 0;
		}
		public int Size() {
			return size + count;
		}

		public static int Serialize(uRMSContact src, ref uRMSMessage dst, int offset) {
			int i = 0;
			dst.payload = new byte[size + src.count];
			i += Serialize(src.count, ref dst.payload, offset + i);
			for (int c = 0; c < src.count; c++) {
				i += Serialize(src.contact[c], ref dst.payload, offset + i);
			}
			return i;
		}

		public static int Deserialize(uRMSMessage src, ref uRMSContact dst, int offset) {
			int i = 0;
			i += Deserialize(src.payload, ref dst.count, offset + i);
			dst.contact = new byte[dst.count];
			for (int c = 0; c < dst.count; c++) {
				i += Deserialize(src.payload, ref dst.contact[c], offset + i);
			}
			return i;
		}
	}
}