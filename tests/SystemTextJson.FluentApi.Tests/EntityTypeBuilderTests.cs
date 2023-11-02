using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

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
            .Ignore(p => p.Property)
            .Ignore(p => p.Field));

        var testObject = new TestClass { Property = "Prop", Field = "field" };

        JsonAsserts.AssertJson(testObject, """{}""", _options);
        JsonAsserts.AssertObject(new TestClass { }, """{"Property":"Prop","Field":"field"}""", _options);
    }

    public class TestClass
    {
        public string? Property { get; set; }

        public string? Field;
    }
}
