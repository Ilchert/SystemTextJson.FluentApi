using System.Collections.Frozen;
using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

public sealed class JsonModelBuilder
{
    private readonly Dictionary<Type, IEntityTypeBuilder> _configurations = [];

    public EntityTypeBuilder<TEntity> Entity<TEntity>()
    {
        if (_configurations.TryGetValue(typeof(TEntity), out var entityTypeBuilder))
            return (EntityTypeBuilder<TEntity>)entityTypeBuilder;

        var newBuilder = new EntityTypeBuilder<TEntity>(this);
        _configurations[typeof(TEntity)] = newBuilder;
        return newBuilder;
    }

    public Action<JsonTypeInfo> Build()
    {
        var config = _configurations.ToFrozenDictionary(p => p.Key, p => p.Value.Build());

        return p => config.GetValueOrDefault(p.Type)?.Invoke(p);
    }
}