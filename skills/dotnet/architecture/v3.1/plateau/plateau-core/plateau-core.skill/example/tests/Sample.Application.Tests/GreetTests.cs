using Ardalis.Result;
using FluentValidation;
using Sample.Application.Features.Greet;
using Sample.Application.Validators.Property;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;
using Xunit;

namespace Sample.Application.Tests;

public class GreetTests
{
    private sealed class NoOpPublisher : MediatR.IPublisher
    {
        public Task Publish(object notification, CancellationToken ct = default) => Task.CompletedTask;
        public Task Publish<T>(T notification, CancellationToken ct = default) where T : MediatR.INotification => Task.CompletedTask;
    }

    [Fact]
    public async Task Handler_renders_and_stores_the_greeting()
    {
        var store = new GreetingStore();
        var handler = new GreetHandler(new NoOpPublisher(), store);

        var result = await handler.Handle(new GreetCommand(new SoftGreeting("world")), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("Hello, world!", result.Value.Rendered);
        Assert.Equal("Hello, world!", store.Last!.Rendered);
    }

    [Theory]
    [InlineData("", "Sample.Greeting.Required")]
    [InlineData("x", null)]
    public async Task PropertyValidator_owns_the_condition(string value, string? expectedErrorCode)
    {
        var validator = new SoftGreetingPropertyValidator();
        var result = await validator.ValidateAsync(new SoftGreeting(value));
        if (expectedErrorCode is null)
            Assert.True(result.IsValid);
        else
            Assert.Contains(result.Errors, e => e.ErrorCode == expectedErrorCode);
    }
}
