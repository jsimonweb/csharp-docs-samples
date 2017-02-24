// Copyright(c) 2017 Google Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License. You may obtain a copy of
// the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
// License for the specific language governing permissions and limitations under
// the License.

using System.IO;
using Xunit;
using System;
using System.Linq;

namespace GoogleCloudSamples
{
    // <summary>
    /// Runs the sample app's methods and tests the outputs
    // </summary>
    public class CommonTests
    {
        private static readonly string s_projectId = Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID");

        readonly CommandLineRunner _cloudMonitoring = new CommandLineRunner()
        {
            VoidMain = Program.Main,
            Command = "Monitoring"
        };

        protected ConsoleOutput Run(params string[] args)
        {
            return _cloudMonitoring.Run(args);
        }

        private readonly RetryRobot _retryRobot = new RetryRobot()
        {
            RetryWhenExceptions = new[] { typeof(Xunit.Sdk.XunitException) }
        };

        /// <summary>
        /// Retry action.
        /// Datastore guarantees only eventual consistency.  Many tests write
        /// an entity and then query it afterward, but may not find it immediately.
        /// </summary>
        /// <param name="action"></param>
        private void Eventually(Action action) => _retryRobot.Eventually(action);

        [Fact(Skip = "Todo")]
        public void TestListMetricDescriptors()
        {
            string timeStamp = $"-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}";
        }

        [Fact(Skip = "Todo")]
        public void TestGetMetricDescriptor()
        {

        }

        [Fact(Skip = "Todo")]
        public void TestCreateCustomMetric()
        {

        }

        [Fact(Skip = "Todo")]
        public void TestWriteTimeSeriesData()
        {

        }

        [Fact(Skip = "Todo")]
        public void TestListMonitoredResourceDescriptors()
        {

        }

        [Fact(Skip = "Todo")]
        public void TestGetMonitoredResourceDescriptor()
        {

        }

        [Fact(Skip = "Todo")]
        public void TestReadTimeSeriesData()
        {

        }

        [Fact(Skip = "Todo")]
        public void TestReadTimeSeriesDataFields()
        {

        }

        [Fact(Skip = "Todo")]
        public void TestReadTimeSeriesDataAggregated()
        {

        }
    }

    public class QuickStartTests
    {
        readonly CommandLineRunner _quickStart = new CommandLineRunner()
        {
            VoidMain = QuickStart.Main,
            Command = "QuickStart"
        };

        [Fact]
        public void TestRun()
        {
            var output = _quickStart.Run();
            Assert.Equal(0, output.ExitCode);
        }
    }
}
