using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemTextJson.FluentApi;

internal class ObjectSerializer<T> : JsonConverter<T>
{
    public static JsonConverter<T> Instance = new ObjectSerializer<T>();

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new JsonException("Can not deserialize as object.");

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            var objectConverter = options.GetConverter(value.GetType());
            var mi = objectConverter.GetType().GetMethod(nameof(Write));
            mi.Invoke(objectConverter, new object[] { writer, value, options });
        }
    }
}
