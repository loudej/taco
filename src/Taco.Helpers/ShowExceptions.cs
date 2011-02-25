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
using Taco;
using Taco.Helpers;
using Taco.Helpers.Utils;

[assembly: Builder("ShowExceptions", typeof(ShowExceptions), "Call")]

namespace Taco.Helpers {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>>>;


    static class EncodingExtensions {
        public static ArraySegment<byte> ToBytes(this string text) {
            return new ArraySegment<byte>(Encoding.Default.GetBytes(text));
        }
    }

    public class ShowExceptions {
        public static AppAction Call(AppAction app) {
            return (env, fault, result) => {
                Action<Exception, Action<ArraySegment<byte>>> writeErrorPageBody = (ex, write) => {
                    write("<h1>Server Error</h1>".ToBytes());
                    write("<p>".ToBytes());
                    write(ex.Message.ToBytes()); //TODO: htmlencode, etc
                    write("</p>".ToBytes());
                };

                Action<Exception> sendErrorPageResponse = ex => {
                    var response = new Response(result) {Status = 500, ContentType = "text/html"};
                    writeErrorPageBody(ex, value => response.Write(value));
                    response.Finish();
                };

                try {
                    // intercept app-fault with sendErrorPageResponse, which is the full error page response
                    // intercept body-error with writeErrorPageBody, which adds the error text to the output and completes the response
                    app(env, sendErrorPageResponse, (status, headers, body) =>
                        result(status, headers, body.Filter((subscribe, next, error, complete) =>
                            subscribe(next, ex => {
                                writeErrorPageBody(ex, data => next.InvokeSync(data));
                                complete();
                            }, complete))));
                }
                catch (Exception ex) {
                    sendErrorPageResponse(ex);
                }
            };
        }
    }
}