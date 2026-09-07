using Ardalis.Result;
using BuildingBlocks.MediatR;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Reqnroll;
using Shared.MediatR;
using Xunit;

namespace BuildingBlocks.Tests.StepDefinitions;

[Binding]
public sealed class PipelineSteps
{
    private record Req(string Value) : ICommand<Result<string>>;

    private sealed class RejectingValidator : AbstractValidator<Req>
    {
        public RejectingValidator() =>
            RuleFor(x => x.Value).Must(_ => false).WithMessage("rejected");
    }

    private bool _handlerInvoked;
    private Result<string> _validationResult = null!;
    private Result<string> _exceptionResult = null!;

    [Given("a pipeline with ValidationBehavior and a validator that rejects the request")]
    public void GivenValidationPipeline() { }

    [When("the request goes through the pipeline")]
    public async Task WhenValidationPipeline()
    {
        var behavior = new ValidationBehavior<Req, Result<string>>([new RejectingValidator()]);
        _validationResult = await behavior.Handle(
            new Req("x"),
            () => { _handlerInvoked = true; return Task.FromResult(Result<string>.Success("ok")); },
            CancellationToken.None);
    }

    [Then("the handler is not invoked")]
    public void ThenHandlerNotInvoked() => Assert.False(_handlerInvoked);

    [Then("the result is invalid")]
    public void ThenInvalid() => Assert.Equal(ResultStatus.Invalid, _validationResult.Status);

    [Given("a pipeline with ExceptionHandlingBehavior")]
    public void GivenExceptionPipeline() { }

    [When("an inner step throws an exception")]
    public async Task WhenInnerThrows()
    {
        var behavior = new ExceptionHandlingBehavior<Req, Result<string>>(
            NullLogger<ExceptionHandlingBehavior<Req, Result<string>>>.Instance);
        _exceptionResult = await behavior.Handle(
            new Req("x"),
            () => throw new InvalidOperationException("boom"),
            CancellationToken.None);
    }

    [Then("the result is an error with message {string}")]
    public void ThenErrorMessage(string message)
    {
        Assert.Equal(ResultStatus.Error, _exceptionResult.Status);
        Assert.Equal(message, _exceptionResult.Errors.First());
    }
}
