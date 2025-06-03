using System.Text;
using System.Text.Json;
using ErrorOr;

namespace GoogleLimitedInputAuth.Net.client;

internal static class RequestExtensionMethods
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions();

    /// <summary>
    /// Sends a POST request to the specified path with the provided content and attempts to deserialize the response body to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to which the response body will be deserialized.</typeparam>
    /// <param name="httpClient">The <see cref="HttpClient"/> used to send the request.</param>
    /// <param name="path">The relative URL path to send the POST request to.</param>
    /// <param name="content">The HTTP content to send in the POST request body.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The result is an <see cref="ErrorOr{T}"/> which contains either the deserialized object of type <typeparamref name="T"/> 
    /// or an error if the request fails or the response is unsuccessful.
    /// </returns>
    /// <exception cref="JsonException">
    /// Thrown if deserialization of the response body fails.
    /// </exception>
    public static async Task<ErrorOr<T>> PostAsync<T>(this HttpClient httpClient, string path, HttpContent content, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync(path, content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode is false)
        {
            return Error.Custom((int)response.StatusCode, response.StatusCode.ToString(), body);
        }

        using var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body));
        return (await JsonSerializer.DeserializeAsync<T>(bodyStream, _jsonOptions, cancellationToken))!;
    }
}