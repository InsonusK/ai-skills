---
name: class-single-entity-property-controller
description: Addressable property controller
domain: skill
type: template
version: 20260616
plateau: default
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication.skill]]"
---

# Goal
- Handle setting and unsetting one addressable boolean or optional property on an entity
- Route: `/{entity}/{id}/{property-name}` — kebab-case property name

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md|Single{Entity}{Property}Controller.cs.create]]

# Core Principals
- `POST /{entity}/{id}/{property}` → set the property → `Set{Entity}{Property}Command`
- `DELETE /{entity}/{id}/{property}` → unset/clear the property → `Unset{Entity}{Property}Command`
- Used when a property has meaningful set/unset semantics — e.g. `is-complete`, `is-archived`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md|Single{Entity}{Property}Controller.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Property controller | `Single{Entity}{Property}Controller` | `SingleTaskIsCompleteController` | `Single{Entity}{Property}Controller.cs` | `SingleTaskIsCompleteController.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md|Single{Entity}{Property}Controller.cs.create]]

# Implementation
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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md|Single{Entity}{Property}Controller.cs.create]]

# Rules
MUST:
	- Named `Single{Entity}{Property}Controller`
	- Route: `[Route("{entity}/{id:int}/{property-name}")]` — property name in kebab-case
	- `[HttpPost]` sets the property, `[HttpDelete]` unsets it
MUST NOT:
	- Handle properties without set/unset semantics

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md|Single{Entity}{Property}Controller.cs.create]]

# Anti-patterns
- Using PUT instead of POST/DELETE for boolean property toggles
- Property name in camelCase in the route — use kebab-case

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md|Single{Entity}{Property}Controller.cs.create]]

# Check list
- [ ] Named `Single{Entity}{Property}Controller`
- [ ] Route uses kebab-case property name
- [ ] POST dispatches `Set{Entity}{Property}Command`
- [ ] DELETE dispatches `Unset{Entity}{Property}Command`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md|Single{Entity}{Property}Controller.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Handle setting and unsetting one addressable boolean or optional property on an entity
- [ ] WHEN applied THEN Route: /{entity}/{id}/{property-name} — kebab-case property name
- [ ] WHEN applied THEN POST /{entity}/{id}/{property} → set the property → Set{Entity}{Property}Command
- [ ] WHEN applied THEN DELETE /{entity}/{id}/{property} → unset/clear the property → Unset{Entity}{Property}Command
- [ ] WHEN applied THEN Used when a property has meaningful set/unset semantics — e.g. is-complete, is-archived
- [ ] WHEN verified THEN Named Single{Entity}{Property}Controller
- [ ] WHEN verified THEN Route uses kebab-case property name
- [ ] WHEN verified THEN POST dispatches Set{Entity}{Property}Command
- [ ] WHEN verified THEN DELETE dispatches Unset{Entity}{Property}Command
- [ ] WHEN naming 'Property controller' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/Single{Entity}{Property}Controller.cs.create.md|Single{Entity}{Property}Controller.cs.create]]
