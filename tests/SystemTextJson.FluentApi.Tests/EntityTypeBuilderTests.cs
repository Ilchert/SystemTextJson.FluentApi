using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using Xunit;
using System.Text.Json.Serialization;

namespace SystemTextJson.FluentApi.Tests;

public class EntityTypeBuilderTests
{
    JsonSerializerOptions _options;
    public EntityTypeBuilderTests()
    {
        _options = new JsonSerializerOptions()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            IncludeFields = true
        };
    }

    [Fact]
    public void Ignore()
    {
        // inheritance, validation

        _options.TypeInfoResolver = _options.TypeInfoResolver!
            .ConfigureTypes(builder =>
            builder.Entity<TestClass>()
            .IgnoreProperty(p => p.Property)
            .IgnoreProperty(p => p.Field));

        var testObject = new TestClass { Property = "Prop", Field = "field" };

        JsonAsserts.AssertJson(testObject, """{}""", _options);
        JsonAsserts.AssertObject(new TestClass { }, """{"Property":"Prop","Field":"field"}""", _options);
    }

    [Fact]
    public void IsUnmappedMemberDisallowed()
    {
        _options.TypeInfoResolver = _options.TypeInfoResolver!
            .ConfigureTypes(builder =>
            builder.Entity<TestClass>()
            .IsUnmappedMemberDisallowed());

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<TestClass>("""{"UnmappedProperty": null}""", _options));
    }

    [Fact]
    public void RespectNullableReferenceType()
    {
        _options.TypeInfoResolver = _options.TypeInfoResolver!
            .ConfigureTypes(builder =>
            builder.RespectNullableReferenceType());


        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<TestClass>("""{"Pro": null}""", _options));
    }

    public class TestClass
    {
        [JsonPropertyName("Pro")]
        public string Property { get; set; }

        public string? Field;
    }
}
