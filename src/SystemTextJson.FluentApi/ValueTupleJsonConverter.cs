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

        return (JsonConverter?)Activator.CreateInstance(converterType.MakeGenericType(typeToConvert.GenericTypeArguments), [options]);
    }

    private abstract class ValueTupleConverterBase<TTuple> : JsonConverter<TTuple>
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

        protected static T? ReadValue<T>(JsonConverter<T?> converter, ref Utf8JsonReader reader, JsonSerializerOptions options)
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

        protected internal abstract TTuple ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);

        public override void Write(Utf8JsonWriter writer, TTuple value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            WriteTuple(writer, value, options);
            writer.WriteEndArray();
        }

        protected internal abstract void WriteTuple(Utf8JsonWriter writer, TTuple value, JsonSerializerOptions options);

        protected static void WriteValue<T>(JsonConverter<T?> converter, Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value is null && !converter.HandleNull)
                writer.WriteNullValue();
            else
                converter.Write(writer, value, options);
        }
    }

    private class ValueTupleConverter<T1>(JsonSerializerOptions options) : ValueTupleConverterBase<ValueTuple<T1?>>
    {
        private readonly JsonConverter<T1?> _converter1 = (JsonConverter<T1?>)options.GetConverter(typeof(T1));
        protected internal override ValueTuple<T1?> ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue(_converter1, ref reader, options);
            return ValueTuple.Create(value1);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, ValueTuple<T1?> value, JsonSerializerOptions options)
        {
            WriteValue(_converter1, writer, value.Item1, options);
        }
    }

    private class ValueTupleConverter<T1, T2>(JsonSerializerOptions options) : ValueTupleConverterBase<ValueTuple<T1?, T2?>>
    {
        private readonly JsonConverter<T1?> _converter1 = (JsonConverter<T1?>)options.GetConverter(typeof(T1));
        private readonly JsonConverter<T2?> _converter2 = (JsonConverter<T2?>)options.GetConverter(typeof(T2));

        protected internal override (T1?, T2?) ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue(_converter1, ref reader, options);
            var value2 = ReadValue(_converter2, ref reader, options);
            return (value1, value2);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, (T1?, T2?) value, JsonSerializerOptions options)
        {
            WriteValue(_converter1, writer, value.Item1, options);
            WriteValue(_converter2, writer, value.Item2, options);
        }
    }

    private class ValueTupleConverter<T1, T2, T3>(JsonSerializerOptions options) : ValueTupleConverterBase<ValueTuple<T1?, T2?, T3?>>
    {
        private readonly JsonConverter<T1?> _converter1 = (JsonConverter<T1?>)options.GetConverter(typeof(T1));
        private readonly JsonConverter<T2?> _converter2 = (JsonConverter<T2?>)options.GetConverter(typeof(T2));
        private readonly JsonConverter<T3?> _converter3 = (JsonConverter<T3?>)options.GetConverter(typeof(T3));
        protected internal override (T1?, T2?, T3?) ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue(_converter1, ref reader, options);
            var value2 = ReadValue(_converter2, ref reader, options);
            var value3 = ReadValue(_converter3, ref reader, options);

            return (value1, value2, value3);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, (T1?, T2?, T3?) value, JsonSerializerOptions options)
        {
            WriteValue(_converter1, writer, value.Item1, options);
            WriteValue(_converter2, writer, value.Item2, options);
            WriteValue(_converter3, writer, value.Item3, options);
        }
    }

    private class ValueTupleConverter<T1, T2, T3, T4>(JsonSerializerOptions options) : ValueTupleConverterBase<ValueTuple<T1?, T2?, T3?, T4?>>
    {
        private readonly JsonConverter<T1?> _converter1 = (JsonConverter<T1?>)options.GetConverter(typeof(T1));
        private readonly JsonConverter<T2?> _converter2 = (JsonConverter<T2?>)options.GetConverter(typeof(T2));
        private readonly JsonConverter<T3?> _converter3 = (JsonConverter<T3?>)options.GetConverter(typeof(T3));
        private readonly JsonConverter<T4?> _converter4 = (JsonConverter<T4?>)options.GetConverter(typeof(T4));

        protected internal override (T1?, T2?, T3?, T4?) ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue(_converter1, ref reader, options);
            var value2 = ReadValue(_converter2, ref reader, options);
            var value3 = ReadValue(_converter3, ref reader, options);
            var value4 = ReadValue(_converter4, ref reader, options);

            return (value1, value2, value3, value4);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, (T1?, T2?, T3?, T4?) value, JsonSerializerOptions options)
        {
            WriteValue(_converter1, writer, value.Item1, options);
            WriteValue(_converter2, writer, value.Item2, options);
            WriteValue(_converter3, writer, value.Item3, options);
            WriteValue(_converter4, writer, value.Item4, options);
        }
    }

    private class ValueTupleConverter<T1, T2, T3, T4, T5>(JsonSerializerOptions options) : ValueTupleConverterBase<ValueTuple<T1?, T2?, T3?, T4?, T5?>>
    {
        private readonly JsonConverter<T1?> _converter1 = (JsonConverter<T1?>)options.GetConverter(typeof(T1));
        private readonly JsonConverter<T2?> _converter2 = (JsonConverter<T2?>)options.GetConverter(typeof(T2));
        private readonly JsonConverter<T3?> _converter3 = (JsonConverter<T3?>)options.GetConverter(typeof(T3));
        private readonly JsonConverter<T4?> _converter4 = (JsonConverter<T4?>)options.GetConverter(typeof(T4));
        private readonly JsonConverter<T5?> _converter5 = (JsonConverter<T5?>)options.GetConverter(typeof(T5));

        protected internal override (T1?, T2?, T3?, T4?, T5?) ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue(_converter1, ref reader, options);
            var value2 = ReadValue(_converter2, ref reader, options);
            var value3 = ReadValue(_converter3, ref reader, options);
            var value4 = ReadValue(_converter4, ref reader, options);
            var value5 = ReadValue(_converter5, ref reader, options);

            return (value1, value2, value3, value4, value5);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, (T1?, T2?, T3?, T4?, T5?) value, JsonSerializerOptions options)
        {
            WriteValue(_converter1, writer, value.Item1, options);
            WriteValue(_converter2, writer, value.Item2, options);
            WriteValue(_converter3, writer, value.Item3, options);
            WriteValue(_converter4, writer, value.Item4, options);
            WriteValue(_converter5, writer, value.Item5, options);
        }
    }

    private class ValueTupleConverter<T1, T2, T3, T4, T5, T6>(JsonSerializerOptions options) : ValueTupleConverterBase<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>
    {
        private readonly JsonConverter<T1?> _converter1 = (JsonConverter<T1?>)options.GetConverter(typeof(T1));
        private readonly JsonConverter<T2?> _converter2 = (JsonConverter<T2?>)options.GetConverter(typeof(T2));
        private readonly JsonConverter<T3?> _converter3 = (JsonConverter<T3?>)options.GetConverter(typeof(T3));
        private readonly JsonConverter<T4?> _converter4 = (JsonConverter<T4?>)options.GetConverter(typeof(T4));
        private readonly JsonConverter<T5?> _converter5 = (JsonConverter<T5?>)options.GetConverter(typeof(T5));
        private readonly JsonConverter<T6?> _converter6 = (JsonConverter<T6?>)options.GetConverter(typeof(T6));

        protected internal override (T1?, T2?, T3?, T4?, T5?, T6?) ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue(_converter1, ref reader, options);
            var value2 = ReadValue(_converter2, ref reader, options);
            var value3 = ReadValue(_converter3, ref reader, options);
            var value4 = ReadValue(_converter4, ref reader, options);
            var value5 = ReadValue(_converter5, ref reader, options);
            var value6 = ReadValue(_converter6, ref reader, options);

            return (value1, value2, value3, value4, value5, value6);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, (T1?, T2?, T3?, T4?, T5?, T6?) value, JsonSerializerOptions options)
        {
            WriteValue(_converter1, writer, value.Item1, options);
            WriteValue(_converter2, writer, value.Item2, options);
            WriteValue(_converter3, writer, value.Item3, options);
            WriteValue(_converter4, writer, value.Item4, options);
            WriteValue(_converter5, writer, value.Item5, options);
            WriteValue(_converter6, writer, value.Item6, options);
        }
    }

    private class ValueTupleConverter<T1, T2, T3, T4, T5, T6, T7>(JsonSerializerOptions options) : ValueTupleConverterBase<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>
    {
        private readonly JsonConverter<T1?> _converter1 = (JsonConverter<T1?>)options.GetConverter(typeof(T1));
        private readonly JsonConverter<T2?> _converter2 = (JsonConverter<T2?>)options.GetConverter(typeof(T2));
        private readonly JsonConverter<T3?> _converter3 = (JsonConverter<T3?>)options.GetConverter(typeof(T3));
        private readonly JsonConverter<T4?> _converter4 = (JsonConverter<T4?>)options.GetConverter(typeof(T4));
        private readonly JsonConverter<T5?> _converter5 = (JsonConverter<T5?>)options.GetConverter(typeof(T5));
        private readonly JsonConverter<T6?> _converter6 = (JsonConverter<T6?>)options.GetConverter(typeof(T6));
        private readonly JsonConverter<T7?> _converter7 = (JsonConverter<T7?>)options.GetConverter(typeof(T7));

        protected internal override (T1?, T2?, T3?, T4?, T5?, T6?, T7?) ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue(_converter1, ref reader, options);
            var value2 = ReadValue(_converter2, ref reader, options);
            var value3 = ReadValue(_converter3, ref reader, options);
            var value4 = ReadValue(_converter4, ref reader, options);
            var value5 = ReadValue(_converter5, ref reader, options);
            var value6 = ReadValue(_converter6, ref reader, options);
            var value7 = ReadValue(_converter7, ref reader, options);

            return (value1, value2, value3, value4, value5, value6, value7);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, (T1?, T2?, T3?, T4?, T5?, T6?, T7?) value, JsonSerializerOptions options)
        {
            WriteValue(_converter1, writer, value.Item1, options);
            WriteValue(_converter2, writer, value.Item2, options);
            WriteValue(_converter3, writer, value.Item3, options);
            WriteValue(_converter4, writer, value.Item4, options);
            WriteValue(_converter5, writer, value.Item5, options);
            WriteValue(_converter6, writer, value.Item6, options);
            WriteValue(_converter7, writer, value.Item7, options);
        }
    }

    private class ValueTupleConverter<T1, T2, T3, T4, T5, T6, T7, TRest>(JsonSerializerOptions options) : ValueTupleConverterBase<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>
        where TRest : struct
    {
        private readonly JsonConverter<T1?> _converter1 = (JsonConverter<T1?>)options.GetConverter(typeof(T1));
        private readonly JsonConverter<T2?> _converter2 = (JsonConverter<T2?>)options.GetConverter(typeof(T2));
        private readonly JsonConverter<T3?> _converter3 = (JsonConverter<T3?>)options.GetConverter(typeof(T3));
        private readonly JsonConverter<T4?> _converter4 = (JsonConverter<T4?>)options.GetConverter(typeof(T4));
        private readonly JsonConverter<T5?> _converter5 = (JsonConverter<T5?>)options.GetConverter(typeof(T5));
        private readonly JsonConverter<T6?> _converter6 = (JsonConverter<T6?>)options.GetConverter(typeof(T6));
        private readonly JsonConverter<T7?> _converter7 = (JsonConverter<T7?>)options.GetConverter(typeof(T7));
        private readonly ValueTupleConverterBase<TRest> _converterRest = (ValueTupleConverterBase<TRest>)options.GetConverter(typeof(TRest));

        protected internal override ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> ReadTuple(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value1 = ReadValue(_converter1, ref reader, options);
            var value2 = ReadValue(_converter2, ref reader, options);
            var value3 = ReadValue(_converter3, ref reader, options);
            var value4 = ReadValue(_converter4, ref reader, options);
            var value5 = ReadValue(_converter5, ref reader, options);
            var value6 = ReadValue(_converter6, ref reader, options);
            var value7 = ReadValue(_converter7, ref reader, options);

            var restValue = _converterRest.ReadTuple(ref reader, typeof(TRest), options);

            return new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>(value1, value2, value3, value4, value5, value6, value7, restValue);
        }

        protected internal override void WriteTuple(Utf8JsonWriter writer, ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> value, JsonSerializerOptions options)
        {
            WriteValue(_converter1, writer, value.Item1, options);
            WriteValue(_converter2, writer, value.Item2, options);
            WriteValue(_converter3, writer, value.Item3, options);
            WriteValue(_converter4, writer, value.Item4, options);
            WriteValue(_converter5, writer, value.Item5, options);
            WriteValue(_converter6, writer, value.Item6, options);
            WriteValue(_converter7, writer, value.Item7, options);

            _converterRest.WriteTuple(writer, value.Rest, options);
        }
    }
}
