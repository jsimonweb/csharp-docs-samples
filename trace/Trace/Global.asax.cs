using Google.Cloud.Diagnostics.AspNet;
using Google.Cloud.Diagnostics.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Trace
{
    public class WebApiApplication : System.Web.HttpApplication
    {

        public override void Init()
        {
            base.Init();
            TraceConfiguration traceConfig = TraceConfiguration.Create(bufferOptions: BufferOptions.NoBuffer());
            CloudTrace.Initialize(this, "your-project-id", traceConfig);
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}
