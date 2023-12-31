using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

public interface IPropertyBuilder
{
    string Name { get; }

    internal IList<Action<JsonPropertyInfo>> JsonPropertyInfoActions { get; }

    internal IEntityTypeBuilder EntityTypeBuilder { get; }

    internal Action<JsonPropertyInfo> Build();
}
public interface IPropertyBuilder<TEntity, TProperty> : IPropertyBuilder
{
    new IEntityTypeBuilder<TEntity> EntityTypeBuilder { get; }
}
