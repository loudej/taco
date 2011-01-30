using System;
using System.Collections.Generic;
using Taco.Helpers.Utils;

namespace Taco.Helpers {
    public class Response {
        private readonly Action<int, IDictionary<string, string>, IObservable<object>> _result;

        private int _status = 200;
        private readonly IDictionary<string, string> _headers = new Dictionary<string, string>();
        private readonly Body _body = new Body();

        public Response(Action<int, IDictionary<string, string>, IObservable<object>> result) {
            _result = result;
        }

        public int Status {
            get { return _status; }
            set { _status = value; }
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

        public void Write(object data) {
            _body.Write(data);
        }


        public void Finish() {
            _result(Status, _headers, _body.Attach((fault, complete) => { complete(); return () => { }; }));
        }

        public void Finish(Action<Action<Exception>, Action> block) {
            _result(Status, _headers, _body.Attach((fault, complete) => { block(fault, complete); return () => { }; }));
        }

        public void Finish(Action block) {
            _result(Status, _headers, _body.Attach((fault, complete) => { block(); complete(); return () => { }; }));
        }

        public void Finish(Func<Action<Exception>, Action, Action> block) {
            _result(Status, _headers, _body.Attach(block));
        }

        class Body : IObservable<object> {
            private readonly IList<object> _body = new List<object>();
            private Func<Action<Exception>, Action, Action> _block;

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

