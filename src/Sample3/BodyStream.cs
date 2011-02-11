using System;
using System.IO;
using System.Threading.Tasks;
using Taco;

namespace Sample3
{
    class BodyStream : Stream {
        private readonly Action<Cargo<object>> _next;
        private readonly Action<Exception> _error;
        private readonly Action _complete;

        public BodyStream(Action<Cargo<object>> next, Action<Exception> error, Action complete) {
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
            _next.InvokeSync(data);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
            var data = new ArraySegment<byte>(buffer, offset, count);
            var tcs = new TaskCompletionSource<object>(state);
            if (callback != null)
                tcs.Task.ContinueWith(t => callback(t), TaskContinuationOptions.ExecuteSynchronously);
            if (!_next.InvokeAsync(data, () => tcs.SetResult(null)))
                tcs.SetResult(null);
            return tcs.Task;
        }

        public override void EndWrite(IAsyncResult asyncResult) {
            ((Task)asyncResult).Wait();
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