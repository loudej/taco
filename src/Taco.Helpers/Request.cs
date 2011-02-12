// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Taco.Helpers.Utils;

namespace Taco.Helpers {
    using LoggerAction = Action<TraceEventType /*traceEventType*/, Func<string> /*message*/, Exception /*exception*/>;

    public class Request {
        readonly IDictionary<string, object> _env;

        public Request(IDictionary<string, object> env) {
            _env = env;
        }

        T Get<T>(string name) {
            object value;
            return _env.TryGetValue(name, out value) ? (T)value : default(T);
        }

        void Set<T>(string name, T value) {
            _env[name] = value;
        }

        public IObservable<Cargo<ArraySegment<byte>>> Body {
            get { return Get<IObservable<Cargo<ArraySegment<byte>>>>("taco.input"); }
        }

        public string ScriptName {
            get { return Get<string>("SCRIPT_NAME"); }
        }

        public string PathInfo {
            get { return Get<string>("PATH_INFO"); }
        }

        public string RequestMethod {
            get { return Get<string>("REQUEST_METHOD"); }
        }

        public string QueryString {
            get { return Get<string>("QUERY_STRING"); }
        }

        public string ContentLength {
            get { return Get<string>("CONTENT_LENGTH"); }
        }

        public string ContentType {
            get { return Get<string>("CONTENT_TYPE"); }
        }

        public IDictionary<string, object> Session {
            get { return Get<IDictionary<string, object>>("taco.session"); }
        }

        public Logger Logger {
            get { return Logger.For(Get<LoggerAction>("taco.logger")); }
        }

        public IDictionary<string, string> GET {
            get {
                if (Get<string>("Taco.Helpers.Request.QueryString") != QueryString) {
                    Set("Taco.Helpers.Request.QueryString", QueryString);
                    Set("Taco.Helpers.Request.GET", ParamDictionary.Parse(QueryString));
                }
                return Get<IDictionary<string, string>>("Taco.Helpers.Request.GET");
            }
        }
    }
}