﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FluentAsync;
using Taco.Startup;
using Environment = Taco.Startup.Environment;

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
                    var httpRequest = httpContext.Request;
                    var httpResponse = httpContext.Response;
                    var serverVariables = httpRequest.ServerVariables;

                    httpResponse.Buffer = false;

                    var env = serverVariables.AllKeys
                        .ToDictionary(key => key, key => (object)serverVariables[key]);

                    new Environment(env) {
                        Version = new Version(1, 0),
                        UrlScheme = httpRequest.Url.Scheme,
                        Body = new RequestBody(httpRequest.InputStream),
                        Errors = Console.OpenStandardError(),
                        Multithread = true,
                        Multiprocess = false,
                        RunOnce = false,
                        Session = new Session(httpContext.Session),
                        Logger = (eventType, message, exception) => { }, //TODO: any default logger for this host?
                    };


                    var task = Task.Factory.StartNew(_ => {
                        app.InvokeAsync(env)
                            .Then((status, headers, body) => {
                                httpResponse.StatusCode = status;
                                foreach (var header in Split(headers)) {
                                    httpResponse.AppendHeader(header.Key, header.Value);
                                }
                                var writer = new ResponseBody(httpResponse.OutputStream.Write, httpResponse.ContentEncoding);
                                body.ForEach(writer.Write).Then(httpResponse.End);
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

