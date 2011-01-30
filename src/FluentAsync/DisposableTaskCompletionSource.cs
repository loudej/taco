// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.Threading.Tasks;

namespace FluentAsync {
    public sealed class DisposableTaskCompletionSource<T> : TaskCompletionSource<T>, IDisposable {
        public DisposableTaskCompletionSource() {}

        public DisposableTaskCompletionSource(TaskCreationOptions creationOptions)
            : base(creationOptions) {}

        public DisposableTaskCompletionSource(object state)
            : base(state) {}

        public DisposableTaskCompletionSource(object state, TaskCreationOptions creationOptions)
            : base(state, creationOptions) {}

        ~DisposableTaskCompletionSource() {
            Dispose(false);
        }

        void IDisposable.Dispose() {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        void Dispose(bool disposing) {
            TrySetCanceled();
        }
    }
}