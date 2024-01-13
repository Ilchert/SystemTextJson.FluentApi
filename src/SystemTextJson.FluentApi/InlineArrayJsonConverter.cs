
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static SystemTextJson.FluentApi.SerializationHelpers;
namespace SystemTextJson.FluentApi;
#if NET8_0_OR_GREATER

public class InlineArrayJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.GetCustomAttribute<InlineArrayAttribute>() != null;

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var attribute = typeToConvert.GetCustomAttribute<InlineArrayAttribute>();
        if (attribute is null)
            return null;

        var length = attribute.Length;
        var itemType = typeToConvert.GetFields()[0].FieldType; // inline array can have only one field

        var converterType = typeof(ConcreteInlineArrayJsonConverter<,>).MakeGenericType(typeToConvert, itemType);
        return (JsonConverter)Activator.CreateInstance(converterType, length, options)!;
    }

    private class ConcreteInlineArrayJsonConverter<TStruct, TItem>(int length, JsonSerializerOptions options) : JsonConverter<TStruct>
        where TStruct : struct
    {
        private readonly JsonConverter<TItem?> _itemConverter = (JsonConverter<TItem?>)options.GetConverter(typeof(TItem));
        public override TStruct Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Start token must be '['.");

            reader.Read();

            var result = default(TStruct);
            var span = MemoryMarshal.CreateSpan(ref Unsafe.As<TStruct, TItem?>(ref result), length);
            for (var i = 0; i < span.Length; i++)
                span[i] = ReadValue(_itemConverter, ref reader, options);

            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException("Expected end token ']'.");

            return result;
        }

        public override void Write(Utf8JsonWriter writer, TStruct value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            var span = MemoryMarshal.CreateSpan(ref Unsafe.As<TStruct, TItem?>(ref value), length);
            for (var i = 0; i < span.Length; i++)
                WriteValue(_itemConverter, writer, span[i], options);

            writer.WriteEndArray();
        }
    }

}
#endif
