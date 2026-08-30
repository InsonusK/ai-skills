---
description: Maps Ardalis.Result's ResultStatus to ProblemDetails — the single place HTTP status codes and error shapes are decided for this module's API
project_name: "{Module}.Api"
name: "ResultExtensions.cs"
element_kind: class
change_kind: create
tags:
  - solution/http-api-publication
  - element/result-extensions-cs
---

# Goals
- Give every controller one shared, tested mapping from a failed `Result`/`Result<T>` to a `ProblemDetails` response — never hand-rolled per action
- Make an undocumented `ResultStatus` a build-visible defect (`InvalidOperationException`), not a silently-wrong 500

# Core Principles
- Only failure statuses are mapped here — the success path (`Ok`, `Created`, `NoContent`) is handled directly in each action, since the success shape differs per endpoint (a body, a location header, nothing)
- One `switch` expression, one arm per `ResultStatus` this module's handlers can actually return — never a generic `_ => 500` catch-all that would silently swallow a status nobody thought about

# Implementation changes

```csharp
// {Module}.Api/Extensions/ResultExtensions.cs
using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;

namespace {Module}.Api.Extensions;

public static class ResultExtensions
{
    public static ProblemDetails ToProblemDetails(this Ardalis.Result.Result result) => result.Status switch
    {
        ResultStatus.NotFound => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Not Found",
            Detail = string.Join("; ", result.Errors)
        },
        ResultStatus.Invalid => new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed",
            Detail = string.Join("; ", result.ValidationErrors.Select(e => e.ErrorMessage))
        },
        ResultStatus.Conflict => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Detail = string.Join("; ", result.Errors)
        },
        ResultStatus.Unauthorized => new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized"
        },
        ResultStatus.Forbidden => new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden"
        },
        ResultStatus.Error or ResultStatus.CriticalError => new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred. Please try again later."
        },
        _ => throw new InvalidOperationException(
            $"Unhandled ResultStatus '{result.Status}' — every status this module's handlers can return must be mapped here explicitly.")
    };
}
```

Usage in a controller — the failure branch of every action looks identical:

```csharp
if (!result.IsSuccess)
{
    var problem = result.ToProblemDetails();
    return StatusCode(problem.Status ?? StatusCodes.Status500InternalServerError, problem);
}
```

# Rule changes

## MUST
- Cover every `ResultStatus` this module's handlers can actually return, one arm each
- `default`/unmatched arm throw `InvalidOperationException` naming the unhandled status

## MUST NOT
- Include a status this module's handlers never return — dead arms drift from reality the same way an unmapped one does
- Return the raw exception message or stack trace in any `ProblemDetails.Detail`

# Check list
- [ ] Every `ResultStatus` value returned anywhere in this module's handlers has a matching `switch` arm
- [ ] Unmatched status throws `InvalidOperationException`, never falls through to a generic 500
- [ ] Called from every controller's failure branch — no controller re-implements this mapping inline
