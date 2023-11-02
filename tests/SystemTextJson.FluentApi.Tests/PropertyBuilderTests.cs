using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Xunit;

namespace SystemTextJson.FluentApi.Tests;

public class PropertyBuilderTests
{
    JsonSerializerOptions _options;
    public PropertyBuilderTests()
    {
        _options = new JsonSerializerOptions()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            IncludeFields = true
        };
    }

    [Fact]
    public void HasName()
    {
        // inheritance, validation

        _options.ConfigureDefaultTypeResolver(builder =>
            builder.Entity<TestClass>()
            .Property(p => p.Property).HasName("PropertyName")
            .Property(p => p.Field).HasName("FieldName"));

        var testObject = new TestClass { Property = "Prop", Field = "field" };

        JsonAsserts.AssertJsonAndObject(testObject, """{"PropertyName":"Prop","FieldName":"field"}""", _options);
    }

    [Fact]
    public void HasConverter()
    {
        var converter = new TestConverter();
        // inheritance, validation

        _options.ConfigureDefaultTypeResolver(builder =>
            builder.Entity<TestClass>()
            .Property(p => p.Property).HasConverter(converter)
            .Property(p => p.Field).HasConverter(converter));

        var testObject = new TestClass { Property = "Prop", Field = "field" };

        JsonAsserts.AssertJsonAndObject(testObject, """{"Property":"Prop","Field":"field"}""", _options);

        Assert.Equal(4, converter.CallCount);
    }

    [Fact]
    public void IsExtensionData_Object()
    {
        _options.ConfigureDefaultTypeResolver(builder =>
            builder.Entity<ExtensionDataObject>()
            .Property(p => p.Data).IsExtensionData());


        var testObject = new ExtensionDataObject
        {
            Data = new()
            {
                { "Property", "Prop" },
                { "Field", "field" },
             }
        };
        var json = """{"Property":"Prop","Field":"field"}""";
        JsonAsserts.AssertJson(testObject, json, _options);
        var deserializedObject = JsonSerializer.Deserialize<ExtensionDataObject>(json, _options);
        Assert.NotNull(deserializedObject);
        Assert.NotNull(deserializedObject.Data);
        Assert.Contains("Property", deserializedObject.Data);
        Assert.Contains("Field", deserializedObject.Data);
    }

    [Fact]
    public void IsExtensionData_JsonElement()
    {
        _options.ConfigureDefaultTypeResolver(builder =>
            builder.Entity<ExtensionDataJsonElement>()
            .Property(p => p.Data).IsExtensionData());

        var json = """{"Property":"Prop","Field":"field"}""";
        var deserializedObject = JsonSerializer.Deserialize<ExtensionDataJsonElement>(json, _options);
        Assert.NotNull(deserializedObject);
        Assert.NotNull(deserializedObject.Data);
        Assert.Contains("Property", deserializedObject.Data);
        Assert.Contains("Field", deserializedObject.Data);

        JsonAsserts.AssertJson(deserializedObject, json, _options);
    }

    [Fact]
    public void IsExtensionData_JsonObject()
    {
        _options.ConfigureDefaultTypeResolver(builder =>
            builder.Entity<ExtensionDataJsonObject>()
            .Property(p => p.Data).IsExtensionData());

        var json = """{"Property":"Prop","Field":"field"}""";
        var deserializedObject = JsonSerializer.Deserialize<ExtensionDataJsonObject>(json, _options);
        Assert.NotNull(deserializedObject);
        Assert.NotNull(deserializedObject.Data);
        Assert.Contains("Property", deserializedObject.Data);
        Assert.Contains("Field", deserializedObject.Data);

        Assert.ThrowsAny<JsonException>(() => JsonAsserts.AssertJson(deserializedObject, json, _options));// https://github.com/dotnet/runtime/issues/60560
    }

    [Fact]
    public void IsExtensionData_JsonNode()
    {
        Assert.Throws<InvalidOperationException>(() =>
        _options.ConfigureDefaultTypeResolver(builder =>
              builder.Entity<ExtensionDataJsonNode>()
              .Property(p => p.Data).IsExtensionData()));
    }

    [Fact]
    public void HasNumberHandling()
    {
        _options.ConfigureDefaultTypeResolver(builder =>
            builder.Entity<NumberHandlinClass>()
            .Property(p => p.Number).HasHumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString));

        var testObject = new NumberHandlinClass { Number = 1 };

        JsonAsserts.AssertJsonAndObject(testObject, """{"Number":"1"}""", _options);
    }

    [Fact]
    public void HasObjectCreationHandling()
    {
        _options.ConfigureDefaultTypeResolver(builder =>
            builder.Entity<ObjectCreationHandlingClass>()
            .Property(p => p.Prop).HasObjectCreationHandling(JsonObjectCreationHandling.Populate));

        var testObject = new ObjectCreationHandlingClass();
        JsonAsserts.AssertObject(testObject, """{"Prop":{ }}""", _options);
    }

    [Fact]
    public void HasOrder()
    {
        // inheritance, validation

        _options.ConfigureDefaultTypeResolver(builder =>
            builder.Entity<TestClass>()
            .Property(p => p.Property).HasOrder(2)
            .Property(p => p.Field).HasOrder(1));

        var testObject = new TestClass { Property = "Prop", Field = "field" };
        var json = JsonSerializer.Serialize(testObject, _options);
        using var doc = JsonDocument.Parse(json);
        Assert.Collection(doc.RootElement.EnumerateObject(),
            p => Assert.Equal("Field", p.Name),
            p => Assert.Equal("Property", p.Name));
    }

    private class TestConverter : JsonConverter<string>
    {
        public int CallCount { get; private set; }
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            CallCount++;
            return reader.GetString();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            CallCount++;
            writer.WriteStringValue(value);
        }
    }

    public class ExtensionDataObject
    {
        public Dictionary<string, object>? Data { get; set; }
    }

    public class ExtensionDataJsonElement
    {
        public Dictionary<string, JsonElement>? Data { get; set; }
    }

    public class ExtensionDataJsonNode
    {
        public Dictionary<string, JsonNode>? Data { get; set; }
    }

    public class ExtensionDataJsonObject
    {
        public JsonObject Data { get; set; }
    }

    public class TestClass
    {
        public string? Property { get; set; }

        public string? Field;
    }

    public class NumberHandlinClass
    {
        public int? Number { get; set; }
    }

    public class ObjectCreationHandlingClass
    {
        public TestClass Prop { get; set; } = new TestClass() { Property = "1" };
    }
}
