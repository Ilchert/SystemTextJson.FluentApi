using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
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

    public EntityTypeBuilder<TEntity> IsUnmappedMemberDisallowed()
    {
        Configure(p => p.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow);
        return this;
    }

    public EntityTypeBuilder<TEntity> HasTypeDiscriminator(string typeDiscriminator)
    {
        Configure(p =>
        {
            p.PolymorphismOptions ??= new JsonPolymorphismOptions();
            p.PolymorphismOptions.TypeDiscriminatorPropertyName = typeDiscriminator;
        });
        return this;
    }

    public EntityTypeBuilder<TEntity> HasDerivedType<T>()
    {
        Configure(p =>
        {
            p.PolymorphismOptions ??= new JsonPolymorphismOptions();
            p.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(typeof(T)));
        });
        return this;
    }

    public EntityTypeBuilder<TEntity> HasDerivedType<T>(string typeDiscriminator)
    {
        Configure(p =>
        {
            p.PolymorphismOptions ??= new JsonPolymorphismOptions();
            p.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(typeof(T), typeDiscriminator));
        });
        return this;
    }

    public EntityTypeBuilder<TEntity> HasDerivedType<T>(int typeDiscriminator)
    {
        Configure(p =>
        {
            p.PolymorphismOptions ??= new JsonPolymorphismOptions();
            p.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(typeof(T), typeDiscriminator));
        });
        return this;
    }

    public EntityTypeBuilder<TEntity> HasDerivedTypesFromAssembly(Assembly assembly, Func<Type, string>? discriminatorFormatter = null)
    {
        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(p => p != null).ToArray()!;
        }

        types = types.Where(p => p != null && p.IsAssignableTo(typeof(TEntity))).ToArray();
        if (types.Length == 0)
            return this;

        if (discriminatorFormatter is not null)
        {
            Configure(p =>
            {
                p.PolymorphismOptions ??= new JsonPolymorphismOptions();
                foreach (var type in types)
                    p.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(type, discriminatorFormatter(type)));
            });
        }
        else
        {
            Configure(p =>
            {
                p.PolymorphismOptions ??= new JsonPolymorphismOptions();
                foreach (var type in types)
                    p.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(type));
            });
        }
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
                var propName = prop.GetMemberName();
                if (propertyConfiguration.TryGetValue(propName, out var propertyConfig))
                    propertyConfig(prop);
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
