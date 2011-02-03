// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.IO;
using Taco;

namespace AspNet.Taco {
    class RequestBody : IObservable<Cargo<object>> {
        readonly Stream _stream;

        public RequestBody(Stream stream) {
            _stream = stream;
        }

        public IDisposable Subscribe(IObserver<Cargo<object>> observer) {
            var buffer = new byte[4096];
            return Loop.Run((halted, continuation) => {
                try {
                    _stream.BeginRead(buffer, 0, buffer.Length, ar => {
                        try {
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
                        }
                        catch (Exception ex) {
                            observer.OnError(ex);
                        }
                    }, null);
                }
                catch (Exception ex) {
                    observer.OnError(ex);
                }
            });
        }
    }
}
