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
using System.Net;
using Sample3;
using Taco;
using Taco.Helpers;

[assembly: Builder("Proxy", typeof(ProxyApp), "App")]

namespace Sample3 {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>>>;

    public class ProxyApp {
        public static AppAction App() {
            return (env, fault, result) => {
                var request = new Request(env);
                var response = new Response(result);
                if (string.IsNullOrWhiteSpace(request.GET["Url"])) {
                    response.ContentType = "text/html";
                    response
                        .Write("<form>")
                        .Write("Url <input type='text' name='Url' style='width:50%;' value='http://download.microsoft.com/download/f/e/6/fe6eb291-e187-4b06-ad78-bb45d066c30f/6.0.6001.18000.367-KRMSDK_EN.iso'/><br/>")
                        .Write("Save as <input type='text' name='SaveAs' style='width:50%;' value='6.0.6001.18000.367-KRMSDK_EN.iso'/><br/>")
                        .Write("<input type='submit' value='Go'/>")
                        .Write("</form>")
                        .Finish();
                }
                else {
                    // make remote request asynchronously
                    var remoteRequest = WebRequest.Create(request.GET["Url"]);

                    remoteRequest.BeginGetResponse(getResponseResult => fault.Guard(() => {
                        var remoteResponse = (HttpWebResponse)remoteRequest.EndGetResponse(getResponseResult);

                        // pass some response headers along
                        response.Status = (int)remoteResponse.StatusCode;
                        response.ContentType = remoteResponse.ContentType;
                        if (!string.IsNullOrWhiteSpace(request.GET["SaveAs"])) {
                            response.AddHeader("Content-Disposition", "attachment; filename=" + request.GET["SaveAs"]);
                        }

                        // pass response body along
                        var remoteStream = remoteResponse.GetResponseStream();
                        if (remoteStream == null) {
                            response.Finish();
                        }
                        else {
                            var buffer = new byte[4096];
                            response.Finish((next, error, complete) =>
                                Loop.Run((halted, continuation) => error.Guard(() =>
                                    remoteStream.BeginRead(buffer, 0, buffer.Length, streamResult => error.Guard(() => {
                                        var count = remoteStream.EndRead(streamResult);
                                        if (halted()) {
                                            return;
                                        }
                                        if (count <= 0) {
                                            complete();
                                            return;
                                        }
                                        if (!next.InvokeAsync(new ArraySegment<byte>(buffer, 0, count), continuation)) {
                                            continuation();
                                        }
                                    }), null))));
                        }
                    }), null);
                }
            };
        }
    }

    static class ErrorExtensions {
        public static void Guard(this Action<Exception> fault, Action action) {
            try {
                action();
            }
            catch (Exception ex) {
                fault(ex);
            }
        }
    }
}