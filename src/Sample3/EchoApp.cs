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
using Sample3;
using Taco;
using Taco.Helpers;

[assembly: Builder("Echo", typeof(EchoApp), "App")]

namespace Sample3 {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>>>;

    public class EchoApp {
        public static AppAction App() {
            return (env, fault, result) => {
                var request = new Request(env);
                if (request.RequestMethod == "POST") {
                    result(
                        200,
                        new Dictionary<string, string> {
                            {"Content-Type", "text/plain"},
                            {"Content-Disposition", "attachment; filename=echo.txt;"}
                        },
                        new EchoObservable(request.Body));
                }
                else {
                    new Response(result) {Status = 200, ContentType = "text/html"}
                        .Write("<form method='post' enctype='multipart/form-data'>")
                        .Write("<input type='text' name='Hello'/><br/>")
                        .Write("<input type='file' name='Echo'/><br/>")
                        .Write("<input type='text' name='World'/><br/>")
                        .Write("<input type='submit' value='Go'/>")
                        .Write("</form>")
                        .Finish();
                }
            };
        }

        class EchoObservable : IObservable<Cargo<ArraySegment<byte>>> {
            readonly IObservable<Cargo<ArraySegment<byte>>> _requestObservable;

            public EchoObservable(IObservable<Cargo<ArraySegment<byte>>> requestObservable) {
                _requestObservable = requestObservable;
            }

            public IDisposable Subscribe(IObserver<Cargo<ArraySegment<byte>>> responseObserver) {
                return _requestObservable.Subscribe(new EchoObserver(responseObserver));
            }

            class EchoObserver : IObserver<Cargo<ArraySegment<byte>>> {
                readonly IObserver<Cargo<ArraySegment<byte>>> _responseObserver;

                public EchoObserver(IObserver<Cargo<ArraySegment<byte>>> responseObserver) {
                    _responseObserver = responseObserver;
                }

                public void OnNext(Cargo<ArraySegment<byte>> value) {
                    _responseObserver.OnNext(value);
                }

                public void OnError(Exception error) {
                    _responseObserver.OnError(error);
                }

                public void OnCompleted() {
                    _responseObserver.OnCompleted();
                }
            }
        }
    }
}