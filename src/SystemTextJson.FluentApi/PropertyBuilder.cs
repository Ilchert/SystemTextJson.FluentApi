using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

public class PropertyBuilder<TEntity, TProperty>(string propertyName, EntityTypeBuilder<TEntity> entityTypeBuilder) : IPropertyBuilder
{
    private readonly List<Action<JsonPropertyInfo>> _propertyConfigurations = [];

    public PropertyBuilder<TEntity, TProperty> Configure(Action<JsonPropertyInfo> configureAction)
    {
        _propertyConfigurations.Add(configureAction);
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> HasName(string name)
    {
        Configure(p => p.Name = name);
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> HasConverter(JsonConverter converter)
    {
        Configure(p => p.CustomConverter = converter);
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> IsExtensionData()
    {
        if (typeof(TProperty).IsAssignableTo(typeof(IDictionary<string, JsonElement>)) ||
            typeof(TProperty).IsAssignableTo(typeof(IDictionary<string, object>)) ||
            typeof(TProperty) == typeof(JsonObject))
        {
            Configure(p => p.IsExtensionData = true);
        }
        else
            throw new InvalidOperationException($"The extension data property {typeof(TEntity)}{propertyName} is invalid. It must implement 'IDictionary<string, JsonElement>' or 'IDictionary<string, object>', or be 'JsonObject'.");

        return this;
    }

    public PropertyBuilder<TEntity, TProperty> HasHumberHandling(JsonNumberHandling numberHandling)
    {
        Configure(p => p.NumberHandling = numberHandling);
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> IsPopulated()
    {
        Configure(p => p.ObjectCreationHandling = JsonObjectCreationHandling.Populate);
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> HasOrder(int order)
    {
        Configure(p => p.Order = order);
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> IsRequired()
    {
        Configure(p => p.IsRequired = true);
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
    {
        return entityTypeBuilder.Property(propertyExpression);
    }

    Action<JsonPropertyInfo> IPropertyBuilder.Build()
    {
        var configurations = _propertyConfigurations.ToArray();
        return p =>
        {
            foreach (var cfg in configurations)
                cfg(p);
        };
    }
}
