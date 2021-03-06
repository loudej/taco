﻿// Licensed to .NET HTTP Abstractions (the "Project") under one
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
using System.IO;
using System.Net;
using Sample3;
using Taco;
using Taco.Helpers;

[assembly: Builder("Proxy2", typeof(ProxyApp2), "App")]

namespace Sample3 {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>>>;

    /// <summary>
    /// This is nearly the same as ProxyApp, but the single method is unrolled 
    /// </summary>
    public class ProxyApp2 {
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
                    GetResponse(remoteRequest, fault, remoteResponse => {
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
                            response.Finish(new ProxyBody(remoteStream));
                        }
                    });
                }
            };
        }

        static void GetResponse(WebRequest request, Action<Exception> fault, Action<HttpWebResponse> callback) {
            try {
                request.BeginGetResponse(result => {
                    try {
                        callback(request.EndGetResponse(result) as HttpWebResponse);
                    }
                    catch (Exception ex) {
                        fault(ex);
                    }
                }, null);
            }
            catch (Exception ex) {
                fault(ex);
            }
        }

        class ProxyBody : IObservable<Cargo<ArraySegment<byte>>> {
            readonly Stream _remoteStream;

            public ProxyBody(Stream remoteStream) {
                _remoteStream = remoteStream;
            }

            public IDisposable Subscribe(IObserver<Cargo<ArraySegment<byte>>> observer) {
                var pump = new Pump(_remoteStream, observer);
                pump.StartRead();
                return pump;
            }

            class Pump : IDisposable {
                readonly Stream _remoteStream;
                readonly IObserver<Cargo<ArraySegment<byte>>> _observer;
                readonly byte[] _buffer;
                bool _halted;

                public Pump(Stream remoteStream, IObserver<Cargo<ArraySegment<byte>>> observer) {
                    _buffer = new byte[4096];
                    _remoteStream = remoteStream;
                    _observer = observer;
                }

                public void Dispose() {
                    _halted = true;
                }

                public void StartRead() {
                    if (_halted) return;
                    try {
                        ReadCallback(true, _remoteStream.BeginRead(_buffer, 0, _buffer.Length, ar => ReadCallback(false, ar), null));
                    }
                    catch (Exception ex) {
                        _observer.OnError(ex);
                    }
                }

                void ReadCallback(bool synchronously, IAsyncResult ar) {
                    if (synchronously != ar.CompletedSynchronously)
                        return;

                    try {
                        var count = _remoteStream.EndRead(ar);
                        if (count <= 0) {
                            _observer.OnCompleted();
                            return;
                        }
                        StartNext(new ArraySegment<byte>(_buffer, 0, count));
                    }
                    catch (Exception ex) {
                        _observer.OnError(ex);
                    }
                }

                void StartNext(ArraySegment<byte> segment) {
                    try {
                        if (_halted) return;
                        var cargo = Cargo.From(segment, NextCallback);
                        _observer.OnNext(cargo);
                        if (!cargo.Delayed)
                            NextCallback();
                    }
                    catch (Exception ex) {
                        _observer.OnError(ex);
                    }
                }

                void NextCallback() {
                    try {
                        StartRead();
                    }
                    catch (Exception ex) {
                        _observer.OnError(ex);
                    }
                }
            }
        }
    }
}