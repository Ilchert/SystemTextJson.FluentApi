using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace SystemTextJson.FluentApi.Tests;
public class ValueTupleJsonConverterTests
{
    [Fact]
    public void ReadT1()
    {
        var json = """{"A":[1], "B":"123"}""";
        var options = new JsonSerializerOptions() { Converters = { new ValueTupleJsonConverter() } };
        var value = JsonSerializer.Deserialize<Dto>(json, options);
        Assert.Equivalent(new Dto { A = ValueTuple.Create(1), B = "123" }, value);
    }

    [Fact]
    public void WriteT1()
    {
        var json = """{"A":[1],"B":"123"}""";
        var options = new JsonSerializerOptions() { Converters = { new ValueTupleJsonConverter() } };
        var actualJson = JsonSerializer.Serialize(new Dto { A = ValueTuple.Create(1), B = "123" }, options);
        Assert.Equal(json, actualJson);
    }

    [Fact]
    public void WriteT17()
    {
        var json = """[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17]""";
        var options = new JsonSerializerOptions() { Converters = { new ValueTupleJsonConverter() } };
        var actualJson = JsonSerializer.Serialize((1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17), options);
        Assert.Equal(json, actualJson);
    }

    [Fact]
    public void ReadT17()
    {
        var json = """[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17]""";
        var options = new JsonSerializerOptions() { Converters = { new ValueTupleJsonConverter() } };
        var value = JsonSerializer.Deserialize<(int,int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int)>(json, options);
        Assert.Equivalent((1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17), value);
    }

    private class Dto
    {
        public ValueTuple<int> A { get; set; }
        public string? B { get; set; }
    }
}
