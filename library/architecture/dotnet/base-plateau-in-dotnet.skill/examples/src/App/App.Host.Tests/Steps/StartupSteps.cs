using App.Host.Api.Entry;
using App.Host.Api.Startup;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Todo.Application.Commands;
using Todo.Interfaces.Commands;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps;

/// <summary>A command of the scenarios whose handler fails for a reason with no kind.</summary>
public sealed record FailingCommand(string Message) : IRequest<string>;

public sealed class FailingCommandHandler : IRequestHandler<FailingCommand, string>
{
    public Task<string> Handle(FailingCommand command, CancellationToken cancellationToken) =>
        throw new InvalidOperationException(command.Message);
}

[Binding]
public sealed class StartupSteps(ProcessOutcome outcome, ITestOutputHelper output)
{
    private readonly StringWriter _output = new();
    private IReadOnlyList<string>? _input;

    // The handlers as the composition root registers them: the module's, and
    // the failing one of these scenarios.
    private static ISender Sender() =>
        new ServiceCollection()
            .AddMediatR(mediatr => mediatr.RegisterServicesFromAssemblies(typeof(PlanItemsHandler).Assembly, typeof(FailingCommandHandler).Assembly))
            .BuildServiceProvider()
            .GetRequiredService<ISender>();

    private async Task Start<TResult>(IRequest<TResult> command)
    {
        Exception? error = null;
        try
        {
            await StartupEntry.RunAsync(Sender(), command, _output, CancellationToken.None);
        }
        catch (Exception thrown)
        {
            error = thrown;
        }
        outcome.ExitCode = EntryRunner.ExitCode(error);
        output.WriteLine($"when: RunAsync({command}) -> {error?.Message ?? "ok"}, exit {outcome.ExitCode}");
    }

    [When("the process starts with PlanItems for the titles {string}")]
    public Task WhenTheProcessStartsWithPlanItems(string titles) => Start(new PlanItems(titles.Split('|')));

    [When("the process starts with a command whose handler fails with {string}")]
    public Task WhenTheProcessStartsWithAFailingCommand(string message) => Start(new FailingCommand(message));

    [Then("the output should be the JSON line {string}")]
    public void ThenTheOutputShouldBe(string line) => Assert.Equal(line + Environment.NewLine, _output.ToString());

    [Then("the output should be empty")]
    public void ThenTheOutputShouldBeEmpty() => Assert.Equal("", _output.ToString());

    [When("the startup input is read from {string}")]
    public void WhenTheStartupInputIsRead(string name)
    {
        _input = StartupEntry.EnvList(name);
        output.WriteLine($"when: EnvList({name}) -> [{string.Join(", ", _input)}]");
    }

    [Then("the startup input should be {string}")]
    public void ThenTheStartupInputShouldBe(string titles) => Assert.Equal(titles.Split('|'), _input);

    [Then("the startup input should be empty")]
    public void ThenTheStartupInputShouldBeEmpty() => Assert.Empty(_input!);
}
