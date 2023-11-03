using System.Text.Json;

namespace SystemTextJson.FluentApi;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions SerializeAsObject<T>(this JsonSerializerOptions options)
        where T : class
    {
        options.Converters.Add(ObjectSerializer<T>.Instance);
        return options;
    }
}