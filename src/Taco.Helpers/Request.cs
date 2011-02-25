// Licensed to .NET HTTP Abstractions (the "Project") under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The Project licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//  
//   http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
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