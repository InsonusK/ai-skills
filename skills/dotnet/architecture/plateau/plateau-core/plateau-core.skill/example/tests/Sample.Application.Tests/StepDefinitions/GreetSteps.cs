using MediatR;
using Reqnroll;
using Sample.Application.Features.Greet;
using Sample.Application.Validators.Property;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;
using Xunit;

namespace Sample.Application.Tests.StepDefinitions;

[Binding]
public sealed class GreetSteps
{
    private sealed class NoopPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken ct = default) => Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken ct = default)
            where TNotification : INotification => Task.CompletedTask;
    }

    private string _message = "";
    private readonly GreetingStore _store = new();
    private string? _rendered;
    private bool _validationFailed;
    private string? _failedCode;

    [Given("the greeting message {string}")]
    public void GivenMessage(string message) => _message = message;

    [When("the greet command is handled")]
    public async Task WhenHandled()
    {
        var handler = new GreetHandler(new NoopPublisher(), _store);
        var result = await handler.Handle(new GreetCommand(new SoftGreeting(_message)), CancellationToken.None);
        _rendered = result.Value.Rendered;
    }

    [Then("the rendered result is {string}")]
    public void ThenRendered(string expected) => Assert.Equal(expected, _rendered);

    [Then("the stored last greeting is {string}")]
    public void ThenStored(string expected) => Assert.Equal(expected, _store.Last!.Rendered);

    [When("the greeting property validator runs")]
    public void WhenValidatorRuns()
    {
        var result = new SoftGreetingPropertyValidator().Validate(new SoftGreeting(_message));
        _validationFailed = !result.IsValid;
        _failedCode = result.Errors.FirstOrDefault()?.ErrorCode;
    }

    [Then("validation fails with error code {string}")]
    public void ThenValidationFails(string code)
    {
        Assert.True(_validationFailed);
        Assert.Equal(code, _failedCode);
    }
}
