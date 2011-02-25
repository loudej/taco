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
using Taco;
using Taco.Helpers;

[assembly: Builder("Path", typeof(Path), "Create")]

namespace Taco.Helpers {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>>>;

    public class Path {
        public static AppAction Create(AppAction app, string path) {
            return (env, fault, result) => {
                var request = new Request(env);
                if (request.PathInfo.StartsWith(path, StringComparison.OrdinalIgnoreCase)) {
                    if (request.PathInfo.Length == path.Length || request.PathInfo[path.Length] == '/') {
                        env["SCRIPT_NAME"] = request.ScriptName + path;
                        env["PATH_INFO"] = request.PathInfo.Substring(path.Length);
                        app(env, fault, result);
                        return;
                    }
                }

                new Response(result) {Status = 404}.Finish();
            };
        }
    }
}