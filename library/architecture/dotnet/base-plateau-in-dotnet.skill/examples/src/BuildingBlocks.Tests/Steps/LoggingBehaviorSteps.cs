using BuildingBlocks.Pipeline;
using MediatR;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace BuildingBlocks.Tests.Steps;

/// <summary>A command of the scenarios; the behavior logs it by its type name.</summary>
public sealed record PlanItems(string Title) : IRequest<string>;

[Binding]
public sealed class LoggingBehaviorSteps(ITestOutputHelper output)
{
    private readonly RecordingLogger<LoggingBehavior<PlanItems, string>> _logger = new();
    private RequestHandlerDelegate<string> _handler = null!;
    private Exception? _error;

    [Given("a handler of PlanItems that returns {string}, wrapped with logging")]
    public void GivenAHandlerThatReturns(string result) => _handler = _ => Task.FromResult(result);

    [Given("a handler of PlanItems that fails with {string}, wrapped with logging")]
    public void GivenAHandlerThatFails(string message) => _handler = _ => throw new InvalidOperationException(message);

    [When("the wrapped handler handles PlanItems for {string}")]
    public async Task WhenTheWrappedHandlerHandles(string title)
    {
        var behavior = new LoggingBehavior<PlanItems, string>(_logger);
        try
        {
            await behavior.Handle(new PlanItems(title), _handler, CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: handle -> {_error?.Message ?? "ok"}; {_logger.Records.Count} records");
    }

    [Then("the wrapped handler should fail with {string}")]
    public void ThenTheWrappedHandlerShouldFailWith(string message) => Assert.Equal(message, _error?.Message);

    // Compares the records, in order, with the table; an empty cell means the
    // record has no such attribute.
    [Then("the records should be:")]
    public void ThenTheRecordsShouldBe(DataTable table)
    {
        Assert.Equal(table.RowCount, _logger.Records.Count);
        for (var i = 0; i < table.RowCount; i++)
        {
            var record = _logger.Records[i];
            output.WriteLine($"then: record {i}: {string.Join(", ", record.Select(a => $"{a.Key}={a.Value}"))}");
            foreach (var header in table.Header)
            {
                var want = table.Rows[i][header];
                if (want == "")
                {
                    Assert.False(record.ContainsKey(header), $"record {i} has {header}, want none");
                }
                else
                {
                    Assert.Equal(want, record.GetValueOrDefault(header));
                }
            }
        }
    }
}
