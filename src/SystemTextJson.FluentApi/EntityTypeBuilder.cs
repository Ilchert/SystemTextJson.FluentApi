using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

public class EntityTypeBuilder<TEntity>(JsonModelBuilder modelBuilder) : IEntityTypeBuilder<TEntity>
{
    public Type EntityType => typeof(TEntity);
    public IList<Action<JsonTypeInfo>> JsonTypeInfoActions { get; } = [];
    public IList<IPropertyBuilder> PropertyBuilders { get; } = [];

    public JsonModelBuilder ModelBuilder { get; } = modelBuilder;

    public EntityTypeBuilder<TEntity> ConfigureTyped(Action<JsonTypeInfo<TEntity>> configureAction)
    {
        JsonTypeInfoActions.Add(p => configureAction((JsonTypeInfo<TEntity>)p));
        return this;
    }

    public EntityTypeBuilder<TEntity> HasDerivedType<T>() where T : TEntity =>
        this.HasDerivedType(typeof(T));

    public EntityTypeBuilder<TEntity> HasDerivedType<T>(string typeDiscriminator) where T : TEntity =>
        this.HasDerivedType(typeof(T), typeDiscriminator);

    public EntityTypeBuilder<TEntity> HasDerivedType<T>(int typeDiscriminator) where T : TEntity =>
        this.HasDerivedType(typeof(T), typeDiscriminator);

    Action<JsonTypeInfo> IEntityTypeBuilder.Build()
    {
        var memberProperties = PropertyBuilders.OfType<IMemberPropertyBuilder>()
            .GroupBy(p => p.MemberInfo, p => p.Build())
            .Select(p => (p.Key, Value: (Action<JsonPropertyInfo>)Delegate.Combine(p.ToArray())!))
            .ToDictionary(p => p.Key, p => p.Value);

        var typeConfigurations = JsonTypeInfoActions.ToArray();
        return p =>
        {
            foreach (var tc in typeConfigurations)
                tc(p);

            foreach (var prop in p.Properties)
            {
                var mi = prop.GetMemberInfo();
                if (mi != null && memberProperties.TryGetValue(mi, out var propertyConfig))
                    propertyConfig(prop);
            }
        };
    }
}
