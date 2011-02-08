using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Sample3;
using Taco;
using Taco.Helpers.Utils;

[assembly: Builder("Uppercase", typeof(SampleFilteringMiddleware), "Uppercase")]
[assembly: Builder("Deflate", typeof(SampleFilteringMiddleware), "Deflate")]

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



        public static AppAction Deflate(AppAction app) {
            return (env, fault, result) => {
                if (HasAcceptEncodingDeflate(env)) {
                    app(env, fault, (status, headers, body) => {
                        headers["Content-Encoding"] = "deflate";
                        headers.Remove("Content-Length");
                        result(status, headers, body.Filter((subscribe, next, error, complete) => {
                            var bodyStream = new BodyStream(next, error, complete);
                            var deflateStream = new DeflateStream(bodyStream, CompressionMode.Compress, false);
                            return subscribe(data => Write(data, deflateStream), error, deflateStream.Close);
                        }));
                    });
                }
                else {
                    app(env, fault, result);
                }
            };
        }

        private static bool HasAcceptEncodingDeflate(IDictionary<string, object> env) {
            object acceptEncoding;
            if (env.TryGetValue("HTTP_ACCEPT_ENCODING", out acceptEncoding) &&
                Convert.ToString(acceptEncoding).Split(',').Contains("deflate")) {
                return true;
            }
            return false;
        }

        private static void Write(Cargo<object> cargo, Stream stream) {
            var data = Normalize(cargo.Result);

            if (!cargo.Delayable) {
                stream.Write(data.Array, data.Offset, data.Count);
                return;
            }

            var result = stream.BeginWrite(data.Array, data.Offset, data.Count, asyncResult => {
                if (asyncResult.CompletedSynchronously)
                    return;
                stream.EndWrite(asyncResult);
                cargo.Resume();
            }, null);

            if (result.CompletedSynchronously)
                stream.EndWrite(result);
            else
                cargo.Delay();
        }

        private static ArraySegment<byte> Normalize(object data) {
            if (data is ArraySegment<byte>)
                return (ArraySegment<byte>)data;
            if (data is byte[])
                return new ArraySegment<byte>((byte[])data);

            //todo: have response encoding in environment?
            return new ArraySegment<byte>(Encoding.Default.GetBytes(Convert.ToString(data)));
        }
    }
}