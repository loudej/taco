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
using System.Linq;
using Taco;
using Taco.Helpers;

[assembly: Builder("Cascade", typeof(Cascade), "Create")]
[assembly: Builder("Cascade", typeof(Cascade), "Create")]

namespace Taco.Helpers {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>>>;

    public class Cascade {
        Action Continue { get; set; }
        Action Transmit { get; set; }
        IDictionary<string, object> Snapshot { get; set; }

        public static AppAction Create(IEnumerable<AppAction> apps) {
            return (env, fault, result) => {
                var iter = apps.GetEnumerator();

                var cascade = new Cascade {
                    Transmit = () =>
                        new Response(result) {
                            Status = 404,
                            ContentType = "text/html"
                        }
                            .Write("<h1>Not Found</h1>")
                            .Finish()
                };

                cascade.Continue = () => {
                    if (!iter.MoveNext()) {
                        cascade.Transmit();
                    }
                    else {
                        cascade.Preserve(env);
                        iter.Current(env, fault, (status, headers, body) => {
                            cascade.Transmit = () => result(status, headers, body);

                            if (status == 404) {
                                cascade.Continue();
                            }
                            else {
                                cascade.Transmit();
                            }
                        });
                    }
                };

                cascade.Continue();
            };
        }

        void Preserve(IDictionary<string, object> env) {
            if (Snapshot == null) {
                Snapshot = new Dictionary<string, object>(env);
            }
            else {
                foreach (var kv in Snapshot) {
                    env[kv.Key] = kv.Value;
                }
                foreach (var k in env.Keys.ToArray().Where(k => !Snapshot.ContainsKey(k))) {
                    env.Remove(k);
                }
            }
        }
    }
}