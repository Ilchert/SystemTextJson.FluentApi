namespace SystemTextJson.FluentApi;
public interface IHaveChangedProperties
{
    ISet<string>? ChangedProperties { get; }
}
