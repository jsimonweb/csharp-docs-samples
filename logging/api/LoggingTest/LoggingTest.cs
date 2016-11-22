﻿/*
 * Copyright (c) 2016 Google Inc.
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

using System;
using Google.Logging.V2;
using Xunit;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using Grpc.Core;

namespace GoogleCloudSamples
{
    public class BaseTest
    {
        private readonly string _projectId;
        private readonly List<string> _logsToDelete = new List<string>();
        private readonly List<string> _sinksToDelete = new List<string>();

        public BaseTest()
        {
            _projectId = Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID");
        }

        public struct ConsoleOutput
        {
            public int ExitCode;
            public string Stdout;
        };

        /// <summary>Runs LoggingSample.exe with the provided arguments</summary>
        /// <returns>The console output of this program</returns>
        public static ConsoleOutput Run(params string[] arguments)
        {
            Console.Write("LoggingSample.exe ");
            Console.WriteLine(string.Join(" ", arguments));

            using (var output = new StringWriter())
            {
                LoggingSample loggingSample = new LoggingSample(output);
                var consoleOutput = new ConsoleOutput()
                {
                    ExitCode = loggingSample.Run(arguments),
                    Stdout = output.ToString()
                };
                Console.Write(consoleOutput.Stdout);
                return consoleOutput;
            }
        }

        protected static void AssertSucceeded(ConsoleOutput output)
        {
            Assert.True(0 == output.ExitCode,
                $"Exit code: {output.ExitCode}\n{output.Stdout}");
        }

        public class LoggingTest : BaseTest, IDisposable
        {
            public void Dispose()
            {
                try
                {
                    // Delete all logs created from running the tests.
                    foreach (string log in _logsToDelete)
                    {
                        Run("delete-log", log);
                    }
                }
                catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.NotFound) { }
                try
                {
                    // Delete all the log sinks created from running the tests.
                    foreach (string sink in _sinksToDelete)
                    {
                        Run("delete-sink", sink);
                    }
                }
                catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.NotFound) { }
            }

            [Fact]
            public void TestCreateLogEntry()
            {
                string logId = "logForTestCreateLogEntry";
                string message = "Example log entry.";
                _logsToDelete.Add(logId);
                // Try creating a log entry.
                var created = Run("create-log-entry", logId, message);
                AssertSucceeded(created);
                // Pause for 5 seconds before trying to get newly added log entry.
                Thread.Sleep(5000);
                // Retrieve the log entry just added, using the logId as a filter.
                var results = Run("list-log-entries", logId);
                // Confirm returned log entry contains expected value.
                Assert.Contains(message, results.Stdout);
            }

            [Fact]
            public void TestListEntries()
            {
                string logId = "logForTestListEntries";
                string message1 = "Example log entry.";
                string message2 = "Another example log entry.";
                string message3 = "Additional example log entry.";
                _logsToDelete.Add(logId);
                // Try creating three log entries.
                var created1 = Run("create-log-entry", logId, message1);
                AssertSucceeded(created1);
                var created2 = Run("create-log-entry", logId, message2);
                AssertSucceeded(created2);
                var created3 = Run("create-log-entry", logId, message3);
                AssertSucceeded(created3);
                // Pause for 5 seconds before trying to get newly added log entries.
                Thread.Sleep(5000);
                // Retrieve the log entries just added, using the logId as a filter.
                var results = Run("list-log-entries", logId);
                // Confirm returned log entry contains expected value.
                Assert.Contains(message3, results.Stdout);
            }

            [Fact]
            public void TestDeleteLog()
            {
                string logId = "logForTestDeleteLog";
                string message = "Example log entry.";
                //Try creating a log entry
                var created = Run("create-log-entry", logId, message);
                AssertSucceeded(created);
                // Pause for 5 seconds before trying to get newly added log entry.
                Thread.Sleep(5000);
                // Retrieve the log entry just added, using the logId as a filter.
                var results = Run("list-log-entries", logId);
                // Confirm returned log entry contains expected value.
                Assert.Contains(message, results.Stdout);
                // Try deleting log.
                Run("delete-log", logId);
                // Pause for 5 seconds before trying to list logs from deleted log.
                Thread.Sleep(5000);
                // Try listing the log entries.  There should be none.
                var listed = Run("list-log-entries", logId);
                AssertSucceeded(listed);
                Assert.Equal("", listed.Stdout.Trim());
            }

            [Fact]
            public void TestCreateSink()
            {
                string sinkId = "sinkForTestCreateSink";
                string logId = "logForTestCreateSink";
                string sinkName = $"projects/{_projectId}/sinks/{sinkId}";
                string message = "Example log entry.";
                _sinksToDelete.Add(sinkId);
                _logsToDelete.Add(logId);
                // Try creating log with log entry.
                var created1 = Run("create-log-entry", logId, message);
                AssertSucceeded(created1);
                // Try creating sink.
                var created2 = Run("create-sink", sinkId, logId);
                AssertSucceeded(created2);
                var sinkClient = ConfigServiceV2Client.Create();
                var results = sinkClient.GetSink(sinkName);
                // Confirm newly created sink is returned.
                Assert.NotNull(results);
            }

            [Fact]
            public void TestListSinks()
            {
                string sinkId = "sinkForTestListSinks";
                string logId = "logForTestListSinks";
                string sinkName = $"projects/{_projectId}/sinks/{sinkId}";
                string message = "Example log entry.";
                _logsToDelete.Add(logId);
                _sinksToDelete.Add(sinkId);
                // Try creating log with log entry.
                var created1 = Run("create-log-entry", logId, message);
                AssertSucceeded(created1);
                // Try creating sink.
                var created2 = Run("create-sink", sinkId, logId);
                AssertSucceeded(created2);
                // Try listing sinks.
                var results = Run("list-sinks");
                // Confirm list-sinks results are not null.
                Assert.NotNull(results);
            }

            [Fact]
            public void TestUpdateSink()
            {
                string sinkId = "sinkForTestUpdateSink";
                string logId = "logForTestUpdateSink";
                string newLogId = "newlogForTestUpdateSink";
                string sinkName = $"projects/{_projectId}/sinks/{sinkId}";
                string message = "Example log entry.";
                _sinksToDelete.Add(sinkId);
                _logsToDelete.Add(logId);
                _logsToDelete.Add(newLogId);
                // Try creating logs with log entries.
                var created1 = Run("create-log-entry", logId, message);
                AssertSucceeded(created1);
                var created2 = Run("create-log-entry", newLogId, message);
                AssertSucceeded(created2);
                // Try creating sink.
                var created3 = Run("create-sink", sinkId, logId);
                AssertSucceeded(created3);
                // Try updating sink.
                var updated = Run("update-sink", sinkId, newLogId);
                AssertSucceeded(updated);
                // Get sink to confirm that log has been updated.
                var sinkClient = ConfigServiceV2Client.Create();
                var results = sinkClient.GetSink(sinkName);
                var currentLog = results.Filter;
                Assert.Contains(newLogId, currentLog);
            }

            [Fact]
            public void TestDeleteSink()
            {
                string sinkId = "sinkForTestDeleteSink";
                string logId = "logForTestDeleteSink";
                string sinkName = $"projects/{_projectId}/sinks/{sinkId}";
                string message = "Example log entry.";
                _logsToDelete.Add(logId);
                // Try creating log with log entry.
                var created1 = Run("create-log-entry", logId, message);
                AssertSucceeded(created1);
                // Try creating sink.
                var created2 = Run("create-sink", sinkId, logId);
                AssertSucceeded(created2);
                // Try deleting sink.
                Run("delete-sink", sinkId);
                // Get sink to confirm it has been deleted.
                var sinkClient = ConfigServiceV2Client.Create();
                Exception ex = Assert.Throws<Grpc.Core.RpcException>(() =>
                    sinkClient.GetSink(sinkName));
            }

            private string GetConsoleAppOutput(string filePath, string path = "")
            {
                string output = "";
                Process consoleApp = new Process();
                consoleApp.StartInfo.FileName = filePath;
                consoleApp.StartInfo.CreateNoWindow = true;
                consoleApp.StartInfo.UseShellExecute = false;
                consoleApp.StartInfo.RedirectStandardOutput = true;
                consoleApp.StartInfo.Arguments = path;
                consoleApp.Start();
                output = consoleApp.StandardOutput.ReadToEnd();
                if (!consoleApp.HasExited)
                {
                    consoleApp.WaitForExit();
                }
                return output;
            }

            [Fact]
            public void TestLog4NetConsoleApp()
            {
                string output;
                string consoleApp = @"..\..\..\Log4Net\bin\Debug\Log4Net.exe";
                string configFilePath = @"..\..\..\Log4Net\bin\Debug\";
                string expectedOutput = "Log Entry created.";
                // This logId should match the logId value set in Log4Net\log4net.config.xml
                string logId = "YOUR-LOG-ID";
                string message = "Hello World!";
                _logsToDelete.Add(logId);
                output = GetConsoleAppOutput(consoleApp, configFilePath).Trim();
                Assert.Contains(expectedOutput, output);
                // Pause for 5 seconds before trying to get newly added log entry.
                Thread.Sleep(5000);
                // Retrieve the log entry just added, using the logId as a filter.
                var results = Run("list-log-entries", logId);
                // Confirm returned log entry contains expected value.
                Assert.Contains(message, results.Stdout.Trim());
            }

            [Fact]
            public void TestQuickStartConsoleApp()
            {
                string output;
                string filePath = @"..\..\..\QuickStart\bin\Debug\QuickStart.exe";
                string expectedOutput = "Log Entry created.";
                // This logId should match the logId value set in QuickStart\QuickStart.cs
                string logId = "my-log";
                string message = "Hello World!";
                _logsToDelete.Add(logId);
                output = GetConsoleAppOutput(filePath).Trim();
                Assert.Equal(expectedOutput, output);
                // Pause for 5 seconds before trying to get newly added log entry.
                Thread.Sleep(5000);
                // Retrieve the log entry just added, using the logId as a filter.
                var results = Run("list-log-entries", logId);
                // Confirm returned log entry contains expected value.
                Assert.Contains(message, results.Stdout);
            }
        }
    }
}
