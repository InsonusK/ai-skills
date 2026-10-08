using Reqnroll;
using Xunit;
using Xunit.Abstractions;
using Linkcheck;
namespace Linkcheck.Tests;
[Binding]
public sealed class ContractSteps(World world, ITestOutputHelper log)
{
    [Then("a check result has the fields:")] public void Fields(Table table)
    {
        var fields = Mapping.ToRecord(new Result(true)).Keys;
        log.WriteLine(string.Join(",", fields));
        Assert.Equal(table.Rows.Select(r => r["field"]).Order(), fields.Order());
        Assert.Equal(new[] { "ErrorCode", "IsValid", "Normalized" }, typeof(Result).GetProperties().Select(p => p.Name).Order());
    }

}
