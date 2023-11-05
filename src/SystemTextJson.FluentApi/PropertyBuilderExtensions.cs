using System.Linq.Expressions;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

public static class PropertyBuilderExtensions
{
    public static T Configure<T>(this T builder, Action<JsonPropertyInfo> configureAction) where T : IPropertyBuilder
    {
        builder.JsonPropertyInfoActions.Add(configureAction);
        return builder;
    }

    public static T HasName<T>(this T builder, string name) where T : IPropertyBuilder =>
       builder.Configure(p => p.Name = name);

    public static T HasConverter<T>(this T builder, JsonConverter converter) where T : IPropertyBuilder =>
      builder.Configure(p => p.CustomConverter = converter);

    public static T HasHumberHandling<T>(this T builder, JsonNumberHandling numberHandling) where T : IPropertyBuilder =>
      builder.Configure(p => p.NumberHandling = numberHandling);

    public static T HasOrder<T>(this T builder, int order) where T : IPropertyBuilder =>
      builder.Configure(p => p.Order = order);

    public static EntityTypeBuilder<TEntity> Entity<TEntity>(this IPropertyBuilder propertyBuilder) =>
        propertyBuilder.EntityTypeBuilder.ModelBuilder.Entity<TEntity>();

    public static MemberPropertyBuilder<TEntity, TOtherProperty> Property<TEntity, TProperty, TOtherProperty>(this IPropertyBuilder<TEntity, TProperty> builder, Expression<Func<TEntity, TOtherProperty>> propertyExpression) =>
       builder.EntityTypeBuilder.Property(propertyExpression);

    public static VirtualPropertyBuilder<TEntity, TOtherProperty> VirtualProperty<TEntity, TProperty, TOtherProperty>(this IPropertyBuilder<TEntity, TProperty> builder, string name, Func<TEntity?, TOtherProperty> compute) =>
      builder.EntityTypeBuilder.VirtualProperty(name, compute);
}
