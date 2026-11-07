using MediatR;
using Middleware.StoreFailures;
using Reqnroll;
using Shared.Errors;
using Shared.Persistence;
using Xunit;

namespace Middleware.Tests.Steps;

/// <summary>The store's meaning of a failure, faked by hand: one message it knows, told as one kind.</summary>
public sealed class FakeStoreFailures : IStoreFailures
{
    public Dictionary<string, (ErrorKind Kind, string Message)> Known { get; } = [];

    public ErrorKindException? KindOf(Exception failure) =>
        Known.TryGetValue(failure.Message, out var told) ? new ErrorKindException(told.Kind, told.Message, failure) : null;
}

[Binding]
public sealed class StoreFailuresSteps
{
    private readonly FakeStoreFailures _failures = new();
    private string? _result;
    private Exception? _error;

    private async Task RunAsync(RequestHandlerDelegate<string> handler)
    {
        var middleware = new StoreFailuresMiddleware<string, string>(_failures);
        _error = await Record.ExceptionAsync(async () => _result = await middleware.Handle("command", handler, CancellationToken.None));
    }

    [Given("the store tells the failure {string} as the kind {string} with the message {string}")]
    public void GivenTheStoreTells(string failure, string kind, string message) =>
        _failures.Known[failure] = (Enum.Parse<ErrorKind>(kind, ignoreCase: true), message);

    [When("a command over the store failures middleware returns {string}")]
    public Task WhenACommandReturns(string result) => RunAsync(_ => Task.FromResult(result));

    [When("a command over the store failures middleware fails with {string}")]
    public Task WhenACommandFails(string failure) => RunAsync(_ => throw new InvalidOperationException(failure));

    [Then("the command should answer {string}")]
    public void ThenTheCommandShouldAnswer(string result)
    {
        Assert.Null(_error);
        Assert.Equal(result, _result);
    }

    [Then("the command should fail with the kind {string} and the message {string}")]
    public void ThenTheCommandShouldFailWithTheKind(string kind, string message)
    {
        Assert.Equal(Enum.Parse<ErrorKind>(kind, ignoreCase: true), ErrorKindException.KindOf(_error));
        Assert.Equal(message, _error!.Message);
    }

    [Then("the cause of the error should be {string}")]
    public void ThenTheCauseShouldBe(string cause) => Assert.Equal(cause, _error!.InnerException?.Message);

    [Then("the command should fail with no kind and the message {string}")]
    public void ThenTheCommandShouldFailWithNoKind(string message)
    {
        Assert.Null(ErrorKindException.KindOf(_error));
        Assert.Equal(message, _error!.Message);
    }
}
