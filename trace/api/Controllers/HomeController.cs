using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

namespace Trace.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //string result = "Trace created.";
            //return Ok(result);
            return View();
        }
    }
}
