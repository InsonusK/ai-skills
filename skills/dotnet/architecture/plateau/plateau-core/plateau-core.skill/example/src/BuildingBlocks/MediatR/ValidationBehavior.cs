using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace BuildingBlocks.MediatR;

// Collects every registered validator's failures and short-circuits with Result.Invalid
// before the handler runs. Contains no per-request condition.
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, ct))))
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count != 0)
            {
                var errors = failures.Select(f => new ValidationError(f.PropertyName, f.ErrorMessage, f.ErrorCode, ValidationSeverity.Error));
                return (TResponse)MakeInvalid(typeof(TResponse), errors);
            }
        }

        return await next();
    }

    private static object MakeInvalid(Type responseType, IEnumerable<ValidationError> errors)
    {
        // TResponse is Result or Result<T>; both expose a static Invalid(IEnumerable<ValidationError>)
        var invalid = responseType.GetMethod("Invalid", [typeof(IEnumerable<ValidationError>)])!;
        return invalid.Invoke(null, [errors])!;
    }
}
