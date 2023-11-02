using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

internal interface IEntityTypeBuilder
{
    Action<JsonTypeInfo> Build();
}
