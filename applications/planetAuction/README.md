# .NET Planet Auction App - Cloud Spanner Sample

The Planet Auction sample demonstrates how to call the
[Google Cloud Spanner API](https://cloud.google.com/spanner/docs/) from C#.

This sample requires [.NET Core 2.0](
    https://www.microsoft.com/net/core) or later.  That means using
[Visual Studio 2017](
    https://www.visualstudio.com/), or the command line.

This app is comprised of 2 sub-app components:
* [Admininstration app](./AdminConsoleApp/)
    * Use this console app first, to create a sample database with sample tables.
    * You can also use this app to insert Planets one at a time or in bulk, using a csv file.
    * You can use this app to bulk insert 249,900 Players at a time.
    * Once you've got Planets and Players in your database you can run a simulated Planet Auction.

* [App Engine web app](./AppEngineApp/)
    * Use this App Engine app to insert a Player and purchase random Planet shares with that Player.
    * You can run this app locally.
    * You can deploy this app to App Engine to use it from a scalably hosted website.

## Contributing changes

* See [CONTRIBUTING.md](../../CONTRIBUTING.md)

## Licensing

* See [LICENSE](../../LICENSE)

## Testing

* See [TESTING.md](../../TESTING.md)

