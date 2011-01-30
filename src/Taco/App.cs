using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Taco {
    using AppAction = Action<
        IDictionary<string, object> /*env*/,
        Action<Exception> /*fault*/,
        Action<int, IDictionary<string, string>, IObservable<object>> /*result*/>;

    using LoggerAction = Action<
        TraceEventType /*traceEventType*/, 
        Func<string> /*message*/, 
        Exception /*exception*/>;


    //public delegate void App<in TFault, in TResult>(IDictionary<string, object> env, TFault fault, TResult result);
    //public delegate void Fault(Exception exception);
    //public delegate void Result(int status, IDictionary<string, string> headers, IObservable<object> body);
    //public delegate void Log(TraceEventType traceEventType, Func<string> message, Exception exception);

}

