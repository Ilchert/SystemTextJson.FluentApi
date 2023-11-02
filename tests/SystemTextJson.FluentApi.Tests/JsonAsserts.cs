using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace SystemTextJson.FluentApi.Tests;

public static class JsonAsserts
{
    public static void AssertJsonAndObject<T>(T testObject, [StringSyntax(StringSyntaxAttribute.Json)] string expectedJson, JsonSerializerOptions options)
    {
        AssertJson(testObject, expectedJson, options);
        AssertObject(testObject, expectedJson, options);        
    }

    public static void AssertJson<T>(T testObject, [StringSyntax(StringSyntaxAttribute.Json)] string expectedJson, JsonSerializerOptions options)
    {
        var json = JsonSerializer.Serialize(testObject, options);
        var actual = JsonNode.Parse(json);
        var expected = JsonNode.Parse(expectedJson);
        Assert.True(JsonNode.DeepEquals(expected, actual), "Jsons are not equals.");
    }

    public static void AssertObject<T>(T testObject, [StringSyntax(StringSyntaxAttribute.Json)] string expectedJson, JsonSerializerOptions options)
    {
        var expectedObject = JsonSerializer.Deserialize<T>(expectedJson, options);
        Assert.Equivalent(testObject, expectedObject);
    }
}