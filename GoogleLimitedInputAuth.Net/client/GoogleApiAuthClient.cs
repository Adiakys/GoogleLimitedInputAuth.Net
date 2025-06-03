using ErrorOr;
using Google.Apis.Auth.OAuth2;
using GoogleLimitedInputAuth.Net.contracts;

namespace GoogleLimitedInputAuth.Net.client;

/// <summary>
/// Handles communication with Google's OAuth 2.0 Device Authorization endpoints.
/// Provides methods for obtaining device/user codes, polling for access tokens, and refreshing tokens.
/// </summary>
internal sealed class GoogleApiAuthClient : IDisposable
{
    private readonly ClientSecrets _clientCredentials;
    internal readonly HttpClient _httpClient;


    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleApiAuthClient"/> class with the specified client credentials.
    /// </summary>
    /// <param name="clientCredentials">The client credentials containing the client ID and secret.</param>
    public GoogleApiAuthClient(ClientSecrets clientCredentials)
    {
        _clientCredentials = clientCredentials;

        _httpClient = new HttpClient()
        {
            BaseAddress = new(GoogleApiConstants.BaseAddress)
        };
    }

    /// <summary>
    /// ONLY FOR TEST PURPOUSE
    /// </summary>
    internal GoogleApiAuthClient(ClientSecrets clientCredentials, HttpClient httpClient)
    {
        _clientCredentials = clientCredentials;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Initiates the device authorization request to retrieve a device code and user verification URL.
    /// </summary>
    /// <param name="scope">The scopes requested by the application.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="GetDeviceCodeAndUserCodeResponse"/> with device code, user code, and verification URL.
    /// </returns>
    public async Task<ErrorOr<GetDeviceCodeAndUserCodeResponse>> GetDeviceCodeAndUserCodeAsync(IEnumerable<string> scope, CancellationToken cancellationToken = default)
    {
        var request = new GetDeviceCodeAndUserCodeRequest()
        {
            client_id = _clientCredentials.ClientId,
            scope = string.Join(" ", scope)
        };

        return await _httpClient.PostAsync<GetDeviceCodeAndUserCodeResponse>(GoogleApiConstants.DeviceCodeApiPath, request.Content(), cancellationToken);
    }


    /// <summary>
    /// Repeatedly polls Google's token endpoint to obtain an access token after user authorization.
    /// </summary>
    /// <param name="authcode">The response from the initial device code request.</param>
    /// <param name="cancellationToken">Token to cancel the polling operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains either a valid <see cref="GDeviceAccessToken"/> or an error.
    /// </returns>
    public async Task<ErrorOr<GDeviceAccessToken>> PollAsync(GetDeviceCodeAndUserCodeResponse authcode, CancellationToken cancellationToken = default)
    {
        var pollresponse = await this.SinglePollAsync(authcode, cancellationToken);

        while (pollresponse.IsError && pollresponse.FirstError.NumericType is 428)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            pollresponse = await this.SinglePollAsync(authcode, cancellationToken);
        }

        return pollresponse;
    }

    /// <summary>
    /// Sends a single polling request to Google's token endpoint using the provided device code.
    /// </summary>
    /// <param name="authcode">The response from the device code initiation.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains either a valid <see cref="GDeviceAccessToken"/> or an error.
    /// </returns>
    public async Task<ErrorOr<GDeviceAccessToken>> SinglePollAsync(GetDeviceCodeAndUserCodeResponse authcode, CancellationToken cancellationToken = default)
    {
        var request = new AuthPollRequest()
        {
            client_id = _clientCredentials.ClientId,
            client_secret = _clientCredentials.ClientSecret,
            device_code = authcode.device_code,
        };

        return await _httpClient.PostAsync<GDeviceAccessToken>(GoogleApiConstants.TokeApiPath, request.Content(), cancellationToken);
    }

    /// <summary>
    /// Refreshes the access token using the provided refresh token.
    /// </summary>
    /// <param name="refresh_token">The refresh token issued during initial authorization.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains either a new <see cref="GDeviceAccessToken"/> or an error.
    /// </returns>
    public async Task<ErrorOr<GDeviceAccessToken>> RefreshTokenAsync(string refresh_token, CancellationToken cancellationToken = default)
    {
        var request = new DeviceRefreshTokenRequest()
        {
            client_id = _clientCredentials.ClientId,
            client_secret = _clientCredentials.ClientSecret,
            refresh_token = refresh_token
        };

        var response = await _httpClient.PostAsync<GDeviceAccessToken>(GoogleApiConstants.TokeApiPath, request.Content(), cancellationToken);
        return response;
    }

    /// <inheritdoc/>
    public void Dispose() => _httpClient.Dispose();
}