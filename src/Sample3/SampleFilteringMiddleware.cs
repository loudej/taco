using System;
using System.Collections.Generic;
using System.Linq;
using Sample3;
using Taco;
using Taco.Helpers.Utils;

[assembly: Builder("Uppercase", typeof(SampleFilteringMiddleware), "Uppercase")]

namespace Sample3 {
    using AppAction = Action<
    IDictionary<string, object>,
    Action<Exception>,
    Action<int, IDictionary<string, string>, IObservable<Cargo<object>>>>;

    public class SampleFilteringMiddleware {

        public static AppAction Uppercase(AppAction app) {
            return (env, fault, result) =>
                app(env, fault, (status, headers, body) => {
                    if (headers.ContainsKey("Content-Type") && headers["Content-Type"].StartsWith("text/"))
                        result(status, headers, body.Filter(data => ToUpper(data)));
                    else
                        result(status, headers, body);
                });
        }

        static object ToUpper(object result) {
            if (result is string)
                return ((string)result).ToUpperInvariant();

            if (result is ArraySegment<byte>)
                return ToUpper((ArraySegment<byte>)result);

            if (result is byte[])
                return ToUpper(new ArraySegment<byte>((byte[])result, 0, ((byte[])result).Length));

            return result;
        }

        static byte[] ToUpper(ArraySegment<byte> input) {
            return Enumerable.Range(0, input.Count)
                .Select(index => input.Array[input.Offset + index])
                .Select(b => (b >= 'a' && b <= 'z') ? (byte)(b + 'A' - 'a') : b)
                .ToArray();
        }
    }
}