using BuildingBlocks.Pipeline;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Xunit;

namespace BuildingBlocks.Tests.Steps;

/// <summary>What lives per command: the scenarios see which instance each command got, and when it ended.</summary>
public sealed class PerCommand(List<string> trace) : IDisposable
{
    public Guid Id { get; } = Guid.NewGuid();
    public void Dispose() => trace.Add($"scope of {Id} disposed");
}

/// <summary>Reports the <see cref="PerCommand"/> it ran with, and, when told to, sends a nested command first.</summary>
public sealed record ScopeCommand(bool Nested = false, bool Fails = false) : IRequest<IReadOnlyList<Guid>>;

public sealed class ScopeHandler(PerCommand perCommand, ISender sender) : IRequestHandler<ScopeCommand, IReadOnlyList<Guid>>
{
    public async Task<IReadOnlyList<Guid>> Handle(ScopeCommand command, CancellationToken cancellationToken)
    {
        if (command.Fails)
        {
            throw new InvalidOperationException("the command failed");
        }
        return command.Nested ? [perCommand.Id, .. await sender.Send(new ScopeCommand(), cancellationToken)] : [perCommand.Id];
    }
}

[Binding]
public sealed class CommandScopeSteps
{
    private readonly List<string> _trace = [];
    private readonly List<IReadOnlyList<Guid>> _answers = [];
    private ServiceProvider? _provider;
    private ISender? _sender;
    private Exception? _error;

    [Given("an application with the command scope")]
    public void GivenAnApplicationWithTheCommandScope()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_trace);
        services.AddScoped<PerCommand>();
        services.AddMediatR(mediatr => mediatr.RegisterServicesFromAssemblyContaining<ScopeHandler>());
        services.AddCommandScope();
        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        _sender = _provider.GetRequiredService<ISender>(); // from the root, as an entry point takes it
    }

    [When("a command is sent")]
    public async Task WhenACommandIsSent() => _answers.Add(await _sender!.Send(new ScopeCommand()));

    [When("a command that sends a nested command is sent")]
    public async Task WhenANestedCommandIsSent() => _answers.Add(await _sender!.Send(new ScopeCommand(Nested: true)));

    [When("a command that fails is sent")]
    public async Task WhenACommandThatFailsIsSent() =>
        _error = await Record.ExceptionAsync(() => _sender!.Send(new ScopeCommand(Fails: true)));

    [When("two commands are sent at once")]
    public async Task WhenTwoCommandsAreSentAtOnce() =>
        _answers.AddRange(await Task.WhenAll(_sender!.Send(new ScopeCommand()), _sender.Send(new ScopeCommand())));

    [When("a stream is asked for")]
    public void WhenAStreamIsAskedFor() =>
        _error = Record.Exception(() => _sender!.CreateStream(new ScopeCommand()));

    [Then("each command should have run in a scope of its own")]
    public void ThenEachCommandShouldHaveRunInAScopeOfItsOwn() =>
        Assert.Equal(_answers.Count, _answers.SelectMany(ids => ids).Distinct().Count());

    [Then("the nested command should have run in its parent's scope")]
    public void ThenTheNestedCommandShouldHaveRunInItsParentsScope() =>
        Assert.Single(_answers.Single().Distinct());

    [Then("{int} scope(s) should have been disposed")]
    public void ThenScopesShouldHaveBeenDisposed(int count) => Assert.Equal(count, _trace.Count);

    [Then("the failure should reach the sender")]
    public void ThenTheFailureShouldReachTheSender() => Assert.IsType<InvalidOperationException>(_error);

    [Then("the stream should be refused")]
    public void ThenTheStreamShouldBeRefused() => Assert.IsType<NotSupportedException>(_error);

    [AfterScenario]
    public async Task StopTheApplication()
    {
        if (_provider is not null)
        {
            await _provider.DisposeAsync();
        }
    }
}
