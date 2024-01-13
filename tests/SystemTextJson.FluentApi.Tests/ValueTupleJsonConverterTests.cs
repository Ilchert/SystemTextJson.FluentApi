using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace SystemTextJson.FluentApi.Tests;
public class ValueTupleJsonConverterTests
{
    private JsonSerializerOptions _options = new JsonSerializerOptions() { Converters = { new ValueTupleJsonConverter() } };


    public static IEnumerable<object[]> Data()
    {
        (object? value, Type type, string json)[] initialData = [
            (1, typeof(int),"1"),
            (null,typeof(int?),"null"),
            (2.2, typeof(double),"2.2"),
            (null, typeof(string),"null"),
            ("",typeof(string),"\"\""),
            ("str", typeof(string),"\"str\""),
            (null,typeof(int[]),"null"),
            (Array.Empty<int>(),typeof(int[]),"[]"),
            (new int[]{1,2},typeof(int[]),"[1,2]"),
            (null,typeof(EmptyDto),"null"),
            (new EmptyDto(),typeof(EmptyDto),"{}"),
            ];

        var defaultItem = initialData[0];

        for (var i = 1; i < 9; i++)
        {
            var template = Enumerable.Repeat(defaultItem, i).ToArray();
            for (var j = 0; j < template.Length; j++)
            {
                foreach (var item in initialData)
                {
                    template[j] = item;
                    var tuple = CreateTuple(template.Select(p => p.type).ToArray(), template.Select(p => p.value).ToArray());
                    var json = string.Join(",", template.Select(p => p.json));
                    yield return [tuple, $"[{json}]"];
                }
                template[j] = defaultItem;
            }
        }

        yield return [(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17), "[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17]"];
    }

    private static object CreateTuple(Type[] types, object?[] values)
    {
        var m = typeof(ValueTuple).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Single(p => p.GetParameters().Length == types.Length);
        var genericMethod = m.MakeGenericMethod(types);
        return genericMethod.Invoke(null, values)!;
    }


    [Theory]
    [MemberData(nameof(Data))]
    public void ReadJson<T>(T expected, string json)
    {
        var actual = JsonSerializer.Deserialize<T>(json, _options);
        Assert.Equivalent(expected, actual);
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void WriteJson<T>(T value, string expected)
    {
        var actual = JsonSerializer.Serialize(value, _options);
        var isEquals = JsonNode.DeepEquals(JsonNode.Parse(expected), JsonNode.Parse(actual));
        Assert.True(isEquals, "Json not equal.");
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void ReadJsonFromDto<T>(T expected, string json)
    {
        var actual = JsonSerializer.Deserialize<Dto<T>>($$"""{"A":{{json}}, "B":"123"}""", _options);
        Assert.Equivalent(new Dto<T> { A = expected, B = "123" }, actual);
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void WriteJsonFromDto<T>(T value, string expected)
    {
        var actual = JsonSerializer.Serialize(new Dto<T> { A = value, B = "123" }, _options);
        var isEquals = JsonNode.DeepEquals(JsonNode.Parse($$"""{"A":{{expected}}, "B":"123"}"""), JsonNode.Parse(actual));
        Assert.True(isEquals, "Json not equal.");
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void ReadJsonFromArray<T>(T expected, string json)
    {
        var actual = JsonSerializer.Deserialize<T[]>($"[{json}]", _options);
        Assert.Equivalent((T[])[expected], actual);
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void WriteJsonFromArray<T>(T value, string expected)
    {
        var actual = JsonSerializer.Serialize<T[]>([value], _options);
        var isEquals = JsonNode.DeepEquals(JsonNode.Parse($"[{expected}]"), JsonNode.Parse(actual));
        Assert.True(isEquals, "Json not equal.");
    }

    private class EmptyDto();

    private class Dto<T>
    {
        public T? A { get; set; }
        public string? B { get; set; }
    }
}
