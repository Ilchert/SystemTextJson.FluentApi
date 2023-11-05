using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

public class MemberPropertyBuilder<TEntity, TProperty>(MemberInfo memberInfo, IEntityTypeBuilder<TEntity> entityTypeBuilder) : IMemberPropertyBuilder, IPropertyBuilder<TEntity, TProperty>
{
    public string Name => MemberInfo.Name;
    public IList<Action<JsonPropertyInfo>> JsonPropertyInfoActions { get; } = [];
    public MemberInfo MemberInfo { get; } = memberInfo;
    IEntityTypeBuilder IPropertyBuilder.EntityTypeBuilder => entityTypeBuilder;
    public IEntityTypeBuilder<TEntity> EntityTypeBuilder => entityTypeBuilder;

    public MemberPropertyBuilder<TEntity, TProperty> IsExtensionData()
    {
        if (typeof(IDictionary<string, JsonElement>).IsAssignableFrom(typeof(TProperty)) ||
            typeof(IDictionary<string, object>).IsAssignableFrom(typeof(TProperty)) ||
            typeof(TProperty) == typeof(JsonObject))
        {
            this.Configure(p => p.IsExtensionData = true);
        }
        else
            throw new InvalidOperationException($"The extension data property {typeof(TEntity)}{MemberInfo} is invalid. It must implement 'IDictionary<string, JsonElement>' or 'IDictionary<string, object>', or be 'JsonObject'.");

        return this;
    }

    public MemberPropertyBuilder<TEntity, TProperty> IsPopulated()
    {
        this.Configure(p => p.ObjectCreationHandling = JsonObjectCreationHandling.Populate);
        return this;
    }

    public MemberPropertyBuilder<TEntity, TProperty> IsRequired()
    {
        this.Configure(p => p.IsRequired = true);
        return this;
    }

    public MemberPropertyBuilder<TEntity, TProperty> IsIgnored() // Add IsIgnoredIfNull/IfDefault
    {
        var mi = MemberInfo;

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

    public MemberPropertyBuilder<TEntity, TProperty> SerializeAsObject()
    {
        this.Configure(p => p.CustomConverter = ObjectSerializer<TProperty>.Instance);
        return this;
    }
    
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
