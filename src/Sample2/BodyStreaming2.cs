// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAsync;
using Sample2;
using Taco;
using Taco.Helpers;
using Taco.Helpers.Utils;

[assembly: Builder("BodyStreaming2", typeof(BodyStreaming2), "Create")]

// This example uses Task<T> to marshal onto continuation delegates

namespace Sample2 {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>>>;

    public class BodyStreaming2 {
        public static AppAction Create() {
            return (env, fault, result) => {
                var request = new Request(env);

                if (request.RequestMethod == "GET") {
                    new Response(result) { Status = 200, ContentType = "text/html" }
                        .Write(@"
<form method='post'>
    <input type='text' name='hello'/>
    <input type='submit' name='ok' value='ok'/>
</form>")
                        .Finish();
                }
                else if (request.RequestMethod == "POST") {
                    request.ReadAllTaskAsync().ContinueWith(stream => {
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
                    }, fault, () => {
                        /* cancel is noop */
                    });
                }
                else {
                    new Response(result) { Status = 404 }.Finish();
                }
            };
        }
    }

    public static class TaskExtensions {
        public static Task<MemoryStream> ReadAllTaskAsync(this Request request) {
            // Aggregate extension method is a convenient way to 
            // produce a Task<T> for a sequence consumption

            return request.Body.Aggregate(new MemoryStream(), (stream, data) => {
                stream.Write(data.Result.Array, data.Result.Offset, data.Result.Count);
                return stream;
            });
        }

        public static Task ContinueWith<T>(this Task<T> task, Action<T> completed, Action<Exception> faulted, Action canceled) {
            return task.ContinueWith(t => {
                if (t.IsCompleted) {
                    completed(t.Result);
                }
                else if (t.IsFaulted) {
                    faulted(t.Exception);
                }
                else if (t.IsCanceled) {
                    canceled();
                }
            }, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}