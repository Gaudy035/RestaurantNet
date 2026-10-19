using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntegrationTests.Helpers;

public static class HttpResponseExtensions
{
    // Matches the API's JSON options (camelCase + enums serialized as strings).
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<T> ReadJsonAsync<T>(this HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, Options)!;
    }
}
