using System;
using System.Diagnostics;

namespace Taco.Helpers {
    using LoggerAction = Action<TraceEventType /*traceEventType*/, Func<string> /*message*/, Exception /*exception*/>;

    public class Logger {
        readonly LoggerAction _writer;
        static readonly Logger NullLogger = new Logger((traceEventType, message, exception) => { });

        Logger(LoggerAction writer) {
            _writer = writer;
        }

        public static Logger For(LoggerAction writer) {
            return writer == null ? NullLogger : new Logger(writer);
        }
    }
}