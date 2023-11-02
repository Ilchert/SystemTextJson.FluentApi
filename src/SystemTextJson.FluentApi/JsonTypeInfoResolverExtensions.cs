﻿using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

public static class JsonTypeInfoResolverExtensions
{
    public static IJsonTypeInfoResolver ConfigureTypes(this IJsonTypeInfoResolver resolver, Action<JsonModelBuilder> configureAction)
    {
        var modelBuilder = new JsonModelBuilder();
        configureAction(modelBuilder);
        var action = modelBuilder.Build();
        return resolver.WithAddedModifier(action);
    }

    public static JsonSerializerOptions ConfigureDefaultTypeResolver(this JsonSerializerOptions options, Action<JsonModelBuilder> configureAction)
    {
        var modelBuilder = new JsonModelBuilder();
        configureAction(modelBuilder);
        var action = modelBuilder.Build();
        options.TypeInfoResolver ??= new DefaultJsonTypeInfoResolver();
        options.TypeInfoResolver = options.TypeInfoResolver.WithAddedModifier(action);
        return options;
    }
}
