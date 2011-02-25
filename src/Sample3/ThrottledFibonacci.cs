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
using System.Text;
using Sample3;
using Taco;
using Taco.Helpers.Utils;

[assembly: Builder("ThrottledFibonacci", typeof(ThrottledFibonacci), "App")]

namespace Sample3 {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>>>;

    public static class EncodingExtensions {
        public static ArraySegment<byte> ToBytes(this string text) {
            return new ArraySegment<byte>(Encoding.Default.GetBytes(text));
        }
    }

    public class ThrottledFibonacci {
        public static AppAction App() {
            return (env, fault, result) => {
                var body = ObservableExtensions.Create<Cargo<ArraySegment<byte>>>((next, error, complete) => {
                    next.InvokeSync("<ul>".ToBytes());
                    next.InvokeSync("<li>0 0</li>".ToBytes());
                    next.InvokeSync("<li>1 1</li>".ToBytes());
                    var n = 2;
                    var fnminus2 = 0.0;
                    var fnminus1 = 1.0;
                    return Loop.Run((halted, continuation) => {
                        while (!halted()) {
                            var fn = fnminus1 + fnminus2;
                            fnminus2 = fnminus1;
                            fnminus1 = fn;
                            n = n + 1;
                            if (next.InvokeAsync(string.Format("<li>{0} {1}</li>", n - 1, fn).ToBytes(), continuation))
                                return;
                        }
                    });
                });
                result(200, new Dictionary<string, string> {{"Content-Type", "text/html"}}, body);
            };
        }
    }
}