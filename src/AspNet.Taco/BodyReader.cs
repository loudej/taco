using System;
using System.IO;

namespace AspNet.Taco {
    class BodyReader : IObservable<object> {
        private readonly Stream _stream;

        public BodyReader(Stream stream) {
            _stream = stream;
        }

        class Loop : IDisposable {
            public bool Stop;
            public readonly byte[] Buffer = new byte[4096];
            public Action Next;
            public void Dispose() { Stop = true; }
        };

        public IDisposable Subscribe(IObserver<object> observer) {
            var loop = new Loop();
            loop.Next = () => {
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
                                loop.Next();
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

            loop.Next();
            return loop;
        }
    }
}