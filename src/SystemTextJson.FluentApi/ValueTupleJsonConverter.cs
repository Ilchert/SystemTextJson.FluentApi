using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemTextJson.FluentApi;
public class ValueTupleJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return !typeToConvert.IsClass &&
            typeToConvert.IsGenericType &&
            typeToConvert.GetGenericTypeDefinition() is { } genericType &&
            (
            genericType == typeof(ValueTuple<>) ||
            genericType == typeof(ValueTuple<,>) ||
            genericType == typeof(ValueTuple<,,>) ||
            genericType == typeof(ValueTuple<,,,>) ||
            genericType == typeof(ValueTuple<,,,,>) ||
            genericType == typeof(ValueTuple<,,,,,>) ||
            genericType == typeof(ValueTuple<,,,,,,>) ||
            genericType == typeof(ValueTuple<,,,,,,,>) ||
            genericType == typeof(ValueTuple<,,,,,,,>)
            );
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeToConvert.GenericTypeArguments.Length switch
        {
            1 => typeof(ValueTupleConverter<>),
            2 => typeof(ValueTupleConverter<,>),
            3 => typeof(ValueTupleConverter<,,>),
            4 => typeof(ValueTupleConverter<,,,>),
            5 => typeof(ValueTupleConverter<,,,,>),
            6 => typeof(ValueTupleConverter<,,,,,>),
            7 => typeof(ValueTupleConverter<,,,,,,>),
            8 => typeof(ValueTupleConverter<,,,,,,,>),
            _ => throw new ArgumentOutOfRangeException(nameof(typeToConvert))
        };

        return (JsonConverter?)Activator.CreateInstance(converterType.MakeGenericType(typeToConvert.GenericTypeArguments));
    }

    private abstract class TupleConverterBase<TTuple> : JsonConverter<TTuple>
        where TTuple : struct
    {
        public override TTuple Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Start token must be '['.");

            reader.Read();

            var result = ReadTuple(ref reader, typeToConvert, options);

            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException("Expected end token ']'.");

            return result;
        }

        protected static T? ReadValue<T>(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var converter = (JsonConverter<T>)options.GetConverter(typeof(T));
            var value = converter.Read(ref reader, typeof(T), options);
            reader.Read();
            return value;
        }
        
        protected internal abstract TTuple ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);

        public override void Write(Utf8JsonWriter writer, TTuple value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            WriteTuple(writer, value, options);
            writer.WriteEndArray();
        }

        protected internal abstract void WriteTuple(Utf8JsonWriter writer, TTuple value, JsonSerializerOptions options);

        protected static void WriteValue<T>(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var converter = (JsonConverter<T?>)options.GetConverter(typeof(T));

            if (value is null && !converter.HandleNull)
                writer.WriteNullValue();
            else
                converter.Write(writer, value, options);
        }
    }

    private class ValueTupleConverter<T1> : TupleConverterBase<ValueTuple<T1?>>
    {
        protected internal override ValueTuple<T1?> ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue<T1>(ref reader, options);
            return ValueTuple.Create(value1);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, ValueTuple<T1?> value, JsonSerializerOptions options)
        {
            WriteValue(writer, value.Item1, options);
        }
    }

    private class ValueTupleConverter<T1, T2> : TupleConverterBase<ValueTuple<T1?, T2?>>
    {
        protected internal override (T1?, T2?) ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue<T1>(ref reader, options);
            var value2 = ReadValue<T2>(ref reader, options);
            return (value1, value2);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, (T1?, T2?) value, JsonSerializerOptions options)
        {
            WriteValue(writer, value.Item1, options);
            WriteValue(writer, value.Item2, options);
        }
    }

    private class ValueTupleConverter<T1, T2, T3> : TupleConverterBase<ValueTuple<T1?, T2?, T3?>>
    {
        protected internal override (T1?, T2?, T3?) ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue<T1>(ref reader, options);
            var value2 = ReadValue<T2>(ref reader, options);
            var value3 = ReadValue<T3>(ref reader, options);

            return (value1, value2, value3);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, (T1?, T2?, T3?) value, JsonSerializerOptions options)
        {
            WriteValue(writer, value.Item1, options);
            WriteValue(writer, value.Item2, options);
            WriteValue(writer, value.Item3, options);
        }
    }

    private class ValueTupleConverter<T1, T2, T3, T4> : TupleConverterBase<ValueTuple<T1?, T2?, T3?, T4?>>
    {
        protected internal override (T1?, T2?, T3?, T4?) ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue<T1>(ref reader, options);
            var value2 = ReadValue<T2>(ref reader, options);
            var value3 = ReadValue<T3>(ref reader, options);
            var value4 = ReadValue<T4>(ref reader, options);

            return (value1, value2, value3, value4);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, (T1?, T2?, T3?, T4?) value, JsonSerializerOptions options)
        {
            WriteValue(writer, value.Item1, options);
            WriteValue(writer, value.Item2, options);
            WriteValue(writer, value.Item3, options);
            WriteValue(writer, value.Item4, options);
        }
    }

    private class ValueTupleConverter<T1, T2, T3, T4, T5> : TupleConverterBase<ValueTuple<T1?, T2?, T3?, T4?, T5?>>
    {
        protected internal override (T1?, T2?, T3?, T4?, T5?) ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue<T1>(ref reader, options);
            var value2 = ReadValue<T2>(ref reader, options);
            var value3 = ReadValue<T3>(ref reader, options);
            var value4 = ReadValue<T4>(ref reader, options);
            var value5 = ReadValue<T5>(ref reader, options);

            return (value1, value2, value3, value4, value5);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, (T1?, T2?, T3?, T4?, T5?) value, JsonSerializerOptions options)
        {
            WriteValue(writer, value.Item1, options);
            WriteValue(writer, value.Item2, options);
            WriteValue(writer, value.Item3, options);
            WriteValue(writer, value.Item4, options);
            WriteValue(writer, value.Item5, options);
        }
    }

    private class ValueTupleConverter<T1, T2, T3, T4, T5, T6> : TupleConverterBase<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>
    {
        protected internal override (T1?, T2?, T3?, T4?, T5?, T6?) ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue<T1>(ref reader, options);
            var value2 = ReadValue<T2>(ref reader, options);
            var value3 = ReadValue<T3>(ref reader, options);
            var value4 = ReadValue<T4>(ref reader, options);
            var value5 = ReadValue<T5>(ref reader, options);
            var value6 = ReadValue<T6>(ref reader, options);

            return (value1, value2, value3, value4, value5, value6);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, (T1?, T2?, T3?, T4?, T5?, T6?) value, JsonSerializerOptions options)
        {
            WriteValue(writer, value.Item1, options);
            WriteValue(writer, value.Item2, options);
            WriteValue(writer, value.Item3, options);
            WriteValue(writer, value.Item4, options);
            WriteValue(writer, value.Item5, options);
            WriteValue(writer, value.Item6, options);
        }
    }

    private class ValueTupleConverter<T1, T2, T3, T4, T5, T6, T7> : TupleConverterBase<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>
    {
        protected internal override (T1?, T2?, T3?, T4?, T5?, T6?, T7?) ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue<T1>(ref reader, options);
            var value2 = ReadValue<T2>(ref reader, options);
            var value3 = ReadValue<T3>(ref reader, options);
            var value4 = ReadValue<T4>(ref reader, options);
            var value5 = ReadValue<T5>(ref reader, options);
            var value6 = ReadValue<T6>(ref reader, options);
            var value7 = ReadValue<T7>(ref reader, options);

            return (value1, value2, value3, value4, value5, value6, value7);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, (T1?, T2?, T3?, T4?, T5?, T6?, T7?) value, JsonSerializerOptions options)
        {
            WriteValue(writer, value.Item1, options);
            WriteValue(writer, value.Item2, options);
            WriteValue(writer, value.Item3, options);
            WriteValue(writer, value.Item4, options);
            WriteValue(writer, value.Item5, options);
            WriteValue(writer, value.Item6, options);
            WriteValue(writer, value.Item7, options);
        }
    }

    private class ValueTupleConverter<T1, T2, T3, T4, T5, T6, T7, TRest> : TupleConverterBase<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>
        where TRest : struct
    {
        protected internal override ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue<T1>(ref reader, options);
            var value2 = ReadValue<T2>(ref reader, options);
            var value3 = ReadValue<T3>(ref reader, options);
            var value4 = ReadValue<T4>(ref reader, options);
            var value5 = ReadValue<T5>(ref reader, options);
            var value6 = ReadValue<T6>(ref reader, options);
            var value7 = ReadValue<T7>(ref reader, options);

            var converter = (TupleConverterBase<TRest>)options.GetConverter(typeof(TRest));
            var restValue = converter.ReadTuple(ref reader, typeof(TRest), options);

            return new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>(value1, value2, value3, value4, value5, value6, value7, restValue);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> value, JsonSerializerOptions options)
        {
            WriteValue(writer, value.Item1, options);
            WriteValue(writer, value.Item2, options);
            WriteValue(writer, value.Item3, options);
            WriteValue(writer, value.Item4, options);
            WriteValue(writer, value.Item5, options);
            WriteValue(writer, value.Item6, options);
            WriteValue(writer, value.Item7, options);

            var converter = (TupleConverterBase<TRest>)options.GetConverter(typeof(TRest));
            converter.WriteTuple(writer, value.Rest, options);
        }
    }
}
