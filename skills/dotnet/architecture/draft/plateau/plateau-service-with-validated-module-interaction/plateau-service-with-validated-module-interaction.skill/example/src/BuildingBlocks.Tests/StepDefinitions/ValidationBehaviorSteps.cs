using Ardalis.Result;
using BuildingBlocks.MediatR;
using FluentValidation;
using MediatR;
using Reqnroll;
using Shared;
using Xunit;

namespace BuildingBlocks.Tests.StepDefinitions;

[Binding]
public sealed class ValidationBehaviorSteps
{
    private ValidationBehavior<DummyCommand, Result<string>>? _behavior;
    private Result<string>? _result;
    private bool _nextWasCalled;

    [Given("a MediatR pipeline with ValidationBehavior")]
    public void GivenAPipeline()
    {
        _nextWasCalled = false;
    }

    [When("a registered validator fails for the request")]
    public async Task WhenValidatorFails()
    {
        var validator = new FailingValidator();
        _behavior = new ValidationBehavior<DummyCommand, Result<string>>(new[] { validator });
        _result = await _behavior.Handle(
            new DummyCommand(),
            () =>
            {
                _nextWasCalled = true;
                return Task.FromResult(Result.Success("ok"));
            },
            CancellationToken.None);
    }

    [When("no validators are registered for the request")]
    public async Task WhenNoValidators()
    {
        _behavior = new ValidationBehavior<DummyCommand, Result<string>>(Array.Empty<IValidator<DummyCommand>>());
        _result = await _behavior.Handle(
            new DummyCommand(),
            () =>
            {
                _nextWasCalled = true;
                return Task.FromResult(Result.Success("ok"));
            },
            CancellationToken.None);
    }

    [Then("the result status is Invalid")]
    public void ThenStatusInvalid()
    {
        Assert.NotNull(_result);
        Assert.False(_result!.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, _result.Status);
        Assert.False(_nextWasCalled);
    }

    [Then("the pipeline reaches the next behavior")]
    public void ThenReachesNext()
    {
        Assert.NotNull(_result);
        Assert.True(_result!.IsSuccess);
        Assert.True(_nextWasCalled);
    }

    private record DummyCommand : ICommand<Result<string>>;

    private class FailingValidator : AbstractValidator<DummyCommand>
    {
        public FailingValidator()
            => RuleFor(x => x).Must(_ => false).WithMessage("Validation failed.");
    }
}
