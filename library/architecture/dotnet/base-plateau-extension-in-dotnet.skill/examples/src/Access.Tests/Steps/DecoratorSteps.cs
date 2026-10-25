using MediatR;
using Reqnroll;
using Shared.Access;
using Shared.Errors;
using Xunit;
using Xunit.Abstractions;

namespace Access.Tests.Steps;

// The commands of these scenarios: one with each mark, one with none, so the
// decorators are tested without any command of the application. Each is named
// EchoCommand, so the decorators call it "Echo".
public static class Restricted
{
    public sealed record EchoCommand(string Role) : IRequest<string>, IRestricted
    {
        public string RequiredRole => Role;
    }
}

public static class Batch
{
    public sealed record EchoCommand(int Size) : IRequest<string>, IBatch
    {
        public int BatchSize => Size;
    }
}

public static class Plain
{
    public sealed record EchoCommand : IRequest<string>;
}

[Binding]
public sealed class DecoratorSteps(ITestOutputHelper output)
{
    private Identity? _caller;
    private int _limit;
    private bool _handled;
    private ErrorKindException? _error;

    // Runs the command through the decorator to a handler that only records
    // that it ran - with the caller in the context when the scenario has one.
    private async Task HandleAsync<TCommand>(IPipelineBehavior<TCommand, string> decorator, TCommand command)
        where TCommand : notnull
    {
        Task<string> Handler(CancellationToken cancellationToken)
        {
            _handled = true;
            return Task.FromResult("done");
        }

        using (_caller is null ? null : CallerIdentity.Begin(_caller))
        {
            try
            {
                await decorator.Handle(command, Handler, CancellationToken.None);
            }
            catch (ErrorKindException error)
            {
                _error = error;
            }
        }
        output.WriteLine($"when: {command} -> handled {_handled}, error '{_error?.Message}'");
    }

    [Given("the caller {string} with the roles {string}")]
    public void GivenTheCaller(string user, string roles) =>
        _caller = new Identity(user, roles == "" ? [] : roles.Split(','));

    [Given("no caller")]
    public void GivenNoCaller() => _caller = null;

    [Given("the batch limit {int}")]
    public void GivenTheBatchLimit(int limit) => _limit = limit;

    [When("a command restricted to {string} goes through the access check")]
    public Task WhenARestrictedCommand(string role) =>
        HandleAsync(new AccessCheckBehavior<Restricted.EchoCommand, string>(), new Restricted.EchoCommand(role));

    [When("a command without a mark goes through the access check")]
    public Task WhenAnUnmarkedCommandIsChecked() =>
        HandleAsync(new AccessCheckBehavior<Plain.EchoCommand, string>(), new Plain.EchoCommand());

    [When("a batch of {int} items goes through the batch limit")]
    public Task WhenABatch(int size) =>
        HandleAsync(new BatchLimitBehavior<Batch.EchoCommand, string>(Limit()), new Batch.EchoCommand(size));

    [When("a command without a mark goes through the batch limit")]
    public Task WhenAnUnmarkedCommandIsLimited() =>
        HandleAsync(new BatchLimitBehavior<Plain.EchoCommand, string>(Limit()), new Plain.EchoCommand());

    private AccessConfig Limit() => new(new Dictionary<string, Identity>(), _limit);

    [Then("the handler should have run")]
    public void ThenTheHandlerShouldHaveRun()
    {
        Assert.Null(_error);
        Assert.True(_handled);
    }

    [Then("the handler should not have run")]
    public void ThenTheHandlerShouldNotHaveRun() => Assert.False(_handled);

    [Then(@"^the call should be rejected as (unauthenticated|permission denied|resource exhausted) with ""(.*)""$")]
    public void ThenTheCallShouldBeRejected(string kind, string message)
    {
        var want = kind switch
        {
            "unauthenticated" => ErrorKind.Unauthenticated,
            "permission denied" => ErrorKind.PermissionDenied,
            _ => ErrorKind.ResourceExhausted,
        };
        Assert.NotNull(_error);
        Assert.Equal((want, message), (_error.Kind, _error.Message));
    }
}
