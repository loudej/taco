// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.Collections.Generic;
using Taco;
using Taco.Helpers;
using Taco.Helpers.Utils;

[assembly: Builder("ShowExceptions", typeof(ShowExceptions), "Call")]

namespace Taco.Helpers {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<Cargo<object>>>>;

    public class ShowExceptions {
        public static AppAction Call(AppAction app) {
            return (env, fault, result) => {
                Action<Exception, Action<object>> writeErrorPageBody = (ex, write) => {
                    write("<h1>Server Error</h1>");
                    write("<p>");
                    write(ex.Message); //TODO: htmlencode, etc
                    write("</p>");
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