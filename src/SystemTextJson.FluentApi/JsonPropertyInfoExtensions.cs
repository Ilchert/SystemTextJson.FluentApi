using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

internal static class JsonPropertyInfoExtensions
{
    public static MemberInfo? GetMemberInfo(this JsonPropertyInfo jsonProp) =>
        jsonProp.AttributeProvider as MemberInfo;
}
