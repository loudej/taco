// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.Text;
using Taco;

namespace AspNet.Taco {
    public class ResponseBody {
        readonly Encoding _encoding;
        readonly Action<byte[], int, int> _write;
        readonly Func<byte[], int, int, AsyncCallback, object, IAsyncResult> _beginWrite;
        readonly Action<IAsyncResult> _endWrite;

        public ResponseBody(
            Encoding encoding,
            Action<byte[], int, int> write,
            Func<byte[], int, int, AsyncCallback, object, IAsyncResult> beginWrite,
            Action<IAsyncResult> endWrite) {
            _encoding = encoding;
            _write = write;
            _beginWrite = beginWrite;
            _endWrite = endWrite;
        }


        public void Write(Cargo<object> cargo) {
            var data = Normalize(cargo.Result);
            if (!cargo.Delayable) {
                _write(data.Array, data.Offset, data.Count);
                return;
            }

            var result = _beginWrite(data.Array, data.Offset, data.Count, asyncResult => {
                if (asyncResult.CompletedSynchronously)
                    return;
                _endWrite(asyncResult);
                cargo.Resume();
            }, null);

            if (result.CompletedSynchronously)
                _endWrite(result);
            else
                cargo.Delay();
        }

        ArraySegment<byte> Normalize(object data) {
            if (data is ArraySegment<byte>) {
                return (ArraySegment<byte>)data;
            }
            
            if (data is byte[]) {
                return new ArraySegment<byte>((byte[])data);
            }

            return new ArraySegment<byte>(_encoding.GetBytes(Convert.ToString(data)));
        }
    }
}