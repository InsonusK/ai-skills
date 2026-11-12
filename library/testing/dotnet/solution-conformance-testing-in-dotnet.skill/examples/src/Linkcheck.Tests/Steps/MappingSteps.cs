using Linkcheck;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace Linkcheck.Tests;

[Binding]
public sealed class MappingSteps(World world, ITestOutputHelper log)
{
    private Dictionary<string, object> record = new();
    private Result? restored;

    [When("I map the result to a record and back")]
    public void RoundTrip()
    {
        restored = Mapping.FromRecord(Mapping.ToRecord(world.Result));
        log.WriteLine($"Restored: {restored}");
    }

    [Then("the result is unchanged")]
    public void Unchanged()
    {
        log.WriteLine($"Expected: {world.Result}");
        Assert.Equal(world.Result, restored);
    }

    [Given("the record:")]
    public void Record(Table table)
    {
        record = table.Rows.ToDictionary(r => r["field"], r => (object)r["value"]);
        log.WriteLine(string.Join(",", record.Keys));
    }

    [When("I map the record to a result")]
    public void Map()
    {
        world.Error = Xunit.Record.Exception(() => Mapping.FromRecord(record));
        log.WriteLine($"Mapping: {world.Error}");
    }

    [Then("the mapping fails with error {string}")]
    public void Error(string expected)
    {
        log.WriteLine($"Error: {world.Error}");
        Assert.Equal(expected, Assert.IsType<RecordError>(world.Error).Message);
    }
}
