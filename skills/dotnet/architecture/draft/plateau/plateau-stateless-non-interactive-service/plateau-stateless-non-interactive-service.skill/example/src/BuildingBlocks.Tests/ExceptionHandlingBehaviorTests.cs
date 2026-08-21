using Ardalis.Result;
using BuildingBlocks.MediatR;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Shared;
using Xunit;

namespace BuildingBlocks.Tests;

public class ExceptionHandlingBehaviorTests
{
    [Fact]
    public async Task Handle_WhenInnerBehaviorThrows_ReturnsErrorResult()
    {
        var behavior = new ExceptionHandlingBehavior<GreetCommand, Result<string>>(
            NullLogger<ExceptionHandlingBehavior<GreetCommand, Result<string>>>.Instance);

        var result = await behavior.Handle(
            new GreetCommand("any"),
            () => throw new InvalidOperationException("boom"),
            CancellationToken.None);

        Assert.True(result.IsError());
        Assert.Equal("An unexpected error occurred. Please try again later.", result.Errors.First());
    }

    private record GreetCommand(string Name) : ICommand<string>;
}
