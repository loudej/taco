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
using Taco.Helpers.Utils;

namespace Taco.Helpers {
    public class Response {
        readonly Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>> _result;

        int _status = 200;
        readonly IDictionary<string, string> _headers = new Dictionary<string, string>();
        readonly Body _body = new Body();

        public Response(Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>> result) {
            _result = result;
        }

        public int Status {
            get { return _status; }
            set { _status = value; }
        }

        public string ContentType {
            get { return GetHeader("Content-Type"); }
            set { SetHeader("Content-Type", value); }
        }

        string GetHeader(string name) {
            string value;
            return _headers.TryGetValue(name, out value) ? value.Replace("\r\n", ",") : null;
        }

        public void SetHeader(string name, string value) {
            _headers.Remove(name);
            AddHeader(name, value);
        }

        public void AddHeader(string name, string value) {
            var sanitized = (value ?? "").Replace("\r", "").Replace("\n", "");
            if (string.IsNullOrWhiteSpace(sanitized))
                return;

            string existing;
            _headers[name] = _headers.TryGetValue(name, out existing) ? existing + "\r\n" + sanitized : sanitized;
        }

        public Response Write(object data) {
            _body.Write(data);
            return this;
        }


        public void Finish() {
            _result(Status, _headers, _body.Attach((fault, complete) => {
                complete();
                return () => { };
            }));
        }

        public void Finish(Action<Action<Exception>, Action> block) {
            _result(Status, _headers, _body.Attach((fault, complete) => {
                block(fault, complete);
                return () => { };
            }));
        }

        public void Finish(Action block) {
            _result(Status, _headers, _body.Attach((fault, complete) => {
                block();
                complete();
                return () => { };
            }));
        }

        public void Finish(Func<Action<Exception>, Action, Action> block) {
            _result(Status, _headers, _body.Attach(block));
        }

        public void Finish(IObservable<Cargo<ArraySegment<byte>>> body) {
            _result(Status, _headers, _body.Attach(body.Subscribe));
        }

        public void Finish(Func<Action<Cargo<ArraySegment<byte>>>, Action<Exception>, Action, IDisposable> subscribe) {
            _result(Status, _headers, _body.Attach(subscribe));
        }

        class Body : IObservable<Cargo<ArraySegment<byte>>> {
            readonly IList<object> _body = new List<object>();
            Func<Action<Cargo<ArraySegment<byte>>>, Action<Exception>, Action, IDisposable> _subscribe;

            public Body() {
                Write = _body.Add;
            }

            public Action<object> Write;


            public IObservable<Cargo<ArraySegment<byte>>> Attach(Func<Action<Exception>, Action, Action> block) {
                return Attach((next, fault, complete) => new Disposable(block(fault, complete)));
            }

            public IObservable<Cargo<ArraySegment<byte>>> Attach(Func<Action<Cargo<ArraySegment<byte>>>, Action<Exception>, Action, IDisposable> subscribe) {
                _subscribe = subscribe;
                return this;
            }

            public IDisposable Subscribe(IObserver<Cargo<ArraySegment<byte>>> observer) {
                try {
                    Write = data => observer.OnNext(Cargo.From(Normalize(data)));
                    foreach (var piece in _body) {
                        observer.OnNext(Cargo.From(Normalize(piece)));
                    }
                    return _subscribe(observer.OnNext, observer.OnError, observer.OnCompleted);
                }
                catch (Exception ex) {
                    observer.OnError(ex);
                    return Disposable.Noop;
                }
            }

            ArraySegment<byte> Normalize(object data) {
                if (data is ArraySegment<byte>)
                    return (ArraySegment<byte>)data;
                if (data is byte[])
                    return new ArraySegment<byte>((byte[])data);

                //todo: have response encoding in environment?
                return new ArraySegment<byte>(Encoding.Default.GetBytes(Convert.ToString(data)));
            }
        }
    }
}