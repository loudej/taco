using System;
using System.IO;
using System.Threading;
using Taco;

namespace Sample3 {
    class BodyStream : Stream {
        private readonly Action<Cargo<ArraySegment<byte>>> _next;
        private readonly Action<Exception> _error;
        private readonly Action _complete;

        public BodyStream(Action<Cargo<ArraySegment<byte>>> next, Action<Exception> error, Action complete) {
            _next = next;
            _error = error;
            _complete = complete;
        }


        public override void Flush() {
        }


        public override bool CanRead {
            get { return false; }
        }

        public override bool CanSeek {
            get { return false; }
        }

        public override bool CanWrite {
            get { return true; }
        }

        public override void Write(byte[] buffer, int offset, int count) {
            var data = new ArraySegment<byte>(buffer, offset, count);
            var cargo = Cargo.From(data);
            _next(cargo);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
            var result = new Result(callback, state);
            var data = new ArraySegment<byte>(buffer, offset, count);
            var cargo = Cargo.From(data, () => result.Complete(true));
            _next(cargo);
            if (!cargo.Delayed) {
                result.Complete(false);
            }
            return result;
        }

        public override void EndWrite(IAsyncResult asyncResult) {
            if (!asyncResult.IsCompleted)
                asyncResult.AsyncWaitHandle.WaitOne();
        }

        class Result : IAsyncResult {
            private readonly AsyncCallback _callback;

            public Result(AsyncCallback callback, object asyncState) {
                _callback = callback;
                AsyncState = asyncState;
                AsyncWaitHandle = new ManualResetEvent(false);
            }

            public object AsyncState { get; private set; }
            public bool IsCompleted { get; private set; }
            public bool CompletedSynchronously { get; private set; }
            public WaitHandle AsyncWaitHandle { get; private set; }

            public void Complete(bool delayed) {
                CompletedSynchronously = !delayed;
                IsCompleted = true;
                ((ManualResetEvent)AsyncWaitHandle).Set();
                if (_callback != null)
                    _callback(this);
            }
        }

        public override void Close() {
            base.Close();
            _complete();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        public override long Length {
            get { throw new NotImplementedException(); }
        }

        public override long Position {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

    }
}