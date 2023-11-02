using System.Collections.Frozen;
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

    public JsonModelBuilder RespectNullableReferenceType()
    {
        var nullabilityInfoContext = new NullabilityInfoContext();

        Configure(p =>
        {

            foreach (var prop in p.Properties)
            {
                if (prop.Set is null)
                    continue;

                var memberInfo = prop.GetMemberInfo(p.Type);
                var nullState = memberInfo switch
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
                            throw new JsonException($"Can not null to {propertyName}.");

                        set(o, value);
                    };
                }
            }
        });
        return this;
    }

    public Action<JsonTypeInfo> Build()
    {
        var config = _configurations.ToFrozenDictionary(p => p.Key, p => p.Value.Build());
        var typeConfig = _typeConfigurations.ToArray();
        return p =>
        {
            foreach (var cfg in typeConfig)
                cfg(p);

            config.GetValueOrDefault(p.Type)?.Invoke(p);
        };
    }
}