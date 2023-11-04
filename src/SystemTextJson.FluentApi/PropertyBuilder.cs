using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

public class PropertyBuilder<TEntity, TProperty>(MemberInfo memberInfo, EntityTypeBuilder<TEntity> entityTypeBuilder) : IPropertyBuilder
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
        if (typeof(IDictionary<string, JsonElement>).IsAssignableFrom(typeof(TProperty)) ||
            typeof(IDictionary<string, object>).IsAssignableFrom(typeof(TProperty)) ||
            typeof(TProperty) == typeof(JsonObject))
        {
            Configure(p => p.IsExtensionData = true);
        }
        else
            throw new InvalidOperationException($"The extension data property {typeof(TEntity)}{memberInfo} is invalid. It must implement 'IDictionary<string, JsonElement>' or 'IDictionary<string, object>', or be 'JsonObject'.");

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

    public PropertyBuilder<TEntity, TProperty> IsIgnored()
    {
        var mi = memberInfo;

        entityTypeBuilder.Configure(p =>
        {
            for (var i = 0; i < p.Properties.Count; i++)
            {
                if (p.Properties[i].GetMemberInfo() == mi)
                {
                    p.Properties.RemoveAt(i);
                    break;
                }
            }
        });
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> SerializeAsObject()
    {
        Configure(p => p.CustomConverter = ObjectSerializer<TProperty>.Instance);
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression) =>
        entityTypeBuilder.Property(propertyExpression);

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
