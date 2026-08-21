using Sample.Application.Handlers;
using Sample.Interfaces.Commands;
using Xunit;

namespace Sample.Application.Tests;

public class GreetCommandHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsGreeting()
    {
        var handler = new GreetCommandHandler();

        var result = await handler.Handle(new GreetCommand("World"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Hello, World!", result.Value);
    }
}
