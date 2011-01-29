using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Taco {
    using FnApp = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<object>>>;

    //public delegate void App<in TFault, in TResult>(IDictionary<string, object> env, TFault fault, TResult result);
    //public delegate void Fault(Exception exception);
    //public delegate void Result(int status, IDictionary<string, string> headers, IObservable<object> body);
    public delegate void Log(TraceEventType traceEventType, Func<string> message, Exception exception);

}

