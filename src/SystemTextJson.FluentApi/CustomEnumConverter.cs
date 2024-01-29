using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemTextJson.FluentApi;

internal class CustomEnumConverter<T> : JsonConverter<T>
        where T : struct, Enum
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

            _valuesCache[attribute.Name] = (T)field.GetRawConstantValue()!;
            _namesCache[(T)field.GetRawConstantValue()!] = JsonEncodedText.Encode(attribute.Name, options.Encoder);
        }

        _inner = inner;
    }

    public CustomEnumConverter(JsonConverter<T> inner, JsonSerializerOptions options, IReadOnlyDictionary<T, string> mapping)
    {
        _inner = inner;
        _valuesCache = [];
        _namesCache = [];

        foreach (var kv in mapping)
        {
            _valuesCache[kv.Value] = kv.Key;
            _namesCache[kv.Key] = JsonEncodedText.Encode(kv.Value, options.Encoder);
        }

        _inner = inner;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
