using System;
using System.Collections.Generic;
using System.IO;
using Taco.Helpers.Utils;

namespace Taco.Helpers {
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

        public IObservable<object> Body { get { return Get<IObservable<object>>("taco.input"); } }
        public string ScriptName { get { return Get<string>("SCRIPT_NAME"); } }
        public string PathInfo { get { return Get<string>("PATH_INFO"); } }
        public string RequestMethod { get { return Get<string>("REQUEST_METHOD"); } }
        public string QueryString { get { return Get<string>("QUERY_STRING"); } }
        public string ContentLength { get { return Get<string>("CONTENT_LENGTH"); } }
        public string ContentType { get { return Get<string>("CONTENT_TYPE"); } }
        //public ISession Session { get { return Get<ISession>("rack.session"); } }
        //public string SessionOptions{get { return Get<string>("rack.session.options"); }}
        public Logger Logger { get { return Logger.For(Get<Log>("taco.logger")); } }

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
