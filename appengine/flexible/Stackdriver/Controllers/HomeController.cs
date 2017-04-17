/*
 * Copyright (c) 2017 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */

using Microsoft.AspNetCore.Mvc;
using Stackdriver.ViewModels;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Google.Cloud.Storage.V1;
using System.Text;
using Microsoft.Extensions.Options;
using Google;
using Microsoft.Extensions.Logging;
using Google.Cloud.Diagnostics.AspNetCore;

namespace Stackdriver.Controllers
{
    // [START cloud_storage]
    public class HomeController : Controller
    {
        // Contains the bucket name and object name
        readonly StackdriverOptions _options;
        // The Google Cloud Storage client.
        readonly StorageClient _storage;

        public HomeController(IOptions<StackdriverOptions> options)
        {
            _options = options.Value;
            _storage = StorageClient.Create();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new HomeIndex();
            return View(model);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
