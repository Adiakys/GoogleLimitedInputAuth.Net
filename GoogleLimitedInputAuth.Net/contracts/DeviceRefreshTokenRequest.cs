using Google.Apis.Auth.OAuth2.Requests;

namespace GoogleLimitedInputAuth.Net.contracts;

internal sealed class DeviceRefreshTokenRequest : RefreshTokenRequest
{
    public string client_id { get; init; } = string.Empty;
    public string client_secret { get; init; } = string.Empty;
    public string refresh_token { get; init; } = string.Empty;

    public FormUrlEncodedContent Content()
    {
        var values = new Dictionary<string, string>()
        {
            { "client_id", client_id },
            { "client_secret", client_secret },
            { "refresh_token", refresh_token },
            { "grant_type", this.GrantType }
        };

        return new FormUrlEncodedContent(values);
    }
}