using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Xunit;

namespace SystemTextJson.FluentApi.Tests;

public class JsonModelBuilderTests
{
    JsonSerializerOptions _options;
    public JsonModelBuilderTests()
    {
        _options = new JsonSerializerOptions()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            IncludeFields = true
        };
    }

    [Fact]
    public void RespectNullableReferenceType()
    {
        _options.ConfigureDefaultTypeResolver(builder =>
            builder.RespectNullableReferenceType());

        JsonAsserts.AssertObject(new TestClass(), "{}", _options);

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<TestClass>("""{"Pro": null}""", _options));
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<TestClass>("""{"Field": null}""", _options));
        JsonAsserts.AssertObject(new TestClass(), "{}", _options);
    }

    public class TestClass
    {
        [JsonPropertyName("Pro")]
        public string Property { get; set; }

        public string Field;
    }
}
