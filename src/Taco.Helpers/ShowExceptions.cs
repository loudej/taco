using System;
using System.Collections.Generic;
using Taco;
using Taco.Helpers;

[assembly: Builder("ShowExceptions", typeof(ShowExceptions), "Call")]

namespace Taco.Helpers {
    using FnApp = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<object>>>;

    public class ShowExceptions {
        public static FnApp Call(FnApp app) {
            return (env, fault, result) => {
                Action<Exception> errorPage = ex => {
                    var response = new Response(result) { Status = 500 };
                    response.SetHeader("Content-Type", "text/html");
                    response.Finish(() => response.Write("<h1>Server Error</h1>"));
                };

                try {
                    app(env, errorPage, (status, headers, body) => {
                        // todo: shim body to show exceptions while streaming
                        result(status, headers, body);
                    });
                }
                catch (Exception ex) {
                    errorPage(ex);
                }
            };
        }
    }
}
