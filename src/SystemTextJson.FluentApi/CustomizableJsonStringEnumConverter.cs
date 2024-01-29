using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemTextJson.FluentApi;
public class CustomizableJsonStringEnumConverter : JsonConverterFactory
{
    private readonly JsonStringEnumConverter _defaultConverter;
    public CustomizableJsonStringEnumConverter() =>
        _defaultConverter = new JsonStringEnumConverter();

    public CustomizableJsonStringEnumConverter(JsonNamingPolicy? namingPolicy = null, bool allowIntegerValues = true) =>
        _defaultConverter = new JsonStringEnumConverter(namingPolicy, allowIntegerValues);

    public override bool CanConvert(Type typeToConvert) =>
        _defaultConverter.CanConvert(typeToConvert);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var defaultConverter = _defaultConverter.CreateConverter(typeToConvert, options);

        foreach (var field in typeToConvert.GetFields())
        {
            if (!field.IsLiteral)
                continue;

            var attribute = field.GetCustomAttribute<JsonPropertyNameAttribute>();
            if (attribute is null)
                continue;

            var type = typeof(CustomEnumConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(type, defaultConverter, options)!;
        }

        return defaultConverter;
    }
}
