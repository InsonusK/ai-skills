---
description: Addressable property controller
project_name: "{Module}.Api"
name: "Single{Entity}{Property}Controller.cs"
change_kind: create
---

# Goals
- Handle setting and unsetting one addressable boolean or optional property on an entity
- Route: `/{entity}/{id}/{property-name}` — kebab-case property name

# Core Principles
- `POST /{entity}/{id}/{property}` → set the property → `Set{Entity}{Property}Command`
- `DELETE /{entity}/{id}/{property}` → unset/clear the property → `Unset{Entity}{Property}Command`
- Used when a property has meaningful set/unset semantics — e.g. `is-complete`, `is-archived`

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Property controller | `Single{Entity}{Property}Controller` | `SingleTaskIsCompleteController` | `Single{Entity}{Property}Controller.cs` | `SingleTaskIsCompleteController.cs` |

# Implementation changes

```csharp
// {Module}.Api/Controllers/{Entity}/Single{Entity}{Property}Controller.cs
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using {Module}.Api.Extensions;
using {Module}.Interfaces.Commands;

namespace {Module}.Api.Controllers.{Entity};

[ApiController]
[Route("{entity}/{id:int}/{property-name}")]
public sealed class Single{Entity}{Property}Controller : ControllerBase
{
    private readonly ISender _sender;

    public Single{Entity}{Property}Controller(ISender sender)
        => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Set(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new Set{Entity}{Property}Command(id), ct);

        return result.Status switch
        {
            ResultStatus.NoContent => NoContent(),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Conflict => Conflict(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for Set{Entity}{Property}Command.")
        };
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Unset(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new Unset{Entity}{Property}Command(id), ct);

        return result.Status switch
        {
            ResultStatus.NoContent => NoContent(),
            ResultStatus.NotFound => NotFound(
                ResultExtensions.ToProblemDetails(result.Errors)),
            ResultStatus.Error => StatusCode(
                StatusCodes.Status500InternalServerError,
                ResultExtensions.ToProblemDetails(result.Errors)),
            _ => throw new InvalidOperationException(
                $"Unexpected result status '{result.Status}' for Unset{Entity}{Property}Command.")
        };
    }
}
```

# Rules

MUST:
- Named `Single{Entity}{Property}Controller`
- Route: `[Route("{entity}/{id:int}/{property-name}")]` — property name in kebab-case
- `[HttpPost]` sets the property, `[HttpDelete]` unsets it

MUST NOT:
- Handle properties without set/unset semantics

# Anti-patterns
- Using PUT instead of POST/DELETE for boolean property toggles
- Property name in camelCase in the route — use kebab-case

# Check list
- [ ] Named `Single{Entity}{Property}Controller`
- [ ] Route uses kebab-case property name
- [ ] POST dispatches `Set{Entity}{Property}Command`
- [ ] DELETE dispatches `Unset{Entity}{Property}Command`
