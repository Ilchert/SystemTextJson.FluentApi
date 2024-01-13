using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace SystemTextJson.FluentApi;
internal class SerializationHelpers
{
    public static T? ReadValue<T>(JsonConverter<T?> converter, ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var value = default(T);
        if (reader.TokenType == JsonTokenType.Null && !converter.HandleNull)
        {
            if (value is not null)
                throw new JsonException("Expected not null value.");
        }
        else
        {
            value = converter.Read(ref reader, typeof(T), options);
        }
        reader.Read();
        return value;
    }

    public static void WriteValue<T>(JsonConverter<T?> converter, Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null && !converter.HandleNull)
            writer.WriteNullValue();
        else
            converter.Write(writer, value, options);
    }
}
