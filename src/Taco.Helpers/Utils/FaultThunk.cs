using System;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;

namespace Taco.Helpers.Utils {
    class FaultThunk : MarshalByRefObject, ILogicalThreadAffinative {
        private readonly Action<Exception> _fault;

        private FaultThunk(Action<Exception> fault) {
            _fault = fault;
        }

        protected FaultThunk(SerializationInfo info, StreamingContext context) {
            _fault = (Action<Exception>)Marshal.GetDelegateForFunctionPointer((IntPtr)info.GetValue("_fault", typeof(IntPtr)), typeof(Action<Exception>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("_fault", Marshal.GetFunctionPointerForDelegate(_fault), typeof(IntPtr));
        }

        private void Fire(Exception ex) {
            _fault(ex);
        }

        public static Action<Exception> Current {
            get {
                var thunk = CallContext.GetData("taco.fault");
                if (thunk != null) return ex => ((FaultThunk)thunk).Fire(ex);
                return null;
            }
            set {
                CallContext.SetData("taco.fault", new FaultThunk(value));
            }
        }

        public static IDisposable Scope(Action<Exception> ex) {
            var prior = Current;
            Current = ex;
            return new Disposable(() => Current = prior);
        }
    }
}