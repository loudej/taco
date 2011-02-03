using System;
using System.Collections.Generic;
using System.Net;
using Sample3;
using Taco;
using Taco.Helpers;
using Taco.Helpers.Utils;

[assembly: Builder("Proxy", typeof(ProxyApp), "App")]

namespace Sample3 {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<Cargo<object>>>>;

    public class ProxyApp {
        public static AppAction App() {
            return (env, fault, result) => {
                var request = new Request(env);
                var response = new Response(result);
                if (string.IsNullOrWhiteSpace(request.GET["Url"])) {
                    response.ContentType = "text/html";
                    response
                       .Write("<form>")
                       .Write("Url <input type='text' name='Url'/><br/>")
                       .Write("Save as <input type='text' name='SaveAs' value='Data.txt'/><br/>")
                       .Write("<input type='submit' value='Go'/>")
                       .Write("</form>")
                       .Finish();
                }
                else {
                    // make remote request asynchronously
                    var remoteRequest = WebRequest.Create(request.GET["Url"]);

                    remoteRequest.BeginGetResponse(getResponseResult => {
                        try {
                            var remoteResponse = (HttpWebResponse)remoteRequest.EndGetResponse(getResponseResult);

                            // pass some response headers along
                            response.Status = (int)remoteResponse.StatusCode;
                            response.ContentType = remoteResponse.ContentType;
                            if (!string.IsNullOrWhiteSpace(request.GET["SaveAs"])) {
                                response.AddHeader("Content-Disposition", "attachment; filename=" + request.GET["SaveAs"]);
                            }

                            // pass response body along
                            var remoteStream = remoteResponse.GetResponseStream();
                            if (remoteStream == null) {
                                response.Finish();
                            }
                            else {
                                response.Finish((next, error, complete) => {
                                    var buffer = new byte[4096];
                                    return Loop.Run((halted, continuation) => {
                                        try {
                                            remoteStream.BeginRead(buffer, 0, buffer.Length, streamResult => {
                                                try {
                                                    var count = remoteStream.EndRead(streamResult);
                                                    if (count <= 0) {
                                                        complete();
                                                        return;
                                                    }
                                                    if (!next.InvokeAsync(new ArraySegment<byte>(buffer, 0, count), continuation)) {
                                                        continuation();
                                                    }
                                                }
                                                catch (Exception ex) {
                                                    error(ex);
                                                }
                                            }, null);
                                        }
                                        catch (Exception ex) {
                                            error(ex);
                                        }
                                    });
                                });
                            }
                        }
                        catch (Exception ex) {
                            fault(ex);
                        }
                    }, null);
                }
            };
        }
    }
}