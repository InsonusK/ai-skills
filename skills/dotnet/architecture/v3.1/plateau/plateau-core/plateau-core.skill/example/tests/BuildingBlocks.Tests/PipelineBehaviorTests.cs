using Ardalis.Result;
using BuildingBlocks.MediatR;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BuildingBlocks.Tests;

public class PipelineBehaviorTests
{
    public record Ping(string Name) : IRequest<Result<string>>;
    private sealed class PingValidator : AbstractValidator<Ping> { public PingValidator() => RuleFor(x => x.Name).NotEmpty(); }

    [Fact]
    public async Task ValidationBehavior_short_circuits_with_Invalid()
    {
        var b = new ValidationBehavior<Ping, Result<string>>([new PingValidator()]);
        var r = await b.Handle(new Ping(""), () => Task.FromResult(Result.Success("handler ran")), default);
        Assert.Equal(ResultStatus.Invalid, r.Status);
    }

    [Fact]
    public async Task ExceptionHandlingBehavior_maps_a_throw_to_Error()
    {
        var b = new ExceptionHandlingBehavior<Ping, Result<string>>(NullLogger<ExceptionHandlingBehavior<Ping, Result<string>>>.Instance);
        var r = await b.Handle(new Ping("x"), () => throw new InvalidOperationException("boom"), default);
        Assert.Equal(ResultStatus.Error, r.Status);
    }
}
