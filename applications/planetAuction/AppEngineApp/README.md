# Google Cloud Spanner and Google App Engine Flexible Environment

This Planet Auction sample application demonstrates Cloud Spanner integration running in Google App Engine Flexible Environment.

## Prerequisites

1.  **Follow the set-up instructions in [the documentation](https://cloud.google.com/dotnet/docs/setup).**

2.  Install the [Google Cloud SDK](https://cloud.google.com/sdk/).  The Google Cloud SDK
    is required to deploy .NET applications to App Engine.

3.  Install the [.NET Core SDK, version 2.0](https://github.com/dotnet/core/blob/master/release-notes/download-archives/2.0.5-download.md).

4.  Use the [PlanetAunctionAdmin](../AdminConsoleApp/README.md) console app to create and populate the Spanner database necessary
    for this app to run as expected.

5.  Edit [appsettings.json](appsettings.json).  Replace `ProjectId`, `InstanceId`, and `DatabaseId` with your project's values.

## ![PowerShell](../.resources/powershell.png) Using PowerShell

### Run Locally

```psm1
PS > dotnet restore
PS > dotnet run
```

### Deploy to App Engine

```psm1
PS > dotnet restore
PS > dotnet publish
PS > gcloud beta app deploy .\bin\Debug\netcoreapp2.0\publish\app.yaml
```


## ![Visual Studio](../.resources/visual-studio.png) Using Visual Studio 2017

[Google Cloud Tools for Visual Studio](
https://marketplace.visualstudio.com/items?itemName=GoogleCloudTools.GoogleCloudPlatformExtensionforVisualStudio)
make it easy to deploy to App Engine.  Install them if you are running Visual Studio.

### Run Locally

Open **PlanetAuction.csproj**, and Press **F5**.

### Deploy to App Engine

1.  In Solution Explorer, right-click the **PlanetAuction** project and choose **Publish PlanetAuction to Google Cloud**.

2.  Click **App Engine Flex**.

3.  Click **Publish**.

## Contributing changes

* See [CONTRIBUTING.md](../../../CONTRIBUTING.md)

## Licensing

* See [LICENSE](../../../LICENSE)

## Testing

* See [TESTING.md](../../../TESTING.md)