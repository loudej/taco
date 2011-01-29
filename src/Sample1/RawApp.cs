using System;
using System.Collections.Generic;
using System.IO;
using FluentAsync;
using Taco;
using Taco.Helpers;

[assembly: Builder("RawApp", typeof(Sample1.RawApp), "Create")]

namespace Sample1 {
    using FnApp = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<object>>>;

    public class RawApp {
        public static FnApp Create() {
            return (env, fault, result) => {
                var req = new Request(env);
                req.Body.Aggregate(new MemoryStream(), (stream, next) => {
                    var data = (ArraySegment<byte>)next;
                    stream.Write(data.Array, data.Offset, data.Count);
                    return stream;
                }).Then(stream => {
                    result(404, new Dictionary<string, string> { { "Content-Type", "text/html" } }, null);
                });
            };
        }
    }
}
