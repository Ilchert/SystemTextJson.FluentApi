using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

internal static class JsonPropertyInfoExtensions
{
    public static string GetMemberName(this JsonPropertyInfo jsonProp) =>
        jsonProp.AttributeProvider is MemberInfo mi ? mi.Name : jsonProp.Name;
    public static MemberInfo? GetMemberInfo(this JsonPropertyInfo jsonProp) =>
        jsonProp.AttributeProvider as MemberInfo;
}
