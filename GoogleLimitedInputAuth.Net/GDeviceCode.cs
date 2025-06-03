namespace Google.Apis.Auth.OAuth2;

/// <summary>
/// Represents the device code and user verification URL returned during the OAuth 2.0 Device Authorization Grant flow.
/// </summary>
public abstract class GDeviceCode
{
    /// <summary>
    /// Gets the user code that the user must enter on the verification URL to authorize the device.
    /// </summary>
    public string user_code { get; init; } = string.Empty;

    /// <summary>
    /// Gets the URL the user must visit to complete the device authorization process.
    /// </summary>
    public string verification_url { get; init; } = string.Empty;
}
