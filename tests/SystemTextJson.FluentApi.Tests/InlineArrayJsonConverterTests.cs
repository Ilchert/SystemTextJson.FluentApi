using Xunit;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SystemTextJson.FluentApi.Tests;
public class InlineArrayJsonConverterTests
{
    readonly JsonSerializerOptions _options = new() { Converters = { new InlineArrayJsonConverter() } };

    [Fact]
    public void Write()
    {
        var array = new InlineArray();
        array[0] = null;
        array[1] = 1;
        array[2] = -1;

        var actualJson = JsonSerializer.Serialize(array, _options);

        var isEquals = JsonNode.DeepEquals(JsonNode.Parse("[null,1,-1]"), JsonNode.Parse(actualJson));
        Assert.True(isEquals, "Json not equal.");
    }


    [Fact]
    public void Read()
    {
        var actual = JsonSerializer.Deserialize<InlineArray>("[null,1,-1]", _options);

        Assert.Null(actual[0]);
        Assert.Equal(actual[1], 1);
        Assert.Equal(actual[2], -1);
    }



    [InlineArray(3)]
    private struct InlineArray
    {
        public int? Value;
    }
}
