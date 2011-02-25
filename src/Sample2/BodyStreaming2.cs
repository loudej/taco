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
                    new Response(result) {Status = 200, ContentType = "text/html"}
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
                    new Response(result) {Status = 404}.Finish();
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