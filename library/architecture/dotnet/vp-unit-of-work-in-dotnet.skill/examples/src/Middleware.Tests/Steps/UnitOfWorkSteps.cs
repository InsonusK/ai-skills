using MediatR;
using Middleware.UnitOfWork;
using Reqnroll;
using Shared.UnitOfWork;
using Xunit;

namespace Middleware.Tests.Steps;

/// <summary>The unit of work faked by hand: records what the middleware asked of it.</summary>
public sealed class FakeUnitOfWork : IUnitOfWork
{
    public bool AlreadyBegun { get; set; }
    public bool CommitFails { get; set; }
    public List<string> Calls { get; } = [];
    public bool CommitCouldBeCancelled { get; private set; }

    public bool TryBegin() => !AlreadyBegun;

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        Calls.Add("commit");
        CommitCouldBeCancelled = cancellationToken.CanBeCanceled;
        return CommitFails ? throw new InvalidOperationException("commit failed") : Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken)
    {
        Calls.Add("rollback");
        return Task.CompletedTask;
    }
}

[Binding]
public sealed class UnitOfWorkSteps
{
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly CancellationTokenSource _caller = new();
    private RequestHandlerDelegate<string> _handler = _ => Task.FromResult("done");
    private string? _result;
    private Exception? _error;

    [Given("a unit of work already begun by the outer command")]
    public void GivenAlreadyBegun() => _unitOfWork.AlreadyBegun = true;

    [Given("the handler fails with {string}")]
    public void GivenTheHandlerFails(string message) => _handler = _ => throw new InvalidOperationException(message);

    [Given("the commit fails")]
    public void GivenTheCommitFails() => _unitOfWork.CommitFails = true;

    [When("a command runs through the unit of work middleware")]
    public async Task WhenACommandRuns()
    {
        var middleware = new UnitOfWorkMiddleware<PlanItemsCommand, string>(_unitOfWork);
        try
        {
            _result = await middleware.Handle(new PlanItemsCommand("buy milk"), _handler, _caller.Token);
        }
        catch (Exception error)
        {
            _error = error;
        }
    }

    [Then("the unit of work should have been asked for {string}")]
    public void ThenTheUnitOfWorkShouldHaveBeenAskedFor(string calls) =>
        Assert.Equal(calls.Split('|'), _unitOfWork.Calls);

    [Then("the unit of work should have been asked for nothing")]
    public void ThenTheUnitOfWorkShouldHaveBeenAskedForNothing() => Assert.Empty(_unitOfWork.Calls);

    [Then("the command should answer its result")]
    public void ThenTheCommandShouldAnswerItsResult() => Assert.Equal("done", _result);

    [Then("the command should fail with {string}")]
    public void ThenTheCommandShouldFailWith(string message) => Assert.Equal(message, _error?.Message);

    [Then("the caller's cancellation should not reach the commit")]
    public void ThenTheCallersCancellationShouldNotReachTheCommit() => Assert.False(_unitOfWork.CommitCouldBeCancelled);
}
