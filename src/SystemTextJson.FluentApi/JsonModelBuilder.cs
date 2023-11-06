using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

public sealed class JsonModelBuilder
{
    private readonly Dictionary<Type, IEntityTypeBuilder> _configurations = [];
    private readonly List<Action<JsonTypeInfo>> _typeConfigurations = [];

    public EntityTypeBuilder<TEntity> Entity<TEntity>()
    {
        if (_configurations.TryGetValue(typeof(TEntity), out var entityTypeBuilder))
            return (EntityTypeBuilder<TEntity>)entityTypeBuilder;

        var newBuilder = new EntityTypeBuilder<TEntity>(this);
        _configurations[typeof(TEntity)] = newBuilder;
        return newBuilder;
    }

    public JsonModelBuilder Configure(Action<JsonTypeInfo> configureAction)
    {
        _typeConfigurations.Add(configureAction);
        return this;
    }

    public JsonModelBuilder TrackChangedProperties()
    {
        Configure(p =>
        {
            if (!typeof(IHaveChangedProperties).IsAssignableFrom(p.Type))
                return;

            for (var i = 0; i < p.Properties.Count; i++)
            {
                var property = p.Properties[i];
                if (property.GetMemberInfo() is not { } mi)
                    continue;

                if (mi.Name == nameof(IHaveChangedProperties.ChangedProperties))
                {
                    p.Properties.RemoveAt(i);
                    i--;
                    continue;
                }

                if (property.Set is not { } set)
                    continue;

                var realPropertyName = mi.Name;

                property.Set = (o, value) =>
                {
                    var cp = o as IHaveChangedProperties;
                    cp?.ChangedProperties?.Add(realPropertyName);
                    set(o, value);
                };
            }
        });


        return this;
    }

    public JsonModelBuilder SerializeOnlyChangedProperties()
    {
        Configure(p =>
        {
            if (!typeof(IHaveChangedProperties).IsAssignableFrom(p.Type))
                return;

            for (var i = 0; i < p.Properties.Count; i++)
            {
                var property = p.Properties[i];
                if (property.GetMemberInfo() is not { } mi)
                    continue;

                if (mi.Name == nameof(IHaveChangedProperties.ChangedProperties))
                {
                    p.Properties.RemoveAt(i);
                    i--;
                    continue;
                }

                if (property.Get is not { })
                    continue;

                var realPropertyName = mi.Name;

                property.ShouldSerialize = (o, value) =>
                {
                    var cp = o as IHaveChangedProperties;
                    return cp?.ChangedProperties?.Contains(realPropertyName) == true;
                };
            }
        });


        return this;
    }


#if NET6_0_OR_GREATER

    public JsonModelBuilder RespectNullableReferenceType()
    {
        var nullabilityInfoContext = new NullabilityInfoContext();

        Configure(p =>
        {
            foreach (var prop in p.Properties)
            {
                if (prop.Set is null || !prop.PropertyType.IsClass)
                    continue;

                var nullState = prop.GetMemberInfo() switch
                {
                    PropertyInfo pi => nullabilityInfoContext.Create(pi).WriteState,
                    FieldInfo fi => nullabilityInfoContext.Create(fi).WriteState,
                    _ => NullabilityState.Unknown
                };

                if (nullState == NullabilityState.NotNull)
                {
                    var set = prop.Set;
                    var propertyName = prop.Name;
                    prop.Set = (o, value) =>
                    {
                        if (value is null)
                            throw new JsonException($"Can not set null to none nullable property {propertyName}.");

                        set(o, value);
                    };
                }
            }
        });
        return this;
    }

#endif
    public Action<JsonTypeInfo> Build()
    {
        var config = _configurations.ToDictionary(p => p.Key, p => p.Value.Build());
        var typeConfig = _typeConfigurations.ToArray();
        return p =>
        {
            foreach (var cfg in typeConfig)
                cfg(p);

            if (config.TryGetValue(p.Type, out var action))
                action(p);
        };
    }
}
