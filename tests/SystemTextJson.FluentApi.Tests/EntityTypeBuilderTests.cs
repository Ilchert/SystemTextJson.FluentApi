using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using Xunit;
using System.Text.Json.Serialization;
using System.Reflection;

namespace SystemTextJson.FluentApi.Tests;

public class EntityTypeBuilderTests
{
    private readonly JsonSerializerOptions _options;
    public EntityTypeBuilderTests()
    {
        _options = new JsonSerializerOptions()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            IncludeFields = true
        };
    }

    [Fact]
    public void IsUnmappedMemberDisallowed()
    {
        _options.ConfigureDefaultTypeResolver(builder =>
            builder.Entity<TestClass>()
            .IsUnmappedMemberDisallowed());

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<TestClass>("""{"UnmappedProperty": null}""", _options));
    }

    [Fact]
    public void VirtualProperty()
    {
        _options.ConfigureDefaultTypeResolver(builder =>
                builder.Entity<TestClass>()
                .Property(p => p.Property).IsIgnored()
                .Property(p => p.Field).IsIgnored()
                .VirtualProperty("virtualProperty", p => "computed")
                .Entity<TestClass>()
                .VirtualProperty("virtualProperty", p => "computed")
                .HasName("renamedVirtualProperty"));

        JsonAsserts.AssertJson(new TestClass { }, """{"renamedVirtualProperty":"computed"}""", _options);
    }

    [Fact]
    public void HasDerivedType()
    {
        _options.ConfigureDefaultTypeResolver(builder =>
          builder.Entity<Root>()
          .HasDerivedType<Derived1>(nameof(Derived1))
          .HasDerivedType<Derived2>(nameof(Derived2))
          .HasDerivedType<Root>(nameof(Root)));

        var testObject = new Root[]
        {
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


    [Fact]
    public void HasDerivedTypesFromAssembly()
    {
        _options.ConfigureDefaultTypeResolver(builder =>
          builder.Entity<Root>().HasDerivedTypesFromAssembly(Assembly.GetExecutingAssembly(), t => t.Name));

        var testObject = new Root[]
        {
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
        public string? RootProperty { get; set; }
    }

    public class Derived1 : Root
    {
        public string? Derived1Property { get; set; }
    }

    public class Derived2 : Root
    {
        public string? Derived2Property { get; set; }
    }

    public class TestClass
    {
        [JsonPropertyName("Pro")]
        public string? Property { get; set; }

        public string? Field;
    }
}
