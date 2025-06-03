using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using GoogleLimitedInputAuth.Net.UsageExample.options;
using Microsoft.Extensions.Options;

namespace GoogleLimitedInputAuth.Net.UsageExample;

public sealed class Worker : BackgroundService
{
    private readonly GApiOptions _options;
    private readonly ILogger<Worker> _logger;

    public Worker(IOptions<GApiOptions> options, ILogger<Worker> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var clientSecrets = new ClientSecrets()
        {
            ClientId = _options.ClientId,
            ClientSecret = _options.ClientSecret,
        };
        
        var credentials = await DeviceCredentials.InitializeAsync(
            clientSecrets: clientSecrets,
            scope: [YouTubeService.Scope.YoutubeReadonly],
            store: new FileDataStore("devicecredenials", fullPath: false),
            logger: _logger,
            callback: CredentialsCallback,
            cancellationToken: stoppingToken);

        _logger.LogInformation("Authorization Successful!");
        
        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credentials,
            ApplicationName = _options.ApplicationName
        });
        
        // use apis
    }

    private static Task CredentialsCallback(GDeviceCode code)
    {
        Console.WriteLine("=========== My Google Authentication Callback ===========");
        Console.WriteLine($"User Code: {code.user_code}");
        Console.WriteLine($"Verification Url: {code.verification_url}");

        return Task.CompletedTask;
    }
}