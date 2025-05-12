using System.Text.Json;

namespace CQRS.AspNet;

public static class HttpResponseMessageExtensions
{
    public static async Task<T> As<T>(this HttpContent response, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await response.ReadFromJsonAsync<T>(options, cancellationToken);
            if (result == null)
            {
                string message = $"Unable to deserialize the content of the response into the specified type.{typeof(T)}";                                
                throw new JsonException(message);
            }
            return result;
        }
        catch (Exception ex)
        {
            var stringResponse = await response.ReadAsStringAsync(cancellationToken);
            string message =
            $"""
            There was a problem deserializing the content of the response into the specified type ({typeof(T)}).
            The raw string response was
            {stringResponse}
            """;
            throw new JsonException(message, ex);
        }
    }
}