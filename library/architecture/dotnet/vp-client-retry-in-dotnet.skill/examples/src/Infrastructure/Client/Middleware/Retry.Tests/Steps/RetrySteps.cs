using Client.Abstractions;
using Client.Pipeline;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace Retry.Tests.Steps;

[Binding]
public sealed class RetrySteps(ITestOutputHelper output)
{
    private readonly RecordingLogger _logger = new();
    private RetryConfig _config = new(3, TimeSpan.FromMilliseconds(1));
    private ClientCall _call = new("todo-api", "POST /v1/items/plan", true);
    private string[] _outcomes = [];
    private int _attempts;
    private Exception? _error;

    private static TimeSpan Duration(string text) =>
        text.EndsWith("ms") ? TimeSpan.FromMilliseconds(int.Parse(text[..^2])) : TimeSpan.FromSeconds(int.Parse(text[..^1]));

    [Given("the retry allows {int} attempts with a backoff of {word}")]
    public void GivenTheRetryAllows(int attempts, string backoff) => _config = new RetryConfig(attempts, Duration(backoff));

    [Given("a retryable call whose attempts end {string}")]
    public void GivenARetryableCall(string outcomes) => (_call, _outcomes) = (_call with { Retryable = true }, outcomes.Split('|'));

    [Given("a call not marked retryable whose attempts end {string}")]
    public void GivenACallNotMarkedRetryable(string outcomes) => (_call, _outcomes) = (_call with { Retryable = false }, outcomes.Split('|'));

    /// <summary>Ends each attempt as the scenario lists: ok, unavailable, or status.</summary>
    private Task Attempt(CancellationToken _) =>
        _outcomes[_attempts++] switch
        {
            "unavailable" => throw new ApiUnavailableException("todo-api", "connection refused"),
            "status" => throw new ApiStatusException("todo-api", ApiStatusCode.InvalidArgument, "rejected", 400),
            _ => Task.CompletedTask,
        };

    private async Task Run(CancellationToken cancellationToken)
    {
        try
        {
            await ClientPipeline.RunAsync(_call, [new RetryMiddleware(_config, _logger)], Attempt, cancellationToken);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: {_attempts} attempts, error {_error?.GetType().Name}: {_error?.Message}");
    }

    [When("the call is run through the retry")]
    public Task WhenTheCallIsRun() => Run(CancellationToken.None);

    [When("the call is run through the retry, cancelled after {word}")]
    public async Task WhenTheCallIsRunCancelled(string after)
    {
        using var cancel = new CancellationTokenSource(Duration(after));
        await Run(cancel.Token);
    }

    [Then("the call should succeed after {int} attempts")]
    public void ThenTheCallShouldSucceed(int attempts) => Assert.Equal((attempts, (Exception?)null), (_attempts, _error));

    [Then("the call should fail as unavailable after {int} attempts")]
    public void ThenTheCallShouldFailAsUnavailable(int attempts)
    {
        Assert.IsType<ApiUnavailableException>(_error);
        Assert.Equal(attempts, _attempts);
    }

    [Then("the call should fail with a status after {int} attempts")]
    public void ThenTheCallShouldFailWithAStatus(int attempts)
    {
        Assert.IsType<ApiStatusException>(_error);
        Assert.Equal(attempts, _attempts);
    }

    [Then("{int} \"api client call retried\" warnings should name the API {string} and the operation {string}")]
    public void ThenWarningsShouldName(int count, string api, string operation) =>
        Assert.Equal(count, _logger.Records.Count(record => record.Message == "api client call retried" && record.Level == LogLevel.Warning
            && (string)record.Attributes["api"]! == api && (string)record.Attributes["operation"]! == operation));

    [Then("no \"api client call retried\" warning should be logged")]
    public void ThenNoWarningShouldBeLogged() => Assert.Empty(_logger.Records);
}
