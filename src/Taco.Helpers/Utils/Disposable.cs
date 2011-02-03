// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;

namespace Taco.Helpers.Utils {
    public class Disposable : IDisposable {
        readonly Action _dispose;

        public Disposable(Action dispose) {
            _dispose = dispose;
        }

        public void Dispose() {
            _dispose();
        }

        public static IDisposable Noop {
            get { return new Disposable(() => { }); }
        }
    }
}