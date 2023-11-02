using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using Xunit;
using System.Text.Json.Serialization;
using System.Reflection;

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
    public void HasDerivedTypesFromAssembly()
    {
        _options.TypeInfoResolver = _options.TypeInfoResolver!
          .ConfigureTypes(builder =>
          builder.Entity<Root>().HasDerivedTypesFromAssembly(Assembly.GetExecutingAssembly(), t => t.Name));

        var testObject = new Root[] {
            new Derived1() { Derived1Property = "derived" },
            new Derived2() { Derived2Property = "derived2" },
            new Root(){ RootProperty = "root"}
        };
        JsonAsserts.AssertJsonAndObject(testObject, """
[
{"$type":"Derived1","Derived1Property":"derived","RootProperty":null},
{"$type":"Derived2","Derived2Property":"derived2","RootProperty":null},
{"$type":"Root","RootProperty":"root"}]
""", _options);
    }


    public class Root
    {
        public string RootProperty { get; set; }
    }

    public class Derived1 : Root
    {
        public string Derived1Property { get; set; }
    }

    public class Derived2 : Root
    {
        public string Derived2Property { get; set; }
    }

    public class TestClass
    {
        [JsonPropertyName("Pro")]
        public string? Property { get; set; }

        public string? Field;
    }
}
