using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Xunit;

namespace SystemTextJson.FluentApi.Tests;

public class CustomizableJsonStringEnumConverterTests
{
    readonly JsonSerializerOptions _optionsWithGlobalSettings = new JsonSerializerOptions() { Converters = { new CustomizableJsonStringEnumConverter(JsonNamingPolicy.CamelCase) } };


    [Theory]
    [InlineData(A.First, "\"f\"")]
    [InlineData(null, "null")]
    [InlineData(A.Third, "\"third\"")]
    [InlineData((A)8, "8")]
    public void WriteGlobal(A? value, string json)
    {
        var actual = JsonSerializer.Serialize<A?[]>([value], _optionsWithGlobalSettings);
        Assert.True(JsonNode.DeepEquals($"""[{json}]""", actual));

    }

    [Theory]
    [InlineData(A.First, "\"f\"")]
    [InlineData(null, "null")]
    [InlineData(A.Third, "\"third\"")]
    [InlineData((A)8, "8")]
    public void ReadGlobal(A? value, string json)
    {
        var actual = JsonSerializer.Deserialize<A?[]>($"""[{json}]""", _optionsWithFluentSettings);
        Assert.Equal([value], actual);
    }

    readonly JsonSerializerOptions _optionsWithFluentSettings = new JsonSerializerOptions().ConfigureEnumValues<A>(namingPolicy: JsonNamingPolicy.CamelCase);

    [Theory]
    [InlineData(A.First, "\"f\"")]
    [InlineData(null, "null")]
    [InlineData(A.Third, "\"third\"")]
    [InlineData((A)8, "8")]
    public void WriteFluentDefault(A? value, string json)
    {
        var actual = JsonSerializer.Serialize<A?[]>([value], _optionsWithFluentSettings);
        Assert.True(JsonNode.DeepEquals($"""[{json}]""", actual));

    }

    [Theory]
    [InlineData(A.First, "\"f\"")]
    [InlineData(null, "null")]
    [InlineData(A.Third, "\"third\"")]
    [InlineData((A)8, "8")]
    public void ReadFluentDefault(A? value, string json)
    {
        var actual = JsonSerializer.Deserialize<A?[]>($"""[{json}]""", _optionsWithGlobalSettings);
        Assert.Equal([value], actual);
    }


    readonly JsonSerializerOptions _optionsWithFluentMapping = new JsonSerializerOptions().ConfigureEnumValues(
        new Dictionary<A, string> { { A.First, "f1" } },
        namingPolicy: JsonNamingPolicy.CamelCase);

    [Theory]
    [InlineData(A.First, "\"f1\"")]
    [InlineData(null, "null")]
    [InlineData(A.Third, "\"third\"")]
    [InlineData((A)8, "8")]
    public void WriteFluentMapping(A? value, string json)
    {
        var actual = JsonSerializer.Serialize<A?[]>([value], _optionsWithFluentMapping);
        Assert.True(JsonNode.DeepEquals($"""[{json}]""", actual));

    }

    [Theory]
    [InlineData(A.First, "\"f1\"")]
    [InlineData(null, "null")]
    [InlineData(A.Third, "\"third\"")]
    [InlineData((A)8, "8")]
    public void ReadFluentMapping(A? value, string json)
    {
        var actual = JsonSerializer.Deserialize<A?[]>($"""[{json}]""", _optionsWithFluentMapping);
        Assert.Equal([value], actual);
    }


    public enum A
    {
        None,
        [JsonPropertyName("f")]
        First = 1,
        Second = 2,
        Third = 3
    }
}

