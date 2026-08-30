using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Api.Extensions;

public static class ResultExtensions
{
    public static ProblemDetails ToProblemDetails(this Ardalis.Result.Result result) => result.Status switch
    {
        ResultStatus.NotFound => new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Not Found", Detail = string.Join("; ", result.Errors) },
        ResultStatus.Invalid => new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "Validation Failed", Detail = string.Join("; ", result.ValidationErrors.Select(e => e.ErrorMessage)) },
        ResultStatus.Conflict => new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Conflict", Detail = string.Join("; ", result.Errors) },
        ResultStatus.Unauthorized => new ProblemDetails { Status = StatusCodes.Status401Unauthorized, Title = "Unauthorized" },
        ResultStatus.Forbidden => new ProblemDetails { Status = StatusCodes.Status403Forbidden, Title = "Forbidden" },
        ResultStatus.Error or ResultStatus.CriticalError => new ProblemDetails { Status = StatusCodes.Status500InternalServerError, Title = "An unexpected error occurred. Please try again later." },
        _ => throw new InvalidOperationException($"Unhandled ResultStatus '{result.Status}' — every status this module's handlers can return must be mapped here explicitly.")
    };
}
