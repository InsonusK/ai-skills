using Linkcheck;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace Linkcheck.Tests;

[Binding]
public sealed class SummarySteps(World world, ITestOutputHelper log)
{
    private IEnumerable<string> urls = [];
    private Linkcheck.Batch.Summary summary = new(0, 0, new Dictionary<string, int>());

    [Given("the URLs:")]
    public void Urls(Table table)
    {
        urls = table.Rows.Select(r => r["url"]).ToArray();
        log.WriteLine(string.Join(",", urls));
    }

    [Given("no URLs")]
    public void NoUrls()
    {
        urls = [];
        log.WriteLine("No URLs");
    }

    [When("I summarize the batch")]
    public void Summarize()
    {
        summary = Linkcheck.Batch.Summarizer.Summarize(urls);
        log.WriteLine($"Summary: {summary}");
    }

    [Then("the batch has {int} valid and {int} invalid URLs")]
    public void Counts(int valid, int invalid)
    {
        log.WriteLine($"{summary.Valid}/{summary.Invalid}");
        Assert.Equal(valid, summary.Valid);
        Assert.Equal(invalid, summary.Invalid);
    }

    [Then("the count of {string} errors is {int}")]
    public void Errors(string code, int count)
    {
        log.WriteLine($"{code}: {summary.Errors.GetValueOrDefault(code)}");
        Assert.Equal(count, summary.Errors.GetValueOrDefault(code));
    }
}
