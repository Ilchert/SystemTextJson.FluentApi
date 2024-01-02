// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using System.Text.Json.Serialization;
using SystemTextJson.FluentApi;

var options = new JsonSerializerOptions() { WriteIndented = true, Converters = { new ValueTupleJsonConverter() } };
options.ConfigureDefaultTypeResolver(p =>
p.Entity<Person>()
.Property(p => p.LastName).HasName("surname")
.Property(p => p.FirstName).IsIgnored()
.VirtualProperty("FullName", p => $"{p?.FirstName} {p?.LastName}")
.Property(p => p.Age).HasHumberHandling(JsonNumberHandling.WriteAsString));

var person = new Person() { FirstName = "First name", LastName = "Last name", Age = 12 };
var json = JsonSerializer.Serialize(person, options);

Console.WriteLine(json);

var tupleJson = JsonSerializer.Serialize((1,"str"),options);
Console.WriteLine(tupleJson);

class Person
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public int Age { get; set; }
}
