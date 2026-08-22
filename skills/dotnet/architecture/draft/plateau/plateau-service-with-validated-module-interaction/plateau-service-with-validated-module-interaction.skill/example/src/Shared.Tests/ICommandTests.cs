using Shared;
using Xunit;

namespace Shared.Tests;

public class ICommandTests
{
    [Fact]
    public void GreetCommand_IsICommandOfString()
    {
        var command = new GreetCommand("World");

        Assert.IsAssignableFrom<ICommand<string>>(command);
    }

    private record GreetCommand(string Name) : ICommand<string>;
}
