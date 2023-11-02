﻿using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

public sealed class EntityTypeBuilder<TEntity>(JsonModelBuilder modelBuilder) : IEntityTypeBuilder
{
    private readonly List<Action<JsonTypeInfo<TEntity>>> _typeConfigurations = [];
    private readonly Dictionary<string, IPropertyBuilder> _propertyConfiguration = [];
    internal readonly JsonModelBuilder ModelBuilder = modelBuilder;

    public EntityTypeBuilder<TEntity> Configure(Action<JsonTypeInfo<TEntity>> configureAction)
    {
        _typeConfigurations.Add(configureAction);
        return this;
    }

    public PropertyBuilder<TEntity, TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
    {
        var propertyName = GetPropertyName(propertyExpression);

        var newBuilder = new PropertyBuilder<TEntity, TProperty>(propertyName, this);
        _propertyConfiguration[propertyName] = newBuilder;
        return newBuilder;
    }

    public EntityTypeBuilder<TEntity> Ignore<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
    {
        var propertyName = GetPropertyName(propertyExpression);
        Configure(p =>
        {
            for (var i = 0; i < p.Properties.Count; i++)
            {
                if (CompareNames(p.Properties[i], propertyName))
                {
                    p.Properties.RemoveAt(i);
                    break;
                }
            }
        });
        return this;
    }


    private static string GetPropertyName<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
    {
        if (propertyExpression.Body is not MemberExpression member)
            throw new ArgumentException($"Expression '{propertyExpression}' refers to a method, not a property.");

        if (member.Member is not (PropertyInfo or FieldInfo))
            throw new ArgumentException($"Expression '{propertyExpression}' refers to a field, not a property.");

        return member.Member.Name;
    }

    Action<JsonTypeInfo> IEntityTypeBuilder.Build()
    {
        var propertyConfiguration = _propertyConfiguration.ToFrozenDictionary(p => p.Key, p => p.Value.Build());
        var typeConfigurations = _typeConfigurations.ToArray();
        return p =>
        {
            var typedInfo = (JsonTypeInfo<TEntity>)p;

            foreach (var tc in typeConfigurations)
                tc(typedInfo);

            foreach (var prop in p.Properties)
            {
                if (propertyConfiguration.TryGetValue(prop.Name, out var propertyConfig))
                {
                    propertyConfig(prop);
                }
                else if (p.Options.PropertyNamingPolicy is { } || p.Options.PropertyNameCaseInsensitive)
                {
                    foreach (var (name, action) in propertyConfiguration)
                    {
                        if (CompareNames(prop, name))
                        {
                            action(prop);
                            break;
                        }
                    }
                }
            }
        };
    }

    private static bool CompareNames(JsonPropertyInfo jsonProp, string name)
    {
        if (jsonProp.Name == name)
            return true;

        var convertedName = name;
        if (jsonProp.Options.PropertyNamingPolicy is { } namingPolicy)
            convertedName = namingPolicy.ConvertName(name);

        var comparationMode = jsonProp.Options.PropertyNameCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return convertedName.Equals(jsonProp.Name, comparationMode);
    }
}
