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
    public class TraceController : Controller
    {
        // This incoming HTTP request should be captured by Trace.
        public ActionResult Index()
        {
            IManagedTracer tracer = CloudTrace.Tracer;
            using (tracer.StartSpan(nameof(Index)))
            {
                var traceHeaderHandler = new TraceHeaderPropagatingHandler(() => tracer);
                var response = TraceOutgoing(traceHeaderHandler);
                ViewData["text"] = response.Result.ToString();
                return View();
            }
        }

        // This outgoing HTTP request should be captured by Trace.
        public async Task<string> TraceOutgoing(TraceHeaderPropagatingHandler traceHeaderHandler)
        {
            // Add a handler to trace outgoing requests and to propagate the trace header.
            using (var httpClient = new HttpClient(traceHeaderHandler))
            {
                string url = "https://www.googleapis.com/discovery/v1/apis";
                using (var response = await httpClient.GetAsync(url).ConfigureAwait(false))
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}