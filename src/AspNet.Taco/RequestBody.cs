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
using System.IO;
using Taco;

namespace AspNet.Taco {
    class RequestBody : IObservable<Cargo<ArraySegment<byte>>> {
        readonly Stream _stream;

        public RequestBody(Stream stream) {
            _stream = stream;
        }

        public IDisposable Subscribe(IObserver<Cargo<ArraySegment<byte>>> observer) {
            Action<Exception> error = observer.OnError;
            var buffer = new byte[4096];
            return Loop.Run((halted, continuation) => error.Guard(() =>
                _stream.BeginRead(buffer, 0, buffer.Length, ar => error.Guard(() => {
                    var count = _stream.EndRead(ar);
                    if (halted()) {
                        return;
                    }
                    if (count == 0) {
                        observer.OnCompleted();
                    }
                    else {
                        if (!observer.OnNextAsync(new ArraySegment<byte>(buffer, 0, count), continuation))
                            continuation();
                    }
                }), null)));
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