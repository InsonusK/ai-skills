using Ardalis.Result;
using BuildingBlocks.MediatR;
using FluentValidation;
using MediatR;
using Shared;
using Xunit;

namespace BuildingBlocks.Tests;

public class ValidationBehaviorTests
{
    private class DummyCommand : ICommand<string>;

    private class FailingValidator : AbstractValidator<DummyCommand>
    {
        public FailingValidator()
            => RuleFor(x => x).Must(_ => false).WithMessage("Validation failed.");
    }

    [Fact]
    public async Task Handle_WithFailingValidator_ReturnsInvalidResult()
    {
        var behavior = new ValidationBehavior<DummyCommand, Result<string>>(new[] { new FailingValidator() });

        var result = await behavior.Handle(
            new DummyCommand(),
            () => Task.FromResult(Result.Success("ok")),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage == "Validation failed.");
    }

    [Fact]
    public async Task Handle_WithNoValidators_ProceedsToNext()
    {
        var behavior = new ValidationBehavior<DummyCommand, Result<string>>(Array.Empty<IValidator<DummyCommand>>());

        var result = await behavior.Handle(
            new DummyCommand(),
            () => Task.FromResult(Result.Success("ok")),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);
    }
}
