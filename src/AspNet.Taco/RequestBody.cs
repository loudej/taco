// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.IO;
using Taco;

namespace AspNet.Taco {
    class RequestBody : IObservable<Cargo<ArraySegment<byte>>> {
        readonly Stream _stream;

        public RequestBody(Stream stream) {
            _stream = stream;
        }

        public IDisposable Subscribe(IObserver<Cargo<ArraySegment<byte>>> observer) {
            Action<Exception> error = observer.OnError;
            var buffer = new byte[4096];
            return Loop.Run((halted, continuation) => error.Guard(() =>
                _stream.BeginRead(buffer, 0, buffer.Length, ar => error.Guard(() => {
                    var count = _stream.EndRead(ar);
                    if (halted()) {
                        return;
                    }
                    if (count == 0) {
                        observer.OnCompleted();
                    }
                    else {
                        if (!observer.OnNextAsync(new ArraySegment<byte>(buffer, 0, count), continuation))
                            continuation();
                    }
                }), null)));
        }
    }

    static class ErrorExtensions {
        public static void Guard(this Action<Exception> fault, Action action) {
            try {
                action();
            }
            catch (Exception ex) {
                fault(ex);
            }
        }
    }
}