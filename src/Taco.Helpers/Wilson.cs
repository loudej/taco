using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Taco;
using Taco.Helpers;
using Timer = System.Timers.Timer;

[assembly: Builder("Wilson", typeof(Wilson), "App")]
[assembly: Builder("WilsonAsync", typeof(Wilson), "AppAsync")]

namespace Taco.Helpers {
    using FnApp = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<object>>>;

    public class Wilson {
        public static FnApp App() {
            return (env, fault, result) => {
                var request = new Request(env);
                var response = new Response(result);
                var wilson = "left - right\r\n123456789012\r\nhello world!\r\n";

                var href = "?flip=left";
                if (request.GET["flip"] == "left") {
                    wilson = wilson.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(line => new string(line.Reverse().ToArray()))
                        .Aggregate("", (agg, line) => agg + line + Environment.NewLine);
                    href = "?flip=right";
                }

                response.Write("<title>Hutchtastic</title>");
                response.Write("<pre>");
                response.Write(wilson);
                response.Write("</pre>");
                if (request.GET["flip"] == "crash") {
                    throw new ApplicationException("Wilson crashed!");
                }
                response.Write("<p><a href='" + href + "'>flip!</a></p>");
                response.Write("<p><a href='?flip=crash'>crash!</a></p>");
                response.Finish();
            };
        }

        public static FnApp AppAsync() {
            return (env, fault, result) => {
                var request = new Request(env);
                var response = new Response(result);
                var wilson = "left - right\r\n123456789012\r\nhello world!\r\n";

                ThreadPool.QueueUserWorkItem(_ => {
                    try {
                        var href = "?flip=left";
                        if (request.GET["flip"] == "left") {
                            wilson = wilson.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                                .Select(line => new string(line.Reverse().ToArray()))
                                .Aggregate("", (agg, line) => agg + line + Environment.NewLine);
                            href = "?flip=right";
                        }

                        response.Finish((fault2, complete) => TimerLoop(350, fault2,
                            () => response.Write("<title>Hutchtastic</title>"),
                            () => response.Write("<pre>"),
                            () => response.Write(wilson),
                            () => response.Write("</pre>"),
                            () => {
                                if (request.GET["flip"] == "crash") {
                                    throw new ApplicationException("Wilson crashed!");
                                }
                            },
                            () => response.Write("<p><a href='" + href + "'>flip!</a></p>"),
                            () => response.Write("<p><a href='?flip=crash'>crash!</a></p>"),
                            complete));
                    }
                    catch (Exception ex) {
                        fault(ex);
                    }
                });
            };
        }

        static void TimerLoop(double interval, Action<Exception> fault, params Action[] steps) {
            var iter = steps.AsEnumerable().GetEnumerator();
            var timer = new Timer(interval);
            timer.Elapsed += (sender, e) => {
                if (iter.MoveNext()) {
                    try {
                        iter.Current();
                    }
                    catch (Exception ex) {
                        timer.Stop();
                        fault(ex);
                    }
                }
                else {
                    timer.Stop();
                }
            };
            timer.Start();
        }
    }
}