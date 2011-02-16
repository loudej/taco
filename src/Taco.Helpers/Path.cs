using System;
using System.Collections.Generic;
using Taco;
using Taco.Helpers;

[assembly: Builder("Path", typeof(Path), "Create")]

namespace Taco.Helpers {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<Cargo<ArraySegment<byte>>>>>;

    public class Path {
        public static AppAction Create(AppAction app, string path) {
            return (env, fault, result) => {
                var request = new Request(env);
                if (request.PathInfo.StartsWith(path, StringComparison.OrdinalIgnoreCase)) {
                    if (request.PathInfo.Length == path.Length || request.PathInfo[path.Length] == '/') {
                        env["SCRIPT_NAME"] = request.ScriptName + path;
                        env["PATH_INFO"] = request.PathInfo.Substring(path.Length);
                        app(env, fault, result);
                        return;
                    }
                }

                new Response(result) { Status = 404 }.Finish();
            };
        }
    }
}
