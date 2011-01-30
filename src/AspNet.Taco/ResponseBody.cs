// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.Text;

namespace AspNet.Taco {
    public class ResponseBody {
        readonly Action<byte[], int, int> _write;
        readonly Encoding _encoding;

        public ResponseBody(Action<byte[], int, int> write, Encoding encoding) {
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