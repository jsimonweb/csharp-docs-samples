using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SessionState.Models;

namespace SessionState.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public ActionResult S(string id = null)
        {
            if (id == null)
            {
                ViewBag.Keys = HttpContext.Session.Keys;
                return View();
            }
            return Content(HttpContext.Session.GetString(id));
        }

        [HttpPost]
        public IActionResult S(Models.SessionVariable svar)
        {
            HttpContext.Session.SetString(svar.Key, svar.Value);
            ViewBag.Keys = HttpContext.Session.Keys;
            if (svar.Silent.HasValue && (bool)svar.Silent)
                return new EmptyResult();
            return View();
        }
    }
}
