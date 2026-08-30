using Ardalis.Result;
using BuildingBlocks.MediatR;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Reqnroll;
using Shared;

namespace BuildingBlocks.Tests.StepDefinitions;

[Binding]
public sealed class ExceptionHandlingBehaviorSteps
{
    private ExceptionHandlingBehavior<GreetCommand, Result<string>>? _behavior;
    private Result<string>? _result;

    [Given("a MediatR pipeline with ExceptionHandlingBehavior")]
    public void GivenAPipeline()
    {
        _behavior = new ExceptionHandlingBehavior<GreetCommand, Result<string>>(
            NullLogger<ExceptionHandlingBehavior<GreetCommand, Result<string>>>.Instance);
    }

    [When("an inner behavior throws an exception")]
    public async Task WhenThrows()
    {
        _result = await _behavior!.Handle(
            new GreetCommand("any"),
            () => throw new InvalidOperationException("boom"),
            CancellationToken.None);
    }

    [Then("the result is an error with message \"([^\"]*)\")]
    public void ThenError(string message)
    {
        Assert.True(!_result!.IsSuccess);
        Assert.Equal(message, _result.Errors.First());
    }

    private record GreetCommand(string Name) : ICommand<Result<string>>;
}
