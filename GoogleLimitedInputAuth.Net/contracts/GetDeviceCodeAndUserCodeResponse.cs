using Google.Apis.Auth.OAuth2;

namespace GoogleLimitedInputAuth.Net.contracts;

internal sealed class GetDeviceCodeAndUserCodeResponse : GDeviceCode
{
    public string device_code { get; init; } = string.Empty;
    public int expires_in { get; init; } = 0;
    public int interval { get; init; } = 0;
}