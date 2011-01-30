// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.IO;

namespace AspNet.Taco {
    class RequestBody : IObservable<object> {
        readonly Stream _stream;

        public RequestBody(Stream stream) {
            _stream = stream;
        }

        class Loop : IDisposable {
            public Action Go;
            public bool Stop;
            public readonly byte[] Buffer = new byte[4096];

            public void Dispose() {
                Stop = true;
            }
        } ;

        public IDisposable Subscribe(IObserver<object> observer) {
            var loop = new Loop();
            loop.Go = () => {
                try {
                    _stream.BeginRead(loop.Buffer, 0, loop.Buffer.Length, ar => {
                        try {
                            var count = _stream.EndRead(ar);
                            if (loop.Stop) {
                                return;
                            }
                            if (count == 0) {
                                observer.OnCompleted();
                            }
                            else {
                                observer.OnNext(new ArraySegment<byte>(loop.Buffer, 0, count));
                                loop.Go();
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
            };

            loop.Go();
            return loop;
        }
    }
}