namespace GoogleLimitedInputAuth.Net.contracts;

internal sealed record GetDeviceCodeAndUserCodeRequest
{
    public string client_id { get; init; } = string.Empty;
    public string scope { get; init; } = string.Empty;

    public FormUrlEncodedContent Content()
    {
        var values = new Dictionary<string, string>()
        {
            { "client_id", client_id },
            { "scope", scope }
        };

        return new FormUrlEncodedContent(values);
    }
}