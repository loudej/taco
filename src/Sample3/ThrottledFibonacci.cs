// Taco - sample code for consideration by Owin working group
// Louis DeJardin
// For purposes of illustration and exploration only.
// Do not use for production system.
// 
using System;
using System.Collections.Generic;
using System.Text;
using Sample3;
using Taco;
using Taco.Helpers.Utils;

[assembly: Builder("ThrottledFibonacci", typeof(ThrottledFibonacci), "App")]

namespace Sample3 {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>>>;

    public static class EncodingExtensions {
        public static ArraySegment<byte> ToBytes(this string text) {
            return new ArraySegment<byte>(Encoding.Default.GetBytes(text));
        }
    }

    public class ThrottledFibonacci {
        public static AppAction App() {
            return (env, fault, result) => {
                var body = ObservableExtensions.Create<Cargo<ArraySegment<byte>>>((next, error, complete) => {
                    next.InvokeSync("<ul>".ToBytes());
                    next.InvokeSync("<li>0 0</li>".ToBytes());
                    next.InvokeSync("<li>1 1</li>".ToBytes());
                    var n = 2;
                    var fnminus2 = 0.0;
                    var fnminus1 = 1.0;
                    return Loop.Run((halted, continuation) => {
                        while (!halted()) {
                            var fn = fnminus1 + fnminus2;
                            fnminus2 = fnminus1;
                            fnminus1 = fn;
                            n = n + 1;
                            if (next.InvokeAsync(string.Format("<li>{0} {1}</li>", n - 1, fn).ToBytes(), continuation))
                                return;
                        }
                    });
                });
                result(200, new Dictionary<string, string> { { "Content-Type", "text/html" } }, body);
            };
        }
    }
}