using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Xunit;

namespace SystemTextJson.FluentApi.Tests;

public class CustomizableJsonStringEnumConverterTests
{
    JsonSerializerOptions _options = new JsonSerializerOptions() { Converters = { new CustomizableJsonStringEnumConverter(JsonNamingPolicy.CamelCase) } };

    [Fact]
    public void Write()
    {
        var actual = JsonSerializer.Serialize<A?[]>([A.First, null, A.Third, (A)8], _options);

        Assert.True(JsonNode.DeepEquals("""["f",null,"third",8]""", actual));

    }

    [Fact]
    public void Read()
    {
        var actual = JsonSerializer.Deserialize<A?[]>("""["f",null,"third",8]""", _options);
        Assert.Equal([A.First, null, A.Third, (A)8], actual);
    }

    enum A
    {
        None,
        [JsonPropertyName("f")]
        First = 1,
        Second = 2,
        Third = 3
    }
}

