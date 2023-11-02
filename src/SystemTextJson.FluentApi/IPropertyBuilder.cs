using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

internal interface IPropertyBuilder
{
    Action<JsonPropertyInfo> Build();
}
