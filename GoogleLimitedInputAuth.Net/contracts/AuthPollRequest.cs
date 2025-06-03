namespace GoogleLimitedInputAuth.Net.contracts;

internal sealed class AuthPollRequest
{
    public string client_id { get; init; } = string.Empty;

    public string client_secret { get; init; } = string.Empty;

    public string device_code { get; init; } = string.Empty;

    public string grant_type { get; private init; } = "urn:ietf:params:oauth:grant-type:device_code";

    public FormUrlEncodedContent Content()
    {
        var values = new Dictionary<string, string>()
        {
            { "client_id", client_id },
            { "client_secret", client_secret },
            { "device_code", device_code },
            { "grant_type", grant_type }
        };

        return new FormUrlEncodedContent(values);
    }
}