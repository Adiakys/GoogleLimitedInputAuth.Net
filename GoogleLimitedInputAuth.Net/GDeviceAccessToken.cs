namespace Google.Apis.Auth.OAuth2;

/// <summary>
/// Represents an OAuth 2.0 access token and associated metadata for Google's Device Authorization flow.
/// </summary>
public sealed class GDeviceAccessToken
{
    /// <summary>
    /// Gets the access token used to authenticate API requests.
    /// </summary>
    public string access_token { get; init; } = string.Empty;

    /// <summary>
    /// Gets the refresh token used to obtain new access tokens when the current one expires.
    /// </summary>
    public string refresh_token { get; init; } = string.Empty;

    /// <summary>
    /// Gets the number of seconds until the access token expires from the time it was issued.
    /// </summary>
    public int expires_in { get; init; }

    /// <summary>
    /// Gets the number of seconds until the refresh token expires from the time it was issued.
    /// If set to 0, the refresh token does not expire.
    /// </summary>
    public int refresh_token_expires_in { get; init; }

    /// <summary>
    /// Gets the scopes granted by the access token.
    /// </summary>
    public string scope { get; init; }

    /// <summary>
    /// Gets the type of the token (typically "Bearer").
    /// </summary>
    public string token_type { get; init; } = string.Empty;

    /// <summary>
    /// Gets the timestamp when the token was issued.
    /// </summary>
    public DateTimeOffset issuedAt { get; init; } = DateTimeOffset.Now;

    /// <summary>
    /// Determines whether the access token has expired based on the current time.
    /// </summary>
    /// <returns><c>true</c> if the access token has expired; otherwise, <c>false</c>.</returns>
    public bool IsAccessTokenExpired() =>
        DateTimeOffset.Now >= issuedAt + TimeSpan.FromSeconds(expires_in);

    /// <summary>
    /// Determines whether the refresh token has expired based on the current time.
    /// </summary>
    /// <returns><c>true</c> if the refresh token has expired; otherwise, <c>false</c>.</returns>
    public bool IsRefreshTokenExpired() =>
        refresh_token_expires_in is not 0 &&
        DateTimeOffset.Now >= issuedAt + TimeSpan.FromSeconds(refresh_token_expires_in);
}

