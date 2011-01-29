using System;
using System.Text;

namespace AspNet.Taco
{
    public class BodyWriter {
        private readonly Action<byte[], int, int> _write;
        private readonly Encoding _encoding;

        public BodyWriter(Action<byte[], int, int> write, Encoding encoding) {
            _write = write;
            _encoding = encoding;
        }

        public void Write(object data) {
            if (data is byte[]) {
                var buffer = (byte[])data;
                _write(buffer, 0, buffer.Length);
            }
            else if (data is ArraySegment<byte>) {
                var segment = (ArraySegment<byte>)data;
                _write(segment.Array, segment.Offset, segment.Count);
            }
            else {
                var bytes = _encoding.GetBytes(Convert.ToString(data));
                _write(bytes, 0, bytes.Length);
            }
        }
    }
}