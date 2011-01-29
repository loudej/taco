namespace Taco.Helpers {
    public class Logger {
        private readonly Log _writer;
        private static readonly Logger NullLogger = new Logger((a, b, c) => { });

        private Logger(Log writer) {
            _writer = writer;
        }

        public static Logger For(Log writer) {
            return writer == null ? NullLogger : new Logger(writer);
        }
    }
}
