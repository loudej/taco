using System;

namespace Taco.Helpers.Utils {
    class Disposable : IDisposable {
        private readonly Action _dispose;
        public Disposable() : this(() => { }) { }
        public Disposable(Action dispose) { _dispose = dispose; }
        public void Dispose() { _dispose(); }
        public static IDisposable Noop { get { return new Disposable(); } }
    }
}