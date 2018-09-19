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
using PlanetAuction.ViewModels;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Google.Cloud.Storage.V1;
using Google.Cloud.Spanner.Data;
using Google.Cloud.Spanner.V1;
using System.Text;
using Microsoft.Extensions.Options;
using Google;
using System;

namespace PlanetAuction.Controllers
{
    public class HomeController : Controller
    {
        // Contains the bucket name and object name
        readonly PlanetAuctionOptions _options;

        // The Google Cloud Storage client.
        //readonly StorageClient _storage;

        public HomeController(IOptions<PlanetAuctionOptions> options)
        {
            _options = options.Value;
            //_storage = StorageClient.Create();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new HomeIndex();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(Form sendForm)
        {
            var model = new HomeIndex();
            model.Content = sendForm.Content;
            model.SavedNewContent = true;

            // Spanner connection string.
            string connectionString =
                $"Data Source=projects/{_options.ProjectId}/instances/{_options.InstanceId}"
                + $"/databases/{_options.DatabaseId}";

            // Insert Player if PlayerID not present in sent form data.
            string playerId = "";
            if(string.IsNullOrEmpty(sendForm.PlayerId))
            {
                // Insert Player Code
                using (var connection = new SpannerConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var tx = await connection.BeginTransactionAsync())
                    {
                        using (var cmd = connection.CreateInsertCommand(
                            "Players", new SpannerParameterCollection
                        {
                            { "PlayerId", SpannerDbType.String },
                            { "PlayerName", SpannerDbType.String },
                            { "PlanetDollars", SpannerDbType.Int64 }
                        }))
                        {
                            cmd.Transaction = tx;
                            playerId = Guid.NewGuid().ToString("N");
                            cmd.Parameters["PlayerId"].Value = playerId;
                            cmd.Parameters["PlayerName"].Value = sendForm.Content;
                            cmd.Parameters["PlanetDollars"].Value = 1000000;
                            cmd.ExecuteNonQuery();
                        }
                        await tx.CommitAsync();
                    }
                }
                model.PlayerId = playerId;
            }
            else {
                model.PlayerId = sendForm.PlayerId;
                playerId = sendForm.PlayerId;
            }

            // Submit transaction for Player purchase of 1 planet share.
            using (var connection = new SpannerConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var transaction =
                        await connection.BeginTransactionAsync())
                {

                    long planetId = 0;
                    string planetName = "";
                    long sharesAvailable = 0;
                    long costPerShare = 0;
                    //string playerId = playerId;
                    string playerName = "";
                    long planetDollars = 0;

                    // Create statement to select a random planet
                    var cmd = connection.CreateSelectCommand(
                    "SELECT PlanetId, PlanetName, SharesAvailable, DIV(PlanetValue, SharesAvailable) as ShareCost "
                    + "FROM (SELECT * FROM Planets TABLESAMPLE BERNOULLI (10 PERCENT)) "
                    + "WHERE SharesAvailable > 0 LIMIT 1");

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
                    // Create statement to select player details.
                    cmd = connection.CreateSelectCommand(
                    "SELECT PlayerId, PlayerName, PlanetDollars FROM Players "
                    + "WHERE PlayerId = @playerId",
                    new SpannerParameterCollection {{"playerId", SpannerDbType.String}});
                    cmd.Parameters["playerId"].Value = playerId;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            playerId = reader.GetFieldValue<string>("PlayerId");
                            playerName = reader.GetFieldValue<string>("PlayerName");
                            planetDollars = reader.GetFieldValue<long>("PlanetDollars");
                        }
                    }
                    if (planetDollars >= costPerShare && planetId != 0)
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

                        model.Status =  $"1 Share of {planetName} sold to {playerName} "
                         + $"for {costPerShare.ToString("N0")} Planet Dollars. "
                         + $"{playerName} now has {planetDollars.ToString("N0")} Planet Dollars.";
                    }
                    else
                    {
                        if(planetId == 0) {
                            model.Status = "Failed to acquire a valid Planet share. Please retry.";
                        }
                        else
                        {
                           // Player doesn't have enough Planet Dollars to purchase planet share
                           model.Status = $"{planetDollars.ToString("N0")} Planet Dollars is not enough to purchase a share of {planetName}";
                        }
                    }
                }
            }
            return View(model);
        }


        public IActionResult Error()
        {
            return View();
        }
    }
}
