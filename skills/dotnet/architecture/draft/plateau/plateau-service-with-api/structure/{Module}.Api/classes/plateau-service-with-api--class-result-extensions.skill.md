---
name: plateau-service-with-api--class-result-extensions
description: Class ResultExtensions in the service-with-api plateau
whenToUse: when a controller or Minimal API endpoint needs to map a failed Result to an HTTP ProblemDetails response
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/class
  - plateau/service-with-api
created_by:
  - "[[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]]"
---

# Goal
- Give every controller one shared, tested mapping from a failed `Result`/`Result<T>` to `ProblemDetails` — never hand-rolled per action

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md|ResultExtensions.cs.create]]

# Implementation
```csharp
//Skill: class-result-extensions
//Plateau: service-with-api
//Version: 20260825120000

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
```

See [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md|ResultExtensions.cs.create]] for the full rationale and usage pattern.

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md|ResultExtensions.cs.create]]

# Rules
MUST:
- Cover every `ResultStatus` this module's handlers can return; unmatched arm throws `InvalidOperationException`
MUST NOT:
- Leave a status this module's handlers never return mapped here (dead arm)

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md|ResultExtensions.cs.create]]

# Check list
- [ ] Every `ResultStatus` returned anywhere in this module has a matching arm
- [ ] Unmatched status throws, never falls through to a silent 500

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md|ResultExtensions.cs.create]]
