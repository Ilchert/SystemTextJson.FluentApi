using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions ConfigureDefaultTypeResolver(this JsonSerializerOptions options, Action<JsonModelBuilder> configureAction)
    {
        var modelBuilder = new JsonModelBuilder();
        configureAction(modelBuilder);
        var action = modelBuilder.Build();
        options.TypeInfoResolver ??= new DefaultJsonTypeInfoResolver();
        options.TypeInfoResolver = options.TypeInfoResolver.WithAddedModifier(action);
        return options;
    }

    public static JsonSerializerOptions ConfigureEnumValues<TEnum>(this JsonSerializerOptions options,
        IReadOnlyDictionary<TEnum, string>? mapping = null,
        JsonNamingPolicy? namingPolicy = null,
        bool allowIntegerValues = true)
        where TEnum : struct, Enum
    {
        var innerConverterFactory = new JsonStringEnumConverter<TEnum>(namingPolicy, allowIntegerValues);
        var innerConverter = (JsonConverter<TEnum>)innerConverterFactory.CreateConverter(typeof(TEnum), options)!;

        JsonConverter<TEnum> converter = mapping is null ? 
            new CustomEnumConverter<TEnum>(innerConverter, options) :
            new CustomEnumConverter<TEnum>(innerConverter, options, mapping);

        options.Converters.Add(converter);
        return options;
    }
}
