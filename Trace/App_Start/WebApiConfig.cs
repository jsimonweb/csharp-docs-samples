using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using System.Web;
using Google.Cloud.Diagnostics.Common;
using Google.Cloud.Diagnostics.AspNet;
using Google.Cloud.Trace.V1;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Web.Http.Routing;

namespace Trace
{
    public static class WebApiConfig
    {
        public class Global : HttpApplication
        {
            public override void Init()
            {
                base.Init();
                string projectId = "YOUR-PROJECT-ID";
                // Trace a sampling of incoming Http requests.
                CloudTrace.Initialize(projectId, this);
            }
        }

        public class HelloWorldHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
    CancellationToken cancellationToken)
            {
                string response = "Done.";

                // Sample: UseTracer
                // Manually trace a specific operation.
                using (CloudTrace.CurrentTracer.StartSpan("hello-world1"))
                {
                    // Pause for a second to simulate work being done.
                    Thread.Sleep(1000);
                    Console.Out.WriteLine("Hello, World!");
                }
                // End sample


                // Sample: UseTracerRunIn
                // Manually trace a specific Action or Func<T>.
                CloudTrace.CurrentTracer.RunInSpan(
                    () => Console.Out.WriteLine("Hello, World!"),
                    "hello-world2");
                // End sample

                return Task.FromResult(new HttpResponseMessage()
                {
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes(response))
                });
            }
        }


        public static void Register(HttpConfiguration config)
        {
            var emptyDictionary = new HttpRouteValueDictionary();
            // Add our one HttpMessageHandler to the root path.
            config.Routes.MapHttpRoute("index", "", emptyDictionary, emptyDictionary,
                new HelloWorldHandler());
        }
    }
}
