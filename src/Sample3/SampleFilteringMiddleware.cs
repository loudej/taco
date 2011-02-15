﻿using System;
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
    Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>>>;

    public class SampleFilteringMiddleware {

        public static AppAction Uppercase(AppAction app) {
            return (env, fault, result) =>
                app(env, fault, (status, headers, body) => {
                    if (headers.ContainsKey("Content-Type") && headers["Content-Type"].StartsWith("text/"))
                        result(status, headers, body.Filter(ToUpper));
                    else
                        result(status, headers, body);
                });
        }

        static ArraySegment<byte> ToUpper(ArraySegment<byte> input) {
            return new ArraySegment<byte>(input.Array
                .Skip(input.Offset)
                .Take(input.Count)
                .Select(b => (b >= 'a' && b <= 'z') ? (byte)(b + 'A' - 'a') : b)
                .ToArray());
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

        private static void Write(Cargo<ArraySegment<byte>> cargo, Stream stream) {
            if (!cargo.Delayable) {
                stream.Write(cargo.Result.Array, cargo.Result.Offset, cargo.Result.Count);
                return;
            }

            var result = stream.BeginWrite(cargo.Result.Array, cargo.Result.Offset, cargo.Result.Count, asyncResult => {
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

    }
}