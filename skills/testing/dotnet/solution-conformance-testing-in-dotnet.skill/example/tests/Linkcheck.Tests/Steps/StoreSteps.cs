using Linkcheck;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace Linkcheck.Tests;

[Binding]
public sealed class StoreSteps(World world, ITestOutputHelper log)
{
    private readonly string directory = Path.Combine(
        Path.GetTempPath(),
        "linkcheck-" + Guid.NewGuid()
    );
    private HistoryStore? store;
    private IReadOnlyList<Result> history = [];
    private string FilePath => Path.Combine(directory, "history.jsonl");

    private void Prepare()
    {
        Directory.CreateDirectory(directory);
        store = new HistoryStore(FilePath);
    }

    [Given("an empty history file")]
    public void Empty()
    {
        Prepare();
        File.WriteAllText(FilePath, "");
        log.WriteLine("Empty history file");
    }

    [Given("no history file")]
    public void Missing()
    {
        Prepare();
        log.WriteLine("History file does not exist");
    }

    [Given("a history file with the line {string}")]
    public void Damaged(string line)
    {
        Prepare();
        File.WriteAllText(FilePath, line + "\n");
        log.WriteLine(line);
    }

    [When("I store the check of {string}")]
    public void Store(string url)
    {
        store!.Append(Checker.Check(url));
        log.WriteLine($"Stored: {url}");
    }

    [Then("the history holds {int} check")]
    [Then("the history holds {int} checks")]
    public void Count(int expected)
    {
        history = store!.Load();
        log.WriteLine($"History: {history.Count}");
        Assert.Equal(expected, history.Count);
    }

    [Then("the last stored URL is {string}")]
    public void Last(string expected)
    {
        log.WriteLine(history[^1].Normalized);
        Assert.Equal(expected, history[^1].Normalized);
    }

    [When("I read the history")]
    public void Read()
    {
        world.Error = Xunit.Record.Exception(() => store!.Load());
        log.WriteLine($"Read: {world.Error}");
    }

    [Then("reading fails because the store is corrupted")]
    public void Corrupted()
    {
        log.WriteLine($"Error: {world.Error}");
        Assert.IsType<StoreCorrupted>(world.Error);
    }

    [When("{int} writers store {int} checks each at the same time")]
    public async Task Concurrent(int writers, int checks)
    {
        using var ready = new CountdownEvent(writers);
        using var start = new ManualResetEventSlim();
        var jobs = Enumerable
            .Range(0, writers)
            .Select(_ =>
                Task.Factory.StartNew(
                    () =>
                    {
                        ready.Signal();
                        start.Wait();
                        for (var i = 0; i < checks; i++)
                            store!.Append(Checker.Check("https://a.example"));
                    },
                    TaskCreationOptions.LongRunning
                )
            )
            .ToArray();
        ready.Wait();
        start.Set();
        await Task.WhenAll(jobs);
        log.WriteLine($"Writers: {writers}; checks per writer: {checks}");
    }

    [AfterScenario]
    public void Cleanup()
    {
        if (Directory.Exists(directory))
            Directory.Delete(directory, true);
    }
}
