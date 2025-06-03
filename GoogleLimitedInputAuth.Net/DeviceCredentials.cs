using Google.Apis.Http;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using GoogleLimitedInputAuth.Net.client;

namespace Google.Apis.Auth.OAuth2;

/// <summary>
/// Handles OAuth 2.0 Device Authorization Grant flow and token management for Google APIs.
/// Implements token storage, refresh logic, and HTTP client initialization/interception for authorized requests.
/// </summary>
public sealed class DeviceCredentials : IConfigurableHttpClientInitializer, IHttpExecuteInterceptor, IHttpUnsuccessfulResponseHandler
{
    /// <summary>
    /// Gets the current access token and metadata.
    /// </summary>
    public GDeviceAccessToken Token { get; private set; }

    private readonly FileDataStore? _store;
    private readonly ClientSecrets _clientSecrets;
    private readonly ILogger? _logger;

    private DeviceCredentials(GDeviceAccessToken token, ClientSecrets clientSecrets, FileDataStore? store, ILogger? logger)
    {
        Token = token;

        _clientSecrets = clientSecrets;
        _store = store;
        _logger = logger;
    }

    /// <summary>
    /// Initializes the <see cref="DeviceCredentials"/> instance using either a stored token or by starting the device authorization flow.
    /// </summary>
    /// <param name="clientSecrets">The client secrets required for authorization.</param>
    /// <param name="scope">The scopes required for API access.</param>
    /// <param name="store">Optional token storage mechanism.</param>
    /// <param name="callback">Optional callback to display device code and URL to the user.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="cancellationToken">Token for cancelling the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="DeviceCredentials"/> instance.</returns>
    public static async Task<DeviceCredentials> InitializeAsync(ClientSecrets clientSecrets, IEnumerable<string> scope, FileDataStore? store = null, Func<GDeviceCode, Task>? callback = null, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        callback ??= DefaultCallback;
        var token = await LoadFromStoreAsync(store, cancellationToken);

        if (token is null || token.IsRefreshTokenExpired())
        {
            token = await AuthorizeAsync(clientSecrets, scope, callback, cancellationToken);
            await StoreAsync(store, token);
        }

        return new(token, clientSecrets, store, logger);
    }

    /// <inheritdoc/>
    public void Initialize(ConfigurableHttpClient httpClient)
    {
        httpClient.MessageHandler.Credential = this;
    }

    /// <inheritdoc/>
    public async Task<bool> HandleResponseAsync(HandleUnsuccessfulResponseArgs args)
    {
        if (args.Response.StatusCode == HttpStatusCode.Unauthorized)
        {
            try
            {
                await this.RefreshTokenAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "{Message}", ex.Message);
                return false;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task InterceptAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (Token.IsAccessTokenExpired())
        {
            await this.RefreshTokenAsync(cancellationToken);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue(Token.token_type, Token.access_token);
    }


    /// <summary>
    /// Refreshes the access token using the stored refresh token.
    /// </summary>
    /// <param name="cancellationToken">Token for cancelling the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the refresh request fails.</exception>
    public async Task RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        using var client = new GoogleApiAuthClient(_clientSecrets);
        var response = await client.RefreshTokenAsync(Token.refresh_token, cancellationToken);

        if (response.IsError)
        {
            throw new UnauthorizedAccessException($"Google API refresh token request failed for client {_clientSecrets.ClientId} (status code {response.FirstError.NumericType}): {response.FirstError.Description}.");
        }

        Token = response.Value;
        await StoreAsync(_store, response.Value);
    }

    /// <summary>
    /// Performs the device authorization flow to obtain an access token.
    /// </summary>
    /// <param name="clientSecrets">The client credentials.</param>
    /// <param name="scope">The scopes required for the application.</param>
    /// <param name="callback">Callback to display user code and verification URL.</param>
    /// <param name="cancellationToken">Token for cancelling the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the obtained access token.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if authorization fails.</exception>
    private static async Task<GDeviceAccessToken> AuthorizeAsync(ClientSecrets clientSecrets, IEnumerable<string> scope, Func<GDeviceCode, Task> callback, CancellationToken cancellationToken = default)
    {
        using var client = new GoogleApiAuthClient(clientSecrets);

        var code = await client.GetDeviceCodeAndUserCodeAsync(scope, cancellationToken);

        if (code.IsError)
        {
            throw new UnauthorizedAccessException($"Google API authorization failed for client {clientSecrets.ClientId}.");
        }

        await callback(code.Value);

        var authres = await client.PollAsync(code.Value, cancellationToken);

        if (authres.IsError)
        {
            throw new UnauthorizedAccessException($"Google API authorization failed for client {clientSecrets.ClientId}.");
        }

        return authres.Value;
    }

    /// <summary>
    /// Attempts to load an access token from the provided <see cref="FileDataStore"/>.
    /// </summary>
    /// <param name="store">The data store from which to load the token.</param>
    /// <param name="cancellationToken">Token for cancelling the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the access token, or <c>null</c> if not found.</returns>
    internal static async Task<GDeviceAccessToken?> LoadFromStoreAsync(FileDataStore? store, CancellationToken cancellationToken)
    {
        if (store is null)
        {
            return null;
        }

        var access_token = await store.GetAsync<string?>(nameof(GDeviceAccessToken.access_token));

        if (access_token is null)
        {
            return null;
        }

        return new GDeviceAccessToken()
        {
            access_token = access_token,
            refresh_token = await store.GetAsync<string>(nameof(GDeviceAccessToken.refresh_token)),
            expires_in = await store.GetAsync<int>(nameof(GDeviceAccessToken.expires_in)),
            refresh_token_expires_in = await store.GetAsync<int>(nameof(GDeviceAccessToken.refresh_token_expires_in)),
            scope = await store.GetAsync<string>(nameof(GDeviceAccessToken.scope)),
            issuedAt = await store.GetAsync<DateTimeOffset>(nameof(GDeviceAccessToken.issuedAt))
        };
    }

    /// <summary>
    /// Stores the access token to the provided <see cref="FileDataStore"/>.
    /// </summary>
    /// <param name="store">The data store to write the token to.</param>
    /// <param name="token">The access token to store.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal static async Task StoreAsync(FileDataStore? store, GDeviceAccessToken token)
    {
        if (store is null)
        {
            return;
        }

        await store.StoreAsync(nameof(GDeviceAccessToken.access_token), token.access_token);
        await store.StoreAsync(nameof(GDeviceAccessToken.scope), token.scope);
        await store.StoreAsync(nameof(GDeviceAccessToken.expires_in), token.expires_in);
        await store.StoreAsync(nameof(GDeviceAccessToken.issuedAt), token.issuedAt);

        if (string.IsNullOrEmpty(token.refresh_token) is false)
        {
            await store.StoreAsync(nameof(GDeviceAccessToken.refresh_token), token.refresh_token);
            await store.StoreAsync(nameof(GDeviceAccessToken.refresh_token_expires_in), token.refresh_token_expires_in);
        }
    }


    /// <summary>
    /// Default callback used to display the user code and verification URL to the console.
    /// </summary>
    /// <param name="code">The device code and user code information.</param>
    /// <returns>A completed task.</returns>
    private static Task DefaultCallback(GDeviceCode code)
    {
        Console.WriteLine("=========== Google Authentication ===========");
        Console.WriteLine($"User Code: {code.user_code}");
        Console.WriteLine($"Verification Url: {code.verification_url}");

        return Task.CompletedTask;
    }
}