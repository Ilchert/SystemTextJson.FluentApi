using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using Xunit;

namespace SystemTextJson.FluentApi.Tests;

public class JsonSerializerOptionsExtensionsTests
{
    private readonly JsonSerializerOptions _options;

    public JsonSerializerOptionsExtensionsTests()
    {
        _options = new JsonSerializerOptions()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            IncludeFields = true
        };
    }

    [Fact]
    public void SerializeAsObject()
    {
        _options.SerializeAsObject<Root>();
        Root testObject = new Derived { Property = "Prop" };

        JsonAsserts.AssertJson(testObject, """{"Property":"Prop"}""", _options);
        Assert.Throws<JsonException>(() => JsonAsserts.AssertObject(testObject, """{"Property":"Prop"}""", _options));
    }

    public class Root { }

    public class Derived : Root
    {
        public string? Property { get; set; }
    }
}
