using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace SystemTextJson.FluentApi;

internal static class JsonPropertyInfoExtensions
{
    private static readonly PropertyInfo? s_getMemberNamePropertyInfo = typeof(JsonPropertyInfo).GetProperty("MemberName", BindingFlags.NonPublic | BindingFlags.Instance);

    public static string GetMemberName(this JsonPropertyInfo jsonProp) =>
        (string?)s_getMemberNamePropertyInfo?.GetValue(jsonProp) ?? jsonProp.Name;

    public static MemberInfo? GetMemberInfo(this JsonPropertyInfo jsonProp, Type type)
    {
        var memberName = jsonProp.GetMemberName();
        var members = type.GetMember(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var mi in members)
        {
            if (mi.MemberType is MemberTypes.Property or MemberTypes.Field)
                return mi;
        }
        return null;
    }
}
