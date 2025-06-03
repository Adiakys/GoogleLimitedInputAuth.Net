using FluentAssertions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace GoogleLimitedInputAuth.Net.Tests;

public class DeviceCredentialsTests
{
    [Fact]
    public async Task InitializeAsync_WhenTokenIsStored_ReturnsCachedToken()
    {
        // Arrange
        var store = new FileDataStore("InitializeAsync_WhenTokenIsStored_ReturnsCachedToken");

        var cachedToken = new GDeviceAccessToken
        {
            access_token = "cached-valid-access",
            refresh_token = "cached-valid-refresh",
            expires_in = 3600,
            refresh_token_expires_in = 36000,
            scope = "scope",
            issuedAt = DateTimeOffset.Now.AddMinutes(-10)
        };

        await DeviceCredentials.StoreAsync(store, cachedToken);

        var clientSecrets = new ClientSecrets { ClientId = "id", ClientSecret = "secret" };

        // Act
        var credentials = await DeviceCredentials.InitializeAsync(clientSecrets, [ "scope" ], store);

        // Assert
        credentials.Token.access_token.Should().Be(cachedToken.access_token);
        credentials.Token.refresh_token.Should().Be(cachedToken.refresh_token);
        credentials.Token.issuedAt.Should().Be(cachedToken.issuedAt);

        await store.ClearAsync();
    }
}