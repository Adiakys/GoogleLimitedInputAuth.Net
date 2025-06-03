# Google Device Authorization Flow Library for .NET

This library provides a clean and extensible way to authenticate with Google APIs using the OAuth 2.0 Device Authorization Grant, ideal for devices and services that lack a browser or have limited input capabilities.

## ✨ Features

- Device Code Flow for OAuth 2.0 with refresh token support
- Token caching with FileDataStore
- Automatic access token refresh on expiration
- Pluggable logging and custom user interaction callbacks
- Designed for use with Google APIs (e.g., YouTube Data API v3)

## 📦 Installation

Currently not published on NuGet. You can include this library in your project by copying the source or referencing it as a submodule.

## 🚀 Quick Start
Here’s how to authenticate with Google using the Device Authorization flow and create a YouTubeService client:

```csharp
var credentials = await DeviceCredentials.InitializeAsync(
    clientSecrets: new ClientSecrets
    {
        ClientId = _options.ClientId,
        ClientSecret = _options.ClientSecret
    },
    scope: [YouTubeService.Scope.YoutubeReadonly],
    store: new FileDataStore("gcredentials", fullPath: false),
    callback: ShowCodeToUser,
    logger: _logger,
    cancellationToken: stoppingToken);

var youtube = new YouTubeService(new BaseClientService.Initializer
{
    HttpClientInitializer = credentials,
    ApplicationName = _options.ApplicationName
});

static Task ShowCodeToUser(GDeviceCode code)
{
    Console.WriteLine($"Visit {code.verification_url} and enter the code: {code.user_code}");
    return Task.CompletedTask;
}
```

### 🔍 What this does:

1. Initialize the credentials using your OAuth 2.0 ClientId and ClientSecret.

2. Specify the scope you need access to — in this case, read-only access to YouTube.

3. Use a file-based store to persist tokens locally (so the user doesn't have to re-authenticate each time).

4. Provide a callback function (ShowCodeToUser) to display the verification URL and user code.

5. Once authorized, it automatically refreshes tokens when needed.

6. Finally, it creates an authenticated YouTubeService client you can use to call the YouTube Data API.

## 📄 License
MIT License. See [LICENSE](./LICENSE) for details.

## 👨‍💻 Dependencies & References
 - [Google.Apis.Auth](https://github.com/googleapis/google-api-dotnet-client)
 - [Google OAuth 2.0 Device Flow](https://developers.google.com/identity/protocols/oauth2/limited-input-device?hl=it)