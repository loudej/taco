
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FluentAsync;
using Taco.Startup;

namespace AspNet.Taco {

    public class AspNetTacoModule : IHttpModule {
        void IHttpModule.Init(HttpApplication context) {

            var applicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var lines = File.ReadLines(Path.Combine(applicationBase, "config.taco"));

            var builder = new Builder();
            builder.ParseLines(lines);
            var app = builder.ToApp();

            context.AddOnBeginRequestAsync(
                (sender, e, callback, state) => {
                    var httpApplication = (HttpApplication)sender;
                    var httpContext = httpApplication.Context;
                    httpContext.Response.Buffer = false;

                    var env = new Dictionary<string, object>();
                    foreach (string key in httpContext.Request.ServerVariables) {
                        env[key] = httpContext.Request.ServerVariables[key];
                    }

                    env["taco.input"] = new BodyReader(httpContext.Request.InputStream);


                    var task = Task.Factory.StartNew(_ => {
                        app.InvokeAsync(env)
                            .Then((status, headers, body) => {
                                httpContext.Response.StatusCode = status;
                                foreach (var header in Split(headers)) {
                                    httpContext.Response.AppendHeader(header.Key, header.Value);
                                }
                                var writer = new BodyWriter(context.Response.OutputStream.Write, context.Response.ContentEncoding);
                                body.ForEach(writer.Write).Then(context.Response.End);
                            });
                    }, state, TaskCreationOptions.PreferFairness);

                    if (callback != null)
                        task.Finally(() => callback(task));

                    return task;
                },
                ar => ((Task)ar).Wait());
        }

        static IEnumerable<KeyValuePair<string, string>> Split(IEnumerable<KeyValuePair<string, string>> headers) {
            return headers.SelectMany(
                kv => kv.Value
                          .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(v => new KeyValuePair<string, string>(kv.Key, v)));
        }

        void IHttpModule.Dispose() {
        }
    }
}

