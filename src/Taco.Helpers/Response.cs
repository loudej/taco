// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.Collections.Generic;
using Taco.Helpers.Utils;

namespace Taco.Helpers {
    public class Response {
        readonly Action<int, IDictionary<string, string>, IObservable<object>> _result;

        int _status = 200;
        readonly IDictionary<string, string> _headers = new Dictionary<string, string>();
        readonly Body _body = new Body();

        public Response(Action<int, IDictionary<string, string>, IObservable<object>> result) {
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

        class Body : IObservable<object> {
            readonly IList<object> _body = new List<object>();
            Func<Action<Exception>, Action, Action> _block;

            public Body() {
                Write = _body.Add;
            }

            public Action<object> Write;

            public IObservable<object> Attach(Func<Action<Exception>, Action, Action> block) {
                _block = block;
                return this;
            }

            public IDisposable Subscribe(IObserver<object> observer) {
                try {
                    Write = observer.OnNext;
                    foreach (var piece in _body) {
                        observer.OnNext(piece);
                    }
                    return new Disposable(_block(observer.OnError, observer.OnCompleted));
                }
                catch (Exception ex) {
                    observer.OnError(ex);
                    return Disposable.Noop;
                }
            }
        }
    }
}