// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.Collections.Generic;
using Sample1;
using Taco;
using Taco.Helpers;

[assembly: Builder("RawApp", typeof(RawApp), "Create")]

namespace Sample1 {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<object>>>;

    public class RawApp {
        public static AppAction Create() {
            return (env, fault, result) => {
                var request = new Request(env);
                var response = new Response(result) {
                    Status = 200,
                    ContentType = "text/html"
                };

                response.Write("<h1>Sample1</h1>");
                response.Write("<p>Hello world</p>");
                response.Write("<p>This part is being added to the response helper's buffer. The result callback has not been invoked yet.</p>");

                response.Finish(() => {

                    response.Write("<p>The result callback has been invoked. This part is going out live, but you can no longer change http response status or headers.</p>");

                    response.Write("<p>request.PathInfo=");
                    response.Write(request.PathInfo);
                    response.Write("</p>");

                    response.Write(new byte[] { 65, 66, 67, 68 });

                    response.Write("<p>And now for something completely different.");
                    throw new ApplicationException("Something completely different");
                });
            };
        }
    }
}
