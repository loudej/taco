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
using Sample1;
using Taco;
using Taco.Helpers;

[assembly: Builder("RawApp", typeof(RawApp), "Create")]

namespace Sample1 {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>>>;

    public class RawApp {
        public static AppAction Create() {
            return (env, fault, result) => {
                var request = new Request(env);
                var response = new Response(result) {
                    Status = 200,
                    ContentType = "text/html"
                };

                response.Write("<h1>Sample1</h1>");
                response.Write("<p>Hello world</p>");
                response.Write("<p>This part is being added to the response helper's buffer. The result callback has not been invoked yet.</p>");

                response.Finish(() => {
                    response.Write("<p>The result callback has been invoked. This part is going out live, but you can no longer change http response status or headers.</p>");

                    response.Write("<p>request.PathInfo=").Write(request.PathInfo).Write("</p>");
                    response.Write("<p>request.ScriptName=").Write(request.ScriptName).Write("</p>");

                    response.Write("<p> see also <a href='wilson'>Wilson</a> and <a href='wilsonasync'>WilsonAsync</a></p>");

                    response.Write(new byte[] {65, 66, 67, 68});

                    response.Write("<p>And now for something completely different.");
                    throw new ApplicationException("Something completely different");
                });
            };
        }
    }
}