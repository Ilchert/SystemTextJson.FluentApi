using System.Reflection;

namespace SystemTextJson.FluentApi;

public interface IMemberPropertyBuilder : IPropertyBuilder
{
    public MemberInfo MemberInfo { get; }
}
