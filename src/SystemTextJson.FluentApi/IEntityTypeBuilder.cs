using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

public interface IEntityTypeBuilder
{
    Type EntityType { get; }
    IList<Action<JsonTypeInfo>> JsonTypeInfoActions { get; }

    IList<IPropertyBuilder> PropertyBuilders { get; }

    JsonModelBuilder ModelBuilder { get; }

    internal Action<JsonTypeInfo> Build();
}

public interface IEntityTypeBuilder<TEntity> : IEntityTypeBuilder
{

}
