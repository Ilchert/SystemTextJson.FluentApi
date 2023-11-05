using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;
public class VirtualPropertyBuilder<TEntity, TProperty>(string name, IEntityTypeBuilder<TEntity> entityTypeBuilder) : IPropertyBuilder<TEntity, TProperty>
{
    public IEntityTypeBuilder<TEntity> EntityTypeBuilder => entityTypeBuilder;

    IEntityTypeBuilder IPropertyBuilder.EntityTypeBuilder => entityTypeBuilder;

    public string Name => name;

    public IList<Action<JsonPropertyInfo>> JsonPropertyInfoActions { get; } = [];

    Action<JsonPropertyInfo> IPropertyBuilder.Build()
    {
        var configurations = JsonPropertyInfoActions.ToArray();
        return p =>
        {
            foreach (var cfg in configurations)
                cfg(p);
        };
    }
}
