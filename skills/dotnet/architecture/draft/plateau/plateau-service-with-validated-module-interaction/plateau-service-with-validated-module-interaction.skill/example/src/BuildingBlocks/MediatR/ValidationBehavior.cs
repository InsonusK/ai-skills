using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace BuildingBlocks.MediatR;

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var errors = _validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
            .ToList();

        if (errors.Count > 0)
            return (TResponse)Result.Invalid(errors);

        return await next();
    }
}
