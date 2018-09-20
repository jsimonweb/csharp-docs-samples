/*
 * Copyright (c) 2018 Google Inc.
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Linq;
using Google.Cloud.Spanner.Data;
using CommandLine;
using log4net;
using System.IO;

namespace GoogleCloudSamples.Spanner
{
    [Verb("createPlanetsDatabase", HelpText = "Create a sample Cloud Spanner database along with sample tables in your project.")]
    class CreatePlanetsDatabaseOptions
    {
        [Value(0, HelpText = "The project ID of the project to use when creating Cloud Spanner resources.", Required = true)]
        public string projectId { get; set; }
        [Value(1, HelpText = "The ID of the instance where the sample database will be created.", Required = true)]
        public string instanceId { get; set; }
        [Value(2, HelpText = "The ID of the sample database to create.", Required = true)]
        public string databaseId { get; set; }
    }

    [Verb("insertPlanet", HelpText = "Insert a planet into a Cloud Spanner database.")]
    class InsertPlanetOptions
    {
        [Value(0, HelpText = "The project ID of the project to use when creating Cloud Spanner resources.", Required = true)]
        public string projectId { get; set; }
        [Value(1, HelpText = "The ID of the instance where the sample database will be created.", Required = true)]
        public string instanceId { get; set; }
        [Value(2, HelpText = "The ID of the sample database to create.", Required = true)]
        public string databaseId { get; set; }
        [Value(3, HelpText = "The name of the planet to insert.", Required = true)]
        public string planetName { get; set; }
        [Value(4, HelpText = "The value of the planet to insert.", Required = true)]
        public long planetValue { get; set; }
    }

    [Verb("batchInsertPlanets", HelpText = "Batch insert planets into a Cloud Spanner database.")]
    class BatchInsertPlanetsOptions
    {
        [Value(0, HelpText = "The project ID of the project to use when creating Cloud Spanner resources.", Required = true)]
        public string projectId { get; set; }
        [Value(1, HelpText = "The ID of the instance where the sample database will be created.", Required = true)]
        public string instanceId { get; set; }
        [Value(2, HelpText = "The ID of the sample database to create.", Required = true)]
        public string databaseId { get; set; }
        [Value(3, HelpText = "The path to the csv to be imported, containing PlanetName, PlanetValue on each line.", Required = true)]
        public string csvFile { get; set; }
    }

    [Verb("batchInsertPlayers", HelpText = "Batch insert players into a Cloud Spanner database.")]
    class BatchInsertPlayersOptions
    {
        [Value(0, HelpText = "The project ID of the project to use when using Cloud Spanner resources.", Required = true)]
        public string projectId { get; set; }
        [Value(1, HelpText = "The ID of the instance where the sample players will be inserted.", Required = true)]
        public string instanceId { get; set; }
        [Value(2, HelpText = "The ID of the sample database where the sample players will be inserted.", Required = true)]
        public string databaseId { get; set; }
    }

    [Verb("runPlanetAuction", HelpText = "Initiate automated player purchase of planet shares in a Cloud Spanner database.")]
    class RunPlanetAuctionOptions
    {
        [Value(0, HelpText = "The project ID of the project to use when using Cloud Spanner resources.", Required = true)]
        public string projectId { get; set; }
        [Value(1, HelpText = "The ID of the instance to use for running the auction.", Required = true)]
        public string instanceId { get; set; }
        [Value(2, HelpText = "The ID of the sample database to use for running the auction.", Required = true)]
        public string databaseId { get; set; }
        [Value(3, HelpText = "The number of shares to be purchased in the auction.", Required = true)]
        public int numberOfShares { get; set; }
        [Value(4, Default = false, HelpText = "Set to 'false' to hide console output.")]
        public bool showConsoleOutput { get; set; }
    }

    // [START spanner_retry_strategy]
    public class RetryRobot
    {
        public TimeSpan FirstRetryDelay { get; set; } = TimeSpan.FromSeconds(1000);
        public float DelayMultiplier { get; set; } = 2;
        public int MaxTryCount { get; set; } = 7;
        public Func<Exception, bool> ShouldRetry { get; set; }

        /// <summary>
        /// Retry action when assertion fails.
        /// </summary>
        /// <param name="func"></param>
        public T Eventually<T>(Func<T> func)
        {
            TimeSpan delay = FirstRetryDelay;
            for (int i = 0; ; ++i)
            {
                try
                {
                    return func();
                }
                catch (Exception e)
                when (ShouldCatch(e) && i < MaxTryCount)
                {
                    Thread.Sleep(delay);
                    delay *= (int)DelayMultiplier;
                }
            }
        }
        private bool ShouldCatch(Exception e)
        {
            return ShouldRetry != null && ShouldRetry(e);
        }
    }
    // [END spanner_retry_strategy]

    public class Program
    {
        static readonly ILog s_logger = LogManager.GetLogger(typeof(Program));
        private static long _failedTransactions = 0;

        enum ExitCode : int
        {
            Success = 0,
            InvalidParameter = 1,
        }

        public class PlanetShareTransaction
        {
            public long planetId { get; set; }
            public string playerId { get; set; }
            public long availableShares { get; set; }
            public long planetDollars { get; set; }
            public long costPerShare { get; set; }
        }

        public static async Task CreatePlanetsDatabaseAsync(
            string projectId, string instanceId, string databaseId)
        {
            // [START spanner_create_database]
            // Initialize request connection string for database creation.
            string connectionString =
                $"Data Source=projects/{projectId}/instances/{instanceId}";
            //Make the request.
            using (var connection = new SpannerConnection(connectionString))
            {
                string createStatement = $"CREATE DATABASE `{databaseId}`";
                var cmd = connection.CreateDdlCommand(createStatement);
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (SpannerException e) when (e.ErrorCode == ErrorCode.AlreadyExists)
                {
                    // OK.
                }
            }
            // Update connection string with Database ID for table creation.
            connectionString = connectionString + $"/databases/{databaseId}";
            using (var connection = new SpannerConnection(connectionString))
            {
                // Define create table statement for table #1.
                string createTableStatement =
               @"CREATE TABLE Planets (
                     PlanetId INT64 NOT NULL,
                     PlanetName  STRING(1024),
                     PlanetValue INT64,
                     SharesAvailable INT64
                 ) PRIMARY KEY (PlanetId)";
                // Make the request.
                var cmd = connection.CreateDdlCommand(createTableStatement);
                await cmd.ExecuteNonQueryAsync();
                // Define create table statement for table #2.
                createTableStatement =
               @"CREATE TABLE Players (
                     PlayerId STRING(MAX) NOT NULL,
                     PlayerName STRING(1024),
                     PlanetDollars INT64
                 ) PRIMARY KEY (PlayerId)";
                // Make the request.
                cmd = connection.CreateDdlCommand(createTableStatement);
                await cmd.ExecuteNonQueryAsync();
                // Define create table statement for table #3.
                createTableStatement =
                @"CREATE TABLE Transactions (
                     PlanetId INT64 NOT NULL,
                     PlayerId STRING(MAX) NOT NULL,
                     Amount INT64,
                     TimeStamp TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
                 ) PRIMARY KEY (PlanetId, PlayerId, TimeStamp)";
                // Make the request.
                cmd = connection.CreateDdlCommand(createTableStatement);
                await cmd.ExecuteNonQueryAsync();
            }
            // [END spanner_create_database]
        }

        public static async Task InsertPlanetAsync(
            string projectId, string instanceId, string databaseId, string planetName,
            long planetValue)
        {
             // Insert Planet Code
            string connectionString =
                $"Data Source=projects/{projectId}/instances/{instanceId}"
                + $"/databases/{databaseId}";

            long planetStartingAvailableShares = 100000000000;
            using (var connection = new SpannerConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var tx = await connection.BeginTransactionAsync())
                {
                    using (var cmd = connection.CreateInsertCommand(
                        "Planets", new SpannerParameterCollection
                    {
                        { "PlanetId", SpannerDbType.String },
                        { "PlanetName", SpannerDbType.String },
                        { "PlanetValue", SpannerDbType.Int64 },
                        { "SharesAvailable", SpannerDbType.Int64 }
                    }))
                    {
                        cmd.Transaction = tx;
                        cmd.Parameters["PlanetId"].Value =
                            Math.Abs(Guid.NewGuid().GetHashCode());
                        cmd.Parameters["PlanetName"].Value = planetName;
                        cmd.Parameters["PlanetValue"].Value = planetValue;
                        cmd.Parameters["SharesAvailable"].Value = planetStartingAvailableShares;
                        cmd.ExecuteNonQuery();
                    }
                    await tx.CommitAsync();
                }
            }
        }

        public static async Task BatchInsertPlanetsAsync(
            string projectId, string instanceId, string databaseId,  string csvFile)
        {
            //string fileToImport = "exoplanets-with-values-import2.csv";
            string fileToImport = csvFile;

            StreamReader sr = new StreamReader(fileToImport);
            // for set encoding
            // StreamReader sr = new StreamReader(@"file.csv", Encoding.GetEncoding(1250));

            string strline = "";
            string[] _values = null;
            int x = 0;
            while (!sr.EndOfStream)
            {
                x++;
                strline = sr.ReadLine();
                _values = strline.Split(',');
                await InsertPlanetAsync(projectId, instanceId, databaseId,
                    _values[0], Convert.ToInt64(_values[1]));
            }
            sr.Close();
        }

        private static async Task BatchInsertPlayersAsync(string projectId,
            string instanceId, string databaseId)
        {
            string connectionString =
                $"Data Source=projects/{projectId}/instances/{instanceId}"
                + $"/databases/{databaseId}";

            long playerStartingPlanetDollars = 1000000;

            // Batch insert 249,900 player records into the Players table.
            using (var connection = new SpannerConnection(connectionString))
            {
                await connection.OpenAsync();

                for (int i = 0; i < 100; i++)
                {
                    // For details on transaction isolation, see the "Isolation" section in:
                    // https://cloud.google.com/spanner/docs/transactions#read-write_transactions
                    using (var tx = await connection.BeginTransactionAsync())
                    using (var cmd = connection.CreateInsertCommand("Players", new SpannerParameterCollection
                    {
                        { "PlayerId", SpannerDbType.String },
                        { "PlayerName", SpannerDbType.String },
                        { "PlanetDollars", SpannerDbType.Int64 }
                    }))
                    {
                        cmd.Transaction = tx;
                        for (var x = 1; x < 2500; x++)
                        {
                            string nameSuffix = Guid.NewGuid().ToString().Substring(0, 8);
                            cmd.Parameters["PlayerId"].Value = Guid.NewGuid().ToString("N");
                            cmd.Parameters["PlayerName"].Value = $"Player-{nameSuffix}";
                            cmd.Parameters["PlanetDollars"].Value = playerStartingPlanetDollars;
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (SpannerException ex) {
                                Console.WriteLine($"Spanner Exception: {ex.Message}");
                                // Decrement x and retry
                                x--;
                                continue;
                            }
                        }
                        await tx.CommitAsync();
                    }
                }
            }
            Console.WriteLine("Done inserting sample records...");
        }

        public static object RunPlanetAuction(string projectId,
            string instanceId, string databaseId, int numberOfShares, bool showConsoleOutput)
        {

            Parallel.For(0, numberOfShares, i =>
            {
                try {
                    var response = Task.Run(async () =>
                    {
                        var retryRobot = new RetryRobot { MaxTryCount = 0, DelayMultiplier = 2, ShouldRetry = (e) => e.IsTransientSpannerFault() };
                        await retryRobot.Eventually(() =>
                                RunPlanetAuctionAsync(projectId, instanceId, databaseId, showConsoleOutput));
                    });
                    response.Wait();
                }
                catch (Exception ex) {
                    Console.WriteLine($"Auction run attempted failed with an exception: {ex.Message}");
                }
            });
            Console.WriteLine($"Players purchased {numberOfShares - _failedTransactions} planet shares in {databaseId} on "
                + $"instance {instanceId}");
            return ExitCode.Success;
        }

       public static async Task RunPlanetAuctionAsync(
            string projectId, string instanceId, string databaseId, bool showConsoleOutput)
        {
            /*
                - Get Random Planet with SharesAvailable > 0
                - Get Cost/Share Amount
                - Get Random Player with PlanetDollars > Cost/Share Amount
                - Subtract Planet's Available Shares
                - Subtract Player's PlanetDollars
                - Insert entry into Transaction table
             */

             string connectionString =
            $"Data Source=projects/{projectId}/instances/{instanceId}"
            + $"/databases/{databaseId}";

            // Create connection to Cloud Spanner.
            using (var connection =
                new SpannerConnection(connectionString))
            {

                await connection.OpenAsync();

                using (var transaction =
                        await connection.BeginTransactionAsync())
                {

                    long planetId = 0;
                    string planetName = "";
                    long sharesAvailable = 0;
                    long costPerShare = 0;
                    string playerId = "";
                    string playerName = "";
                    long planetDollars = 0;

                    // Create statement to select a random planet
                    var cmd = connection.CreateSelectCommand(
                    "SELECT PlanetId, PlanetName, SharesAvailable, DIV(PlanetValue, SharesAvailable) as ShareCost "
                    + "FROM (SELECT * FROM Planets TABLESAMPLE BERNOULLI (10 PERCENT)) "
                    + "WHERE SharesAvailable > 0 LIMIT 1");
                    //cmd.Transaction = transaction;
                    // Excecute the select query.
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Read the planet's ID.
                            planetId = reader.GetFieldValue<long>("PlanetId");
                            // Read the planet's Name.
                            planetName = reader.GetFieldValue<string>("PlanetName");
                            // Read the planet's shares available.
                            sharesAvailable = reader.GetFieldValue<long>("SharesAvailable");
                            // Read the planet's cost per share.
                            costPerShare = reader.GetFieldValue<long>("ShareCost");
                        }
                    }
                    if (showConsoleOutput) {
                        Console.WriteLine($"Planet: {planetName}");
                        Console.WriteLine($"Planet sharesAvailable:{sharesAvailable}");
                        Console.WriteLine($"Planet costPerShare: {costPerShare.ToString("N0")}");
                    }
                    // Create statement to select a random player.
                    cmd = connection.CreateSelectCommand(
                    "SELECT PlayerId, PlayerName, PlanetDollars FROM "
                    + "(SELECT * FROM Players TABLESAMPLE BERNOULLI (10 PERCENT)) "
                    + "WHERE PlanetDollars >= @costPerShare LIMIT 1",
                    new SpannerParameterCollection {{"costPerShare", SpannerDbType.Int64}});
                    cmd.Parameters["costPerShare"].Value = costPerShare;
                    //cmd.Transaction = transaction;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            playerId = reader.GetFieldValue<string>("PlayerId");
                            playerName = reader.GetFieldValue<string>("PlayerName");
                            planetDollars = reader.GetFieldValue<long>("PlanetDollars");
                        }
                    }
                    if (showConsoleOutput) {
                        Console.WriteLine($"1 Share of {planetName} sold to {playerName} "
                            + $"for {costPerShare.ToString("N0")} Planet Dollars");
                        Console.WriteLine($"{playerName} now has {planetDollars.ToString("N0")} Planet Dollars");
                    }
                    if (planetId != 0 && playerId != "")
                    {

                        // Subtract 1 from planet's shares available.
                        using (cmd = connection.CreateUpdateCommand(
                            "Planets", new SpannerParameterCollection
                        {
                            {"PlanetId", SpannerDbType.Int64},
                            {"SharesAvailable", SpannerDbType.Int64},
                        }))
                        {
                            cmd.Transaction = transaction;
                            sharesAvailable--;
                            cmd.Parameters["PlanetId"].Value = planetId;
                            cmd.Parameters["SharesAvailable"].Value = sharesAvailable;
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // Subtract cost per share from player's planet dollars.
                        using (cmd = connection.CreateUpdateCommand(
                            "Players", new SpannerParameterCollection
                        {
                            {"PlayerId", SpannerDbType.String},
                            {"PlanetDollars", SpannerDbType.Int64},
                        }))
                        {
                            cmd.Transaction = transaction;
                            planetDollars -= costPerShare;
                            cmd.Parameters["PlayerId"].Value = playerId;
                            cmd.Parameters["PlanetDollars"].Value = planetDollars;
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // Insert record of transaction in Transactions table.
                        using (cmd = connection.CreateInsertCommand(
                            "Transactions", new SpannerParameterCollection
                        {
                            {"PlanetId", SpannerDbType.Int64},
                            {"PlayerId", SpannerDbType.String},
                            {"TimeStamp", SpannerDbType.Timestamp},
                            {"Amount", SpannerDbType.Int64}
                        }))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters["PlanetId"].Value = planetId;
                            cmd.Parameters["PlayerId"].Value = playerId;
                            cmd.Parameters["TimeStamp"].Value = SpannerParameter.CommitTimestamp;
                            cmd.Parameters["Amount"].Value = costPerShare;
                            await cmd.ExecuteNonQueryAsync();
                        }

                        await transaction.CommitAsync();
                    }
                    else
                    {
                        _failedTransactions++;
                        if (showConsoleOutput) {
                            Console.WriteLine("PlanetId or PlayerId was invalid.");
                        }
                    }
                }
                if (showConsoleOutput) {
                    Console.WriteLine("1 Transaction complete");
                }
            }
        }

        public static object CreatePlanetsDatabase(string projectId,
            string instanceId, string databaseId)
        {
            var response =
                CreatePlanetsDatabaseAsync(projectId, instanceId, databaseId);
            s_logger.Info("Waiting for operation to complete...");
            response.Wait();
            s_logger.Info($"Operation status: {response.Status}");
            Console.WriteLine($"Created sample database {databaseId} on "
                + $"instance {instanceId}");
            return ExitCode.Success;
        }

        public static object InsertPlanet(string projectId,
            string instanceId, string databaseId, string planetName, long planetValue)
        {
            var response =
                InsertPlanetAsync(projectId, instanceId, databaseId, planetName, planetValue);
            s_logger.Info("Waiting for operation to complete...");
            response.Wait();
            s_logger.Info($"Operation status: {response.Status}");
            Console.WriteLine($"Inserted planet into {databaseId} on "
                + $"instance {instanceId}");
            return ExitCode.Success;
        }

        public static object BatchInsertPlanets(string projectId,
            string instanceId, string databaseId, string csvFile)
        {
            var response =
                BatchInsertPlanetsAsync(projectId, instanceId, databaseId, csvFile);
            s_logger.Info("Waiting for operation to complete...");
            response.Wait();
            s_logger.Info($"Operation status: {response.Status}");
            Console.WriteLine($"Inserted planets into {databaseId} on "
                + $"instance {instanceId}");
            return ExitCode.Success;
        }

        public static object BatchInsertPlayers(string projectId,
            string instanceId, string databaseId)
        {
            var response =
                BatchInsertPlayersAsync(projectId, instanceId, databaseId);
            s_logger.Info("Waiting for operation to complete...");
            response.Wait();
            s_logger.Info($"Operation status: {response.Status}");
            Console.WriteLine($"Inserted players into {databaseId} on "
                + $"instance {instanceId}");
            return ExitCode.Success;
        }

        public static int Main(string[] args)
        {
            var verbMap = new VerbMap<object>();
            verbMap
               .Add((CreatePlanetsDatabaseOptions opts) =>
                    CreatePlanetsDatabase(opts.projectId, opts.instanceId,
                        opts.databaseId))
                .Add((InsertPlanetOptions opts) =>
                    InsertPlanet(opts.projectId, opts.instanceId,
                        opts.databaseId, opts.planetName, opts.planetValue))
                .Add((BatchInsertPlanetsOptions opts) =>
                    BatchInsertPlanets(opts.projectId, opts.instanceId,
                        opts.databaseId, opts.csvFile))
                .Add((BatchInsertPlayersOptions opts) =>
                    BatchInsertPlayers(opts.projectId, opts.instanceId,
                        opts.databaseId))
                .Add((RunPlanetAuctionOptions opts) =>
                    RunPlanetAuction(opts.projectId, opts.instanceId,
                        opts.databaseId, opts.numberOfShares, opts.showConsoleOutput))
                .NotParsedFunc = (err) => 1;
            return (int)verbMap.Run(args);
        }

        /// <summary>
        /// Returns true if an AggregateException contains a SpannerException
        /// with the given error code.
        /// </summary>
        /// <param name="e">The exception to examine.</param>
        /// <param name="errorCode">The error code to look for.</param>
        /// <returns></returns>
        public static bool ContainsError(AggregateException e, params ErrorCode[] errorCode)
        {
            foreach (var innerException in e.InnerExceptions)
            {
                SpannerException spannerException = innerException as SpannerException;
                if (spannerException != null && errorCode.Contains(spannerException.ErrorCode))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if an AggregateException contains a Grpc.Core.RpcException
        /// with the given error code.
        /// </summary>
        /// <param name="e">The exception to examine.</param>
        /// <param name="errorCode">The error code to look for.</param>
        /// <returns></returns>
        public static bool ContainsGrpcError(AggregateException e,
            Grpc.Core.StatusCode errorCode)
        {
            foreach (var innerException in e.InnerExceptions)
            {
                Grpc.Core.RpcException grpcException = innerException
                    as Grpc.Core.RpcException;
                if (grpcException != null &&
                    grpcException.Status.StatusCode == errorCode)
                    return true;
            }
            return false;
        }
    }
}
