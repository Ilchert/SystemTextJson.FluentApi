[![NuGet](https://buildstats.info/nuget/SystemTextJson.FluentApi)](https://www.nuget.org/packages/SystemTextJson.FluentApi/ "Download SystemTextJson.FluentApi from NuGet.org")
WARNING: this package is under active development so the api may change at any time.
Normally you do not need to use this package and just copy paste required functionality like NRT or polymorphism support.
# SystemTextJson.FluentApi
SystemTextJson.FluentApi is a fluent configuration library for System.Text.Json that allows developers to configure serialization uses strongly typed fluent interface and lambda expression.

# Documentation
All api usually repeats attributes from [System.Text.Json.Serialization](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization) and set corresponding property in [JsonPropertyInfo](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.metadata.jsonpropertyinfo?view=net-7.0) or [JsonTypeInfo](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.metadata.jsontypeinfo?view=net-7.0). Configuration based on [IJsonTypeInfoResolver](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.metadata.ijsontypeinforesolver) so developers can configure reflection based [TypeInfoResolver](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.metadata.defaultjsontypeinforesolver) and source generator [JsonSerializerContext](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonserializercontext).

# Quick start
To use FluentApi need to configure [JsonSerializerOptions](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsonserializeroptions) instance via `JsonModelBuilder` and pass it to serializer.

```C#
var options = new JsonSerializerOptions() { WriteIndented = true };
options.ConfigureDefaultTypeResolver(p =>
p.Entity<Person>()
.Property(p => p.LastName).HasName("surname")
.Property(p => p.FirstName).IsIgnored()
.VirtualProperty("FullName", p => $"{p.FirstName} {p.LastName}")
.Property(p => p.Age).HasHumberHandling(JsonNumberHandling.WriteAsString));

var person = new Person() { FirstName = "First name", LastName = "Last name", Age = 12 };
var json = JsonSerializer.Serialize(person, options);

Console.WriteLine(json);
```

This example produce this JSON
```Json
{
  "surname": "Last name",
  "Age": "12",
  "FullName": "First name Last name"
}
```

# Polymorphism serialization
STJ has build in support polymorphic serialization, but user have to annotate base class with [JsonDerivedTypeAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonderivedtypeattribute) with all derived types. In fluent API you can configure each derived type manually or find all derived types in runtime. 

```C#
builder.Entity<Root>()
.HasDerivedType<Derived1>(nameof(Derived1))
.HasDerivedType<Derived2>(nameof(Derived2))
.HasDerivedType<Root>(nameof(Root))
// or
builder.Entity<Root>().HasDerivedTypesFromAssembly(Assembly.GetExecutingAssembly(), t => t.Name)

var testObject = new Root[] 
{
    new Derived1() { Derived1Property = "derived" },
    new Derived2() { Derived2Property = "derived2" },
    new Root(){ RootProperty = "root"}
};


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
```

Serialization of `testObject` collection will produce:

```JSON
[
   {
      "$type":"Derived1",
      "Derived1Property":"derived",
      "RootProperty":null
   },
   {
      "$type":"Derived2",
      "Derived2Property":"derived2",
      "RootProperty":null
   },
   {
      "$type":"Root",
      "RootProperty":"root"
   }
]
```

With `$type` discriminator serializer are able to deserialize this collection. Another approach to serialization is use actual type from object instance, instead of property type. To achieve this behavior serializer can threat specific property as `object` using `PropertyBuilder.SerializeAsObject`.

```C#
builder.Entity<AsObjectTestClass>().Property(p => p.Data).SerializeAsObject();

var testObject = new AsObjectTestClass { Data = new Derived() { Property = "Prop" } };

public class AsObjectTestClass
{
    public Root? Data { get; set; }
}

public class Root { }

public class Derived : Root
{
    public string? Property { get; set; }
}

```

Serialization of `testObject` will produce:
```JSON
{
   "Data":{
      "Property":"Prop"
   }
}
```

But in this case only serialization is available because JSON does not contain type discriminator and `JsonException` will be thrown on deserialization.

# Nullable reference type support
STJ has build in support of [`required`](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/required) properties, but it just check, that value exists in JSON on deserialization and does not prevent setting `null` to none nullable properties. Fluent Api can configure `JsonSerializerOptions` to respect NRT annotations on fields and properties. Internally it uses [JsonPropertyInfo.Set](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.metadata.jsonpropertyinfo.set) property and reduces deserialization performance.

```C#
builder.RespectNullableReferenceType();

JsonSerializer.Deserialize<TestClass>("""{"Property": null}""", _options); // this throws JsonException
JsonAsserts.AssertObject(new TestClass(), "{}", _options); // BUT this is not because Property is not requred.

public class TestClass
{
    public string Property { get; set; }
}
```

# Virtual properties

Fluent Api can define virtual properties, that does not match to any real property in object.

```C#
builder.Entity<Person>()
.Property(p => p.LastName).IsIgnored()
.Property(p => p.FirstName).IsIgnored()
.VirtualProperty("FullName", p => $"{p.FirstName} {p.LastName}")

var testObject = new Person() { FirstName = "First name", LastName = "Last name" };

class Person
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }
}

```

Serialization of `testObject` will produce:

```JSON
{
    "FullName": "First name Last name"
}
```

# Change tracking

Fluent Api can track changes during serialization and deserialization. If some entity implement `IHaveChnagedProperties` interface with not null `ChangedProperties` property, it will be used to track changes. To populate property/field names that set deserialization use `TrackChangedProperties()` method. To serialize properties only from `ChangedProperties` use `SerializeOnlyChangedProperties()`. This method will override `JsonIgnoreCondition`.

```C#
builder.TrackChangedProperties().SerializeOnlyChangedProperties();

var testObject = new TrackTestClass()
{
    StringProperty = "str",
    IntProperty = 1,
    ChangedProperties = { nameof(TrackTestClass.IntProperty) }
};

public class TrackTestClass : IHaveChangedProperties
{
    public string? StringProperty { get; set; }
    public int IntProperty { get; set; }
    public ISet<string> ChangedProperties { get; } = new HashSet<string>();
}

```

Serialization of `testObject` will produce:

```JSON
{
    "IntProperty": 1
}
```
And deserialization of this JSON will populate `"IntProperty"` value to `ChangedProperties`.

# ValueTuple serialization

Fluent Api has `ValueTupleJsonConverter` to serialize and deserialize `ValueTuple` as array. 

```C#
var options = new JsonSerializerOptions() { Converters = { new ValueTupleJsonConverter() } };
JsonSerializer.Serialize((1,"str"),options);
```

This code output:

```JSON
[1,"str"]
```

# Inline arrays support

Fluent Api has `InlineArrayJsonConverter` for .NET 8 and above to serialize and deserialize [`InlineArray`](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/inline-arrays) structs as arrays.

```C#
var array = new InlineArray();
array[0] = null;
array[1] = 1;
array[2] = -1;
var options = new JsonSerializerOptions() { Converters = { new InlineArrayJsonConverter() } };
JsonSerializer.Serialize(array,options);

[InlineArray(3)]
private struct InlineArray
{
    public int? Value;
}
```
 
Output: `"[null,1,-1]"`

