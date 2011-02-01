using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Sample2;
using Taco;
using Taco.Helpers;
using Taco.Helpers.Utils;

[assembly: Builder("BodyStreaming", typeof(BodyStreaming), "Create")]

// This example accepts calls directly from the observable

namespace Sample2 {
    using AppAction = Action<
        IDictionary<string, object>,
        Action<Exception>,
        Action<int, IDictionary<string, string>, IObservable<object>>>;

    public class BodyStreaming {
        public static AppAction Create() {
            return (env, fault, result) => {
                var request = new Request(env);

                if (request.RequestMethod == "GET") {
                    new Response(result) { Status = 200, ContentType = "text/html" }
                    .Write(@"
<form method='post'>
    <input type='text' name='hello'/>
    <input type='submit' name='ok' value='ok'/>
</form>")
                    .Finish();
                }
                else if (request.RequestMethod == "POST") {
                    // stream request body
                    var body = new MemoryStream();
                    request.Body.Subscribe(
                        data => {
                            // each data called
                            if (!(data is ArraySegment<byte>)) {
                                throw new ApplicationException("Not actually handling data appropriately");
                            }
                            var segment = (ArraySegment<byte>)data;
                            ((Action<byte[], int, int>)body.Write)(segment.Array, segment.Offset, segment.Count);
                        },
                        fault,
                        () => {
                            // completion continues
                            var response = new Response(result) {
                                Status = 200,
                                ContentType = "text/html"
                            };
                            response.Finish(() => {
                                response.Write("<p>You posted ");
                                response.Write(body.Length);
                                response.Write(" bytes of form data<p>");

                                var form = ParamDictionary.Parse(Encoding.Default.GetString(body.ToArray()));
                                response.Write("<ul>");
                                foreach (var kv in form) {
                                    response.Write("<li>").Write(kv.Key).Write(": ").Write(kv.Value).Write("</li>");
                                }
                                response.Write("</ul>");
                            });
                        });
                }
                else {
                    new Response(result) { Status = 404 }.Finish();
                }
            };
        }
    }
}
