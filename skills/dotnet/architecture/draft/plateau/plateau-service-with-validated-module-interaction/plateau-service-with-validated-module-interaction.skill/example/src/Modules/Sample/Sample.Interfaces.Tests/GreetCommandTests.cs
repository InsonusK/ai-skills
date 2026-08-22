using Sample.Interfaces.Commands;
using Shared;
using Xunit;

namespace Sample.Interfaces.Tests;

public class GreetCommandTests
{
    [Fact]
    public void GreetCommand_ImplementsICommandOfString()
    {
        var command = new GreetCommand("World");

        Assert.IsAssignableFrom<ICommand<string>>(command);
    }
}
