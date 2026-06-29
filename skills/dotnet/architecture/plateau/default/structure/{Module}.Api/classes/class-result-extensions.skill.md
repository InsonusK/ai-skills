---
name: class-result-extensions
description: ToProblemDetails helper for Result error mapping
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication.skill]]"
---

# Goal
- Provide a shared `ToProblemDetails` helper that converts `Result` validation errors and error messages into `ProblemDetails`
- Keep error mapping consistent across all controllers in the module

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md|ResultExtensions.cs.create]]

# Core Principals
- Apply ONE plateau template per class
- `ProblemDetails` is the only error shape returned from the API
- Validation errors grouped by identifier into a dictionary
- Plain error messages returned as an array

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md|ResultExtensions.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Result extensions | `ResultExtensions` | `ResultExtensions` | `ResultExtensions.cs` | `ResultExtensions.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md|ResultExtensions.cs.create]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-result-extensions
//Plateau: default
//Version: 20260628
```

```csharp
// {Module}.Api/Extensions/ResultExtensions.cs
using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;

namespace {Module}.Api.Extensions;

public static class ResultExtensions
{
    public static ProblemDetails ToProblemDetails(
        IEnumerable<ValidationError> validationErrors)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        details.Extensions["errors"] = validationErrors
            .GroupBy(e => e.Identifier)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return details;
    }

    public static ProblemDetails ToProblemDetails(IEnumerable<string> errors)
    {
        var details = new ProblemDetails
        {
            Title = "An error occurred",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        details.Extensions["errors"] = errors.ToArray();
        return details;
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md|ResultExtensions.cs.create]]

# Rules
MUST:
	- `ToProblemDetails(IEnumerable<ValidationError>)` returns 400 `ProblemDetails` with grouped errors
	- `ToProblemDetails(IEnumerable<string>)` returns `ProblemDetails` with error array
	- Both methods set the `errors` extension key
MUST NOT:
	- Return custom error shapes — only `ProblemDetails`
	- Use `ValidationFailure` directly — map from `ValidationError`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md|ResultExtensions.cs.create]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- Inline `ProblemDetails` construction in every controller action instead of using `ResultExtensions`
- Returning raw strings as error bodies

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md|ResultExtensions.cs.create]]

# Check list
- [ ] `ResultExtensions.cs` exists in `/Extensions`
- [ ] `ToProblemDetails` handles both validation errors and plain error messages
- [ ] Errors grouped by identifier for validation errors

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md|ResultExtensions.cs.create]]

# Unittest TestCases
- [ ] WHEN component is requested THEN it provide a shared ToProblemDetails helper that converts Result validation errors and error messages into ProblemDetails
- [ ] WHEN applied THEN Keep error mapping consistent across all controllers in the module
- [ ] WHEN applied THEN ProblemDetails is the only error shape returned from the API
- [ ] WHEN applied THEN Validation errors grouped by identifier into a dictionary
- [ ] WHEN applied THEN Plain error messages returned as an array
- [ ] WHEN verified THEN ResultExtensions.cs exists in /Extensions
- [ ] WHEN verified THEN ToProblemDetails handles both validation errors and plain error messages
- [ ] WHEN verified THEN Errors grouped by identifier for validation errors
- [ ] WHEN naming 'Result extensions' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/ResultExtensions.cs.create.md|ResultExtensions.cs.create]]
