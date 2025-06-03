using System.Net;
using ErrorOr;
using FluentAssertions;
using Google.Apis.Auth.OAuth2;
using GoogleLimitedInputAuth.Net.client;
using GoogleLimitedInputAuth.Net.contracts;
using Moq;
using Moq.Protected;

namespace GoogleLimitedInputAuth.Net.Tests.GoogleApiAuthClientTests;

public sealed class GetDeviceCodeAndUserCodeAsyncTests
{
    [Fact]
    public async Task GetDeviceCodeAndUserCodeAsync_WhenRequestOk_ReturnsResponse()
    {
        // Arrange
        var expectedResponse = new GetDeviceCodeAndUserCodeResponse
        {
            device_code = "device-code",
            user_code = "user-code",
            verification_url = "http://verify"
        };

        var httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expectedResponse))
            })
            .Verifiable();

        var httpClient = new HttpClient(httpHandlerMock.Object)
        {
            BaseAddress = new System.Uri("https://oauth2.googleapis.com")
        };

        var clientSecrets = new ClientSecrets { ClientId = "id", ClientSecret = "secret" };
        var authClient = new GoogleApiAuthClient(clientSecrets, httpClient);

        // Act
        var result = await authClient.GetDeviceCodeAndUserCodeAsync(["scope"]);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.device_code.Should().Be(expectedResponse.device_code);
        result.Value.user_code.Should().Be(expectedResponse.user_code);

        httpHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetDeviceCodeAndUserCodeAsync_WhenRequestFails_ReturnsError()
    {
        // Arrange
        var httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(Error.Unauthorized()))
            })
            .Verifiable();

        var httpClient = new HttpClient(httpHandlerMock.Object)
        {
            BaseAddress = new Uri("https://oauth2.googleapis.com")
        };

        var clientSecrets = new ClientSecrets { ClientId = "id", ClientSecret = "secret" };
        var authClient = new GoogleApiAuthClient(clientSecrets, httpClient);

        // Act
        var result = await authClient.GetDeviceCodeAndUserCodeAsync(["scope"]);

        // Assert
        result.IsError.Should().BeTrue();

        httpHandlerMock.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
            ItExpr.IsAny<CancellationToken>());
    }
}