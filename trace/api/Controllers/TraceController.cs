using Google.Cloud.Diagnostics.AspNet;
using Google.Cloud.Diagnostics.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Trace.Controllers
{
    // [START trace_example]
    public class TraceController : Controller
    {
        // This incoming HTTP request should be captured by Trace.
        public ActionResult Index()
        {
            using (CloudTrace.Tracer.StartSpan(nameof(Index)))
            {
                string url = "https://www.googleapis.com/discovery/v1/apis";
                var response = TraceOutgoing(url);
                ViewData["text"] = response.Result.ToString();
                return View();
            }
        }

        public async Task<string> TraceOutgoing(string url)
        {
            // Manually trace a specific operation.
            using (CloudTrace.Tracer.StartSpan("get-api-discovery-doc"))
            {
                using (var httpClient = new HttpClient())
                {
                    // This outgoing HTTP request should be captured by Trace.
                    using (var response = await httpClient.GetAsync(url)
                        .ConfigureAwait(false))
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
        }
    }
    // [END trace_example]
}