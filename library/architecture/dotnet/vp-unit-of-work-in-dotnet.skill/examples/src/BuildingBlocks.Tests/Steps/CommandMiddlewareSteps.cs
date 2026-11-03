using BuildingBlocks.Pipeline;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Shared.Errors;
using Xunit;
using Xunit.Abstractions;

namespace BuildingBlocks.Tests.Steps;

/// <summary>A command of the middleware scenarios, answered by <see cref="EchoHandler"/>.</summary>
public sealed record EchoCommand(string Text) : IRequest<string>;

/// <summary>What the middleware and the handler of a scenario did, in order.</summary>
public sealed class CallTrace
{
    public List<string> Calls { get; } = [];
}

public sealed class EchoHandler(CallTrace trace) : IRequestHandler<EchoCommand, string>
{
    public Task<string> Handle(EchoCommand command, CancellationToken cancellationToken)
    {
        trace.Calls.Add($"handler {command.Text}");
        return Task.FromResult($"planned {command.Text}");
    }
}

// The middleware of the scenarios: each records "{name} in" and "{name} out"
// around the rest of the chain; the guard rejects every command.
public sealed class OuterMiddleware<TCommand, TResult>(CallTrace trace) : IPipelineBehavior<TCommand, TResult>
    where TCommand : notnull
{
    public Task<TResult> Handle(TCommand command, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken) =>
        Recorded.Around(trace, "outer", next, cancellationToken);
}

public sealed class InnerMiddleware<TCommand, TResult>(CallTrace trace) : IPipelineBehavior<TCommand, TResult>
    where TCommand : notnull
{
    public Task<TResult> Handle(TCommand command, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken) =>
        Recorded.Around(trace, "inner", next, cancellationToken);
}

public sealed class GuardMiddleware<TCommand, TResult>(CallTrace trace) : IPipelineBehavior<TCommand, TResult>
    where TCommand : notnull
{
    public Task<TResult> Handle(TCommand command, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        trace.Calls.Add("guard in");
        throw new ErrorKindException(ErrorKind.PermissionDenied, "rejected by guard");
    }
}

internal static class Recorded
{
    public static async Task<TResult> Around<TResult>(CallTrace trace, string name, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        trace.Calls.Add($"{name} in");
        try
        {
            return await next(cancellationToken);
        }
        finally
        {
            trace.Calls.Add($"{name} out");
        }
    }
}

[Binding]
public sealed class CommandMiddlewareSteps(ITestOutputHelper output)
{
    private static readonly Dictionary<string, Type> Middlewares = new()
    {
        ["outer passes"] = typeof(OuterMiddleware<,>),
        ["inner passes"] = typeof(InnerMiddleware<,>),
        ["guard rejects"] = typeof(GuardMiddleware<,>),
    };

    private readonly CallTrace _trace = new();
    private Type[] _middlewares = [];
    private string _result = "";
    private Exception? _error;

    [Given("a handler that succeeds, wrapped by the middleware:")]
    public void GivenAHandlerWrappedBy(DataTable table) =>
        _middlewares = table.Rows.Select(row => Middlewares[$"{row["middleware"]} {row["does"]}"]).ToArray();

    [When("the wrapped handler handles the command {string}")]
    public async Task WhenTheWrappedHandlerHandles(string text)
    {
        // As the composition root registers them: the handlers, then the list.
        var services = new ServiceCollection()
            .AddSingleton(_trace)
            .AddMediatR(mediatr => mediatr.RegisterServicesFromAssemblyContaining<EchoHandler>())
            .AddCommandMiddleware(_middlewares)
            .BuildServiceProvider();
        try
        {
            _result = await services.GetRequiredService<ISender>().Send(new EchoCommand(text));
        }
        catch (ErrorKindException error)
        {
            _error = error;
        }
        output.WriteLine($"when: send {text} -> result '{_result}', error '{_error?.Message}', calls [{string.Join(", ", _trace.Calls)}]");
    }

    [When("the type {string} is listed as a middleware")]
    public void WhenTheTypeIsListed(string name)
    {
        _error = Record.Exception(() => new ServiceCollection().AddCommandMiddleware(Type.GetType($"System.{name}")!));
        output.WriteLine($"when: list {name} -> {_error?.Message}");
    }

    [Then("the calls should be {string}")]
    public void ThenTheCallsShouldBe(string calls) => Assert.Equal(calls, string.Join(", ", _trace.Calls));

    [Then("the result should be {string}")]
    public void ThenTheResultShouldBe(string result) => Assert.Equal(result, _result);

    [Then("the error should be {string}")]
    public void ThenTheErrorShouldBe(string message) => Assert.Equal(message, _error?.Message);

    [Then("listing should fail mentioning {string}")]
    public void ThenListingShouldFail(string text) => Assert.Contains(text, Assert.IsType<ArgumentException>(_error).Message);
}
