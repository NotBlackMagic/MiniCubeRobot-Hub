using System;

namespace uRMSConnector {
	public class Header : uRMSStdTypes {
		public ushort topicID;
		public int nsec;
		public ushort frameID;

		const int size = 8;

		public Header() {
			this.topicID = 0;
			this.nsec = 0;
			this.frameID = 0;
		}

		public Header(ushort topicID, int nsec, ushort frameID) {
			this.topicID = topicID;
			this.nsec = nsec;
			this.frameID = frameID;
		}

		public int Size() {
			return size;
		}

		public static int Serialize(Header src, ref byte[] dst, int offset) {
			int i = 0;
			i += Serialize(src.topicID, ref dst, offset + i);
			i += Serialize(src.nsec, ref dst, offset + i);
			i += Serialize(src.frameID, ref dst, offset + i);
			return i;
		}

		public static int Deserialize(byte[] src, ref Header dst, int offset) {
			int i = 0;
			i += Deserialize(src, ref dst.topicID, offset + i);
			i += Deserialize(src, ref dst.nsec, offset + i);
			i += Deserialize(src, ref dst.frameID, offset + i);
			return i;
		}
	}
	public class uRMSMessage : uRMSStdTypes {
		public Header header;
		public byte[] payload;

		public uRMSMessage() {
			this.header = new Header();
		}

		public static int Serialize(uRMSMessage src, ref byte[] dst, int offset) {
			int i = 0;
			i += Header.Serialize(src.header, ref dst, offset + i);
			Array.Copy(src.payload, 0, dst, offset + i, src.payload.Length);
			i += src.payload.Length;
			return i;
		}

		public static int Deserialize(byte[] src, ref uRMSMessage dst, int offset) {
			int i = 0;
			i += Header.Deserialize(src, ref dst.header, offset + i);
			dst.payload = new byte[src.Length - i];
			Array.Copy(src, offset + i, dst.payload, 0, src.Length - i);
			i += src.Length - i;
			return i;
		}

		public static int Deserialize(byte[] src, ref uRMSMessage dst, int offset, int length) {
			int i = 0;
			i += Header.Deserialize(src, ref dst.header, offset + i);
			dst.payload = new byte[length - i];
			Array.Copy(src, offset + i, dst.payload, 0, length - i);
			i += length - i;
			return i;
		}
	}
}
