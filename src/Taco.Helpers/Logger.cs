using System;
using System.Diagnostics;

namespace Taco.Helpers {
    using LoggerAction = Action<TraceEventType /*traceEventType*/, Func<string> /*message*/, Exception /*exception*/>;

    public class Logger {
        private readonly LoggerAction _writer;
        private static readonly Logger NullLogger = new Logger((traceEventType, message, exception) => { });

        private Logger(LoggerAction writer) {
            _writer = writer;
        }

        public static Logger For(LoggerAction writer) {
            return writer == null ? NullLogger : new Logger(writer);
        }
    }
}
