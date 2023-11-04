using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace SystemTextJson.FluentApi;

internal static class JsonPropertyInfoExtensions
{
    private static readonly PropertyInfo? s_getMemberNamePropertyInfo = typeof(JsonPropertyInfo).GetProperty("MemberName", BindingFlags.NonPublic | BindingFlags.Instance);

    public static string GetMemberName(this JsonPropertyInfo jsonProp) =>
        (string?)s_getMemberNamePropertyInfo?.GetValue(jsonProp) ?? jsonProp.Name;

    public static MemberInfo? GetMemberInfo(this JsonPropertyInfo jsonProp, Type type)
    {
        var memberName = jsonProp.GetMemberName();
        var members = type.GetMember(memberName, MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if(members?.Length>0) 
            return members[0];

        return null;
    }
}
