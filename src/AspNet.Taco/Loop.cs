using System;

namespace AspNet.Taco {
    class Loop : IDisposable {
        readonly Action<Func<bool>, Action> _action;
        bool _halted;

        Loop(Action<Func<bool>, Action> action) {
            _action = action;
        }

        void Resume() { _action(() => _halted, Resume); }
        void IDisposable.Dispose() { _halted = true; }

        public static IDisposable Run(Action<Func<bool>, Action> action) {
            var loop = new Loop(action);
            action(() => false, loop.Resume);
            return loop;
        }
    }
}