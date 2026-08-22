using Ardalis.Result;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;
using Shared;
using Xunit;

namespace Sample.Interfaces.Tests;

public class CreateTaskCommandTests
{
    [Fact]
    public void CreateTaskCommand_ImplementsICommandOfResult()
    {
        var command = new CreateTaskCommand("Title", 1, new SoftEmail("user@example.com"));

        Assert.IsAssignableFrom<ICommand<Result<CreateTaskResult>>>(command);
    }
}
