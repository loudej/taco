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
        Action<int, IDictionary<string, string>, IObservable<Cargo<object>>>>;

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

        class EchoObservable : IObservable<Cargo<object>> {
            readonly IObservable<Cargo<object>> _requestObservable;

            public EchoObservable(IObservable<Cargo<object>> requestObservable) {
                _requestObservable = requestObservable;
            }

            public IDisposable Subscribe(IObserver<Cargo<object>> responseObserver) {
                return _requestObservable.Subscribe(new EchoObserver(responseObserver));
            }

            class EchoObserver : IObserver<Cargo<object>> {
                readonly IObserver<Cargo<object>> _responseObserver;

                public EchoObserver(IObserver<Cargo<object>> responseObserver) {
                    _responseObserver = responseObserver;
                }

                public void OnNext(Cargo<object> value) {
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