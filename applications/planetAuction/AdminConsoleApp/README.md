# .NET Planet Auction Admin Console App - Cloud Spanner Sample

The Planet Auction Admin sample demonstrates how to call the
[Google Cloud Spanner API](https://cloud.google.com/spanner/docs/) from C#.

This sample requires [.NET Core 2.0](
    https://www.microsoft.com/net/core) or later.  That means using
[Visual Studio 2017](
    https://www.visualstudio.com/), or the command line.

## Build and Run

1.  **Follow the set-up instructions in [the documentation](https://cloud.google.com/dotnet/docs/setup).**

4.  Enable APIs for your project.
    [Click here](https://console.cloud.google.com/flows/enableapi?apiid=spanner.googleapis.com&showconfirmation=true)
    to visit Cloud Platform Console and enable the Google Cloud Spanner API.

5.  In the [Cloud Console](https://console.cloud.google.com/spanner/), create a Cloud Spanner instance that will be used for creating this sample's required database and tables.

10. Run the PlanetAuctionAdmin sample to see a list of subcommands:
    ```
    PS C:\...\csharp-docs-samples\applications\planetAuction\AdminConsoleApp> dotnet run
    PlanetAuctionAdmin 1.0.0
    Copyright (C) 2018 PlanetAuctionAdmin

    ERROR(S):
    No verb selected.

    createPlanetsDatabase    Create a sample Cloud Spanner database along with sample tables in your project.

    insertPlanet             Insert a planet into a Cloud Spanner database.

    batchInsertPlanets       Batch insert planets into a Cloud Spanner database.

    batchInsertPlayers       Batch insert players into a Cloud Spanner database.

    runPlanetAuction         Initiate automated player purchase of planet shares in a Cloud Spanner database.

    help                     Display more information on a specific command.

    version                  Display version information.
    ```

    ```
    PS > dotnet run createPlanetsDatabase your-project-id your-instance your-database
    Waiting for operation to complete...
    Database create operation state: Ready
    Operation status: RanToCompletion
    Created sample database your-database on instance your-instance
    ```

## Contributing changes

* See [CONTRIBUTING.md](../../CONTRIBUTING.md)

## Licensing

* See [LICENSE](../../LICENSE)

## Testing

* See [TESTING.md](../../TESTING.md)
