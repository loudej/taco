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

namespace Taco.Startup {
    using LoggerAction = Action<TraceEventType /*traceEventType*/, Func<string> /*message*/, Exception /*exception*/>;

    public class Environment {
        readonly IDictionary<string, object> _env;

        public Environment(IDictionary<string, object> env) {
            _env = env;
        }

        T Get<T>(string name) {
            object value;
            return _env.TryGetValue(name, out value) ? (T)value : default(T);
        }

        void Set<T>(string name, T value) {
            _env[name] = value;
        }

        public Version Version {
            get { return Get<Version>("taco.version"); }
            set { Set("taco.version", value); }
        }

        public string UrlScheme {
            get { return Get<string>("taco.url_scheme"); }
            set { Set("taco.url_scheme", value); }
        }

        public IObservable<Cargo<ArraySegment<byte>>> Body {
            get { return Get<IObservable<Cargo<ArraySegment<byte>>>>("taco.input"); }
            set { Set("taco.input", value); }
        }

        //TODO: what is the body data?
        public object Errors {
            get { return Get<object>("taco.errors"); }
            set { Set("taco.errors", value); }
        }

        //TODO: what is default error pipe?
        public bool Multithread {
            get { return Get<bool>("taco.multithread"); }
            set { Set("taco.multithread", value); }
        }

        public bool Multiprocess {
            get { return Get<bool>("taco.multiprocess"); }
            set { Set("taco.multiprocess", value); }
        }

        public bool RunOnce {
            get { return Get<bool>("taco.run_once"); }
            set { Set("taco.run_once", value); }
        }

        public IDictionary<string, object> Session {
            get { return Get<IDictionary<string, object>>("taco.session"); }
            set { Set("taco.session", value); }
        }

        //TODO: what is default session interface?
        public LoggerAction Logger {
            get { return Get<LoggerAction>("taco.logger"); }
            set { Set("taco.logger", value); }
        }
    }
}