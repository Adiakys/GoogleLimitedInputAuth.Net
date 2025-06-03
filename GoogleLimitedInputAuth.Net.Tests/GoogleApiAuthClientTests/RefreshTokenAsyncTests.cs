using System.Net;
using FluentAssertions;
using Google.Apis.Auth.OAuth2;
using GoogleLimitedInputAuth.Net.client;
using Moq;
using Moq.Protected;

namespace GoogleLimitedInputAuth.Net.Tests.GoogleApiAuthClientTests;

public class RefreshTokenAsyncTests
{
    [Fact]
    public async Task RefreshTokenAsync_WhenSuccessful__ReturnsToken()
    {
        // Arrange
        var token = new GDeviceAccessToken
        {
            access_token = "token",
            refresh_token = "refresh",
            expires_in = 3600,
            refresh_token_expires_in = 36000,
            scope = "scope",
            token_type = "Bearer",
            issuedAt = DateTimeOffset.Now
        };
        var responseContent = System.Text.Json.JsonSerializer.Serialize(token);

        var httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        var httpClient = new HttpClient(httpHandlerMock.Object)
        {
            BaseAddress = new Uri("https://oauth2.googleapis.com")
        };

        var clientSecrets = new ClientSecrets { ClientId = "id", ClientSecret = "secret" };
        var authClient = new GoogleApiAuthClient(clientSecrets, httpClient);

        // Act
        var result = await authClient.RefreshTokenAsync("refresh_token");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.access_token.Should().Be("token");

        httpHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
            ItExpr.IsAny<CancellationToken>());
    }
}