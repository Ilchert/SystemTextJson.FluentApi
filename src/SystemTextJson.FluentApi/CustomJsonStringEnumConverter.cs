using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SystemTextJson.FluentApi;
public class CustomJsonStringEnumConverter : JsonConverterFactory
{
    private JsonStringEnumConverter _defaultConverter;
    public CustomJsonStringEnumConverter() =>
        _defaultConverter = new JsonStringEnumConverter();

    public CustomJsonStringEnumConverter(JsonNamingPolicy? namingPolicy = null, bool allowIntegerValues = true) =>
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
            return (JsonConverter)Activator.CreateInstance(type, defaultConverter, options);
        }

        return defaultConverter;

    }

    private class CustomEnumConverter<T> : JsonConverter<T>
        where T : Enum
    {
        private readonly JsonConverter<T> _inner;
        private readonly Dictionary<string, T> _valuesCache;
        private readonly Dictionary<T, JsonEncodedText> _namesCache;

        public CustomEnumConverter(JsonConverter<T> inner, JsonSerializerOptions options)
        {
            _inner = inner;
            _valuesCache = [];
            _namesCache = [];

            foreach (var field in typeof(T).GetFields())
            {
                if (!field.IsLiteral)
                    continue;

                var attribute = field.GetCustomAttribute<JsonPropertyNameAttribute>();
                if (attribute is null)
                    continue;

                _valuesCache[attribute.Name] = (T)field.GetRawConstantValue();
                _namesCache[(T)field.GetRawConstantValue()] = JsonEncodedText.Encode(attribute.Name, options.Encoder);
            }

            _inner = inner;
        }

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var valueStr = reader.GetString();
                if (valueStr is null)
                    return default;

                if (_valuesCache.TryGetValue(valueStr, out var value))
                    return value;
            }

            return _inner.Read(ref reader, typeToConvert, options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (_namesCache.TryGetValue(value, out var valueStr))
                writer.WriteStringValue(valueStr);
            else
                _inner.Write(writer, value, options);
        }
    }
}
