using Ardalis.Result;
using Sample.Application.Features.CreateTask;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;
using Xunit;

namespace Sample.Application.Tests;

public class CreateTaskHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ReturnsCreatedResult()
    {
        var handler = new CreateTaskHandler();
        var command = new CreateTaskCommand("Review example", 1, new SoftEmail("reviewer@example.com"));

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Created, result.Status);
        Assert.Equal(0, result.Value.Id);
    }
}
