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

    [Fact]
    public void TrackChangedProperties()
    {
        _options.ConfigureDefaultTypeResolver(builder => builder.TrackChangedProperties());

        var obj = JsonSerializer.Deserialize<TrackTestClass>("""{"StringProperty":"str", "IntProperty":1}""", _options);

        Assert.NotNull(obj);
        Assert.Contains(nameof(TrackTestClass.IntProperty), obj.ChangedProperties);
        Assert.Contains(nameof(TrackTestClass.StringProperty), obj.ChangedProperties);
    }

    [Fact]
    public void SerializeOnlyChangedProperties()
    {
        _options.ConfigureDefaultTypeResolver(builder => builder.SerializeOnlyChangedProperties());

        var testObject = new TrackTestClass()
        {
            StringProperty = "str",
            IntProperty = 1,
            ChangedProperties = { nameof(TrackTestClass.IntProperty) }
        };

        JsonAsserts.AssertJson(testObject, """{"IntProperty":1}""", _options);
    }

    public class TrackTestClass : IHaveChangedProperties
    {
        public string? StringProperty { get; set; }
        public int IntProperty { get; set; }
        public ISet<string> ChangedProperties { get; } = new HashSet<string>();
    }

    public class TestClass
    {
        [JsonPropertyName("Pro")]
        public string Property { get; set; } = null!;

        public string Field = null!;
    }
}
