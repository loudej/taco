using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sample3;
using Taco;
using Taco.Helpers.Utils;

[assembly: Builder("ThrottledFibonacci", typeof(ThrottledFibonacci), "App")]
namespace Sample3 {

    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<Cargo<object>>>>;

    public class ThrottledFibonacci {



        public static AppAction App() {
            return (env, fault, result) => {
                var body = ObservableExtensions.Create<Cargo<object>>((next, error, complete) => {
                    next.InvokeSync("<ul>");
                    next.InvokeSync("<li>0 0</li>");
                    next.InvokeSync("<li>1 1</li>");
                    var n = 2;
                    var fnminus2 = 0.0;
                    var fnminus1 = 1.0;
                    return Loop.Run((halted, continuation) => {
                        while (!halted()) {
                            var fn = fnminus1 + fnminus2;
                            fnminus2 = fnminus1;
                            fnminus1 = fn;
                            n = n + 1;
                            if (next.InvokeAsync(string.Format("<li>{0} {1}</li>", n - 1, fn), continuation))
                                return;
                        }
                    });
                });
                result(200, new Dictionary<string, string> { { "Content-Type", "text/html" } }, body);
            };
        }

        class Loop : IDisposable {
            readonly Action<Func<bool>, Action> _action;
            bool _halted;

            Loop(Action<Func<bool>, Action> action) {
                _action = action;
            }

            void Resume() { _action(() => _halted, Resume); }
            void IDisposable.Dispose() { _halted = true; }

            public static IDisposable Run(Action<Func<bool>, Action> action) {
                var loop = new Loop(action);
                loop.Resume();
                return loop;
            }
        }
    }

}