// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAsync;
using Sample2;
using Taco;
using Taco.Helpers;
using Taco.Helpers.Utils;

[assembly: Builder("BodyStreaming3", typeof(BodyStreaming3), "Create")]

// This example uses something like the await keyword

namespace Sample2 {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<Cargo<object>>>>;

    /// <summary>
    /// Approximate emulation of await keyword 
    /// </summary>
    public static class Await<T1> {
        public static Action<T1, Action<T3>> Construct<T2, T3>(
            Func<T1, T2> getAwaiter,
            Func<T2, Action, bool> beginAwait,
            Func<T2, T3> endAwait) {
            return (awaitable, continuation) => {
                var awaiter = getAwaiter(awaitable);
                if (beginAwait(awaiter, () => continuation(endAwait(awaiter))))
                    return;
                continuation(endAwait(awaiter));
            };
        }
    }

    public class BodyStreaming3 {
        public static AppAction Create() {
            // The expression t of an await-expression await t is called the task of the await expression. The task t is required to be awaitable, which means that all of the following must hold:
            // * (t).GetAwaiter() is a valid expression of type A. 
            // * Given an expression a of type A and an expression r of type System.Action, (a).BeginAwait(r) is a valid boolean-expression. 
            // * Given an expression a of type A, (a).EndAwait() is a valid expression. 

            var awaitRequest = Await<Request>.Construct(
                t => t.GetAwaiter(),
                (a, r) => a.BeginAwait(r),
                a => a.EndAwait());


            return (env, fault, result) => {
                var request = new Request(env);

                if (request.RequestMethod == "GET") {
                    new Response(result) {Status = 200, ContentType = "text/html"}
                        .Write(@"
<form method='post'>
    <input type='text' name='hello'/>
    <input type='submit' name='ok' value='ok'/>
</form>")
                        .Finish();
                }
                else if (request.RequestMethod == "POST") {
                    // to the best of my knowledge this is the pre-c#5 form of await keyword
                    // not sure how error handling integrates

                    awaitRequest(request, stream => {
                        try {
                            // the the rest of this is the same as the first example
                            var response = new Response(result) {
                                Status = 200,
                                ContentType = "text/html"
                            };
                            response.Finish(() => {
                                response.Write("<p>You posted ");
                                response.Write(stream.Length);
                                response.Write(" bytes of form data<p>");

                                var form = ParamDictionary.Parse(Encoding.Default.GetString(stream.ToArray()));
                                response.Write("<ul>");
                                foreach (var kv in form) {
                                    response.Write("<li>")
                                        .Write(kv.Key)
                                        .Write(": ")
                                        .Write(kv.Value)
                                        .Write("</li>");
                                }
                                response.Write("</ul>");
                            });
                        }
                        catch (Exception ex) {
                            fault(ex);
                        }
                    });
                }
                else {
                    new Response(result) {Status = 404}.Finish();
                }
            };
        }
    }

    /// <summary>
    /// Adding extension methods to make Request awaitable as a MemoryStream.
    /// This particular implementation punts the actual asynchronisity. It's actually 
    /// an example of marshalling async events onto a synchronous continuation.
    /// </summary>
    public static class RequestExtensions {
        public static Awaiter GetAwaiter(this Request request) {
            return new Awaiter {Request = request, MemoryStream = new MemoryStream()};
        }

        public static bool BeginAwait(this Awaiter awaiter, Action resumption) {
            awaiter.Request.Body.ForEach(data => {
                if (!(data.Result is ArraySegment<byte>)) {
                    throw new ApplicationException(
                        "Not actually handling data appropriately");
                }
                var segment = (ArraySegment<byte>)data.Result;
                ((Action<byte[], int, int>)awaiter.MemoryStream.Write)(segment.Array, segment.Offset,
                    segment.Count);
            }).Wait();
            return false;
        }

        public static MemoryStream EndAwait(this Awaiter awaiter) {
            return awaiter.MemoryStream;
        }

        public class Awaiter {
            public Request Request { get; set; }
            public MemoryStream MemoryStream { get; set; }
        }
    }
}