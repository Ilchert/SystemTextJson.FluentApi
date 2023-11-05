using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

public static class EntityTypeBuilderExtensions
{
    public static T Configure<T>(this T builder, Action<JsonTypeInfo> configureAction) where T : IEntityTypeBuilder
    {
        builder.JsonTypeInfoActions.Add(configureAction);
        return builder;
    }

    public static T IsUnmappedMemberDisallowed<T>(this T builder) where T : IEntityTypeBuilder =>
        builder.Configure(p => p.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow);

    public static T HasTypeDiscriminator<T>(this T builder, string typeDiscriminator) where T : IEntityTypeBuilder =>
        builder.Configure(p =>
        {
            p.PolymorphismOptions ??= new JsonPolymorphismOptions();
            p.PolymorphismOptions.TypeDiscriminatorPropertyName = typeDiscriminator;
        });

    public static T HasDerivedType<T>(this T builder, Type derivedType) where T : IEntityTypeBuilder =>
        builder.HasDerivedType(new JsonDerivedType(derivedType));

    public static T HasDerivedType<T>(this T builder, Type derivedType, string typeDiscriminator) where T : IEntityTypeBuilder =>
        builder.HasDerivedType(new JsonDerivedType(derivedType, typeDiscriminator));

    public static T HasDerivedType<T>(this T builder, Type derivedType, int typeDiscriminator) where T : IEntityTypeBuilder =>
        builder.HasDerivedType(new JsonDerivedType(derivedType, typeDiscriminator));

    public static T HasDerivedType<T>(this T builder, JsonDerivedType derivedType) where T : IEntityTypeBuilder
    {
        return builder.Configure(p =>
        {
            p.PolymorphismOptions ??= new JsonPolymorphismOptions();
            p.PolymorphismOptions.DerivedTypes.Add(derivedType);
        });
    }

    public static T HasDerivedType<T>(this T builder, params JsonDerivedType[] derivedTypes) where T : IEntityTypeBuilder
    {
        return builder.Configure(p =>
        {
            p.PolymorphismOptions ??= new JsonPolymorphismOptions();
            foreach (var derivedType in derivedTypes)
                p.PolymorphismOptions.DerivedTypes.Add(derivedType);
        });
    }

    public static T HasDerivedTypesFromAssembly<T>(this T builder, Assembly assembly, Func<Type, string>? discriminatorFormatter = null) where T : IEntityTypeBuilder
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

        types = types.Where(p => p != null && builder.EntityType.IsAssignableFrom(p)).ToArray();
        if (types.Length == 0)
            return builder;

        var jsonTypes = discriminatorFormatter is null ?
            types.Select(p => new JsonDerivedType(p)) :
            types.Select(p => new JsonDerivedType(p, discriminatorFormatter(p)));

        builder.HasDerivedType(jsonTypes.ToArray());

        return builder;
    }

    public static VirtualPropertyBuilder<TEntity, TProperty> VirtualProperty<TEntity, TProperty>(this IEntityTypeBuilder<TEntity> builder, string name, Func<TEntity?, TProperty> compute)
    {
        builder.Configure(p =>
        {
            var propInfo = p.CreateJsonPropertyInfo(typeof(TProperty), name);
            propInfo.Get = (o) => compute((TEntity)o);
            p.Properties.Add(propInfo);
        });
        return new(name, builder);
    }


    public static MemberPropertyBuilder<TEntity, TProperty> Property<TEntity, TProperty>(this IEntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> propertyExpression)
    {
        var mi = GetMemberInfo(propertyExpression);
        var newBuilder = new MemberPropertyBuilder<TEntity, TProperty>(mi, builder);
        builder.PropertyBuilders.Add(newBuilder);
        return newBuilder;
    }

    private static MemberInfo GetMemberInfo<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
    {
        if (propertyExpression.Body is not MemberExpression member)
            throw new ArgumentException($"Expression '{propertyExpression}' refers to a method, not a property.");

        if (member.Member is not (PropertyInfo or FieldInfo))
            throw new ArgumentException($"Expression '{propertyExpression}' refers to a field, not a property.");

        return member.Member;
    }
}
