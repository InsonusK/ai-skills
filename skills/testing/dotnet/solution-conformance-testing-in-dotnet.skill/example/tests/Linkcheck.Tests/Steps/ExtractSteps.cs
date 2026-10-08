using Reqnroll;
using Xunit;
using Xunit.Abstractions;
using Linkcheck;
namespace Linkcheck.Tests;
[Binding]
public sealed class ExtractSteps(World world, ITestOutputHelper log)
{
    private string text = "";
    private IReadOnlyList<string> links = [];
    [Given("the text {string}")] public void Text(string value) { text = value; log.WriteLine(value); }
    [When("I extract the links")] public void Extract() { links = Extractor.Extract(text); log.WriteLine(string.Join(",", links)); }
    [Then("the links are:")] public void Links(Table table) { log.WriteLine(string.Join(",", links)); Assert.Equal(table.Rows.Select(r => r["link"]), links); }
    [Then("the number of links is {int}")] public void Count(int count) { log.WriteLine($"Count: {links.Count}"); Assert.Equal(count, links.Count); }

}
