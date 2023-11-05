using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Xunit;

namespace SystemTextJson.FluentApi.Tests;

public class JsonModelBuilderTests
{
    private readonly JsonSerializerOptions _options;
    public JsonModelBuilderTests()
    {
        _options = new JsonSerializerOptions()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            IncludeFields = true
        };
    }


    [Fact]
    public void MultiConfiguration()
    {
        _options.ConfigureDefaultTypeResolver(builder =>
            builder.Entity<TestClass>()
            .Property(p => p.Property).HasName("Prop1")
            .Property(p => p.Property).HasName("Prop2")
            .Entity<TestClass>()
            .Property(p => p.Field).HasName("Field1").HasName("Field2"));

        JsonAsserts.AssertJsonAndObject(new TestClass() { Property = "Prop", Field = "Field" }, """{"Prop2":"Prop","Field2":"Field"}""", _options);
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
        public string Property { get; set; } = null!;

        public string Field = null!;
    }
}
