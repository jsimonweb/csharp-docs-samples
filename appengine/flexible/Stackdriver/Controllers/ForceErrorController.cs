using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stackdriver.ViewModels;
using System.IO;
using Google.Cloud.Storage.V1;
using System.Text;
using Microsoft.Extensions.Options;
using Google;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Stackdriver.Controllers
{
    public class ForceErrorController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // Simulate an exception.
            bool exception = true;
            if (exception)
            {
                throw new Exception("Generic exception for testing Stackdriver Error Reporting");
            }
            return View();
        }
    }
}
