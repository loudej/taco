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


        public void Write(Cargo<ArraySegment<byte>> cargo) {
            if (!cargo.Delayable) {
                _write(cargo.Result.Array, cargo.Result.Offset, cargo.Result.Count);
                return;
            }

            var result = _beginWrite(cargo.Result.Array, cargo.Result.Offset, cargo.Result.Count, asyncResult => {
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
    }
}