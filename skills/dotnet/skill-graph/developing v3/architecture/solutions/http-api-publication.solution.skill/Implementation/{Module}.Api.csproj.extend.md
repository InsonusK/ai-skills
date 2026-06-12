---
description: Add controllers, extensions, and minimal API endpoints to {Module}.Api
name: "{Module}.Api.csproj"
element_kind: project
change_kind: extend
---

# Goals
- Own all HTTP endpoint definitions for this module — controllers and Minimal API endpoint groups
- Be the only project that translates HTTP input to MediatR requests and HTTP output from MediatR results

# Core Principles
- References only `{Module}.Interfaces` — command records, query records, and DTOs live there
- No business logic, validation logic, or persistence logic in any controller action
- One controller type per operation category — five types cover all entity lifecycle scenarios
- Minimal API used exclusively for system, webhook, batch, and cross-aggregate operations

# Structure

## Project Structure
```
/{Module}.Api
  /Controllers
    /{Entity}
      {Entity}Controller.cs               ← collection: POST + GET collection
      Single{Entity}Controller.cs         ← single entity: GET + PUT + PATCH + DELETE
      Single{Entity}{Property}Controller.cs ← addressable property: POST + DELETE
      /{RelatedEntity}
        {Entity}{RelatedEntity}Controller.cs       ← sub-collection: GET + POST
        Single{Entity}{RelatedEntity}Controller.cs ← relationship instance: GET + PUT + PATCH + DELETE
  /MinimalApi
    {System}Endpoints.cs
  /Extensions
    ResultExtensions.cs
```

## Directory and class skills
| `Directory\|file`                                                    | Description                                       |
| -------------------------------------------------------------------- | ------------------------------------------------- |
| /Controllers/{Entity}/{Entity}Controller.cs                          | Collection root — POST create, GET list           |
| /Controllers/{Entity}/Single{Entity}Controller.cs                    | Single entity lifecycle — GET, PUT, PATCH, DELETE |
| /Controllers/{Entity}/Single{Entity}{Property}Controller.cs          | Addressable property — POST set, DELETE unset     |
| /Controllers/{Entity}/{Related}/{Entity}{Related}Controller.cs       | Sub-collection — GET list, POST add               |
| /Controllers/{Entity}/{Related}/Single{Entity}{Related}Controller.cs | Relationship instance — GET, PUT, PATCH, DELETE   |
| /MinimalApi/{System}Endpoints.cs                                     | System, webhook, batch, cross-aggregate endpoints |
| /Extensions/ResultExtensions.cs                                      | ToProblemDetails helper for Result error mapping  |

## NuGet Packages
| Package | Purpose |
| --- | --- |
| `Microsoft.AspNetCore.Mvc` | `ControllerBase`, `[ApiController]`, `ActionResult`, `ProblemDetails` |
| `MediatR` | `ISender` injected into controllers |
| `Ardalis.Result` | `Result<T>`, `ResultStatus` mapped to HTTP responses |

## Allowed Dependencies
- `{Module}.Interfaces` — command records, query records, DTOs
- `Shared` — if needed for shared markers (indirectly via Interfaces)

# Rules

MUST:
- Every controller action dispatches exactly one `ISender.Send()` call
- Controllers inject `ISender` — never `IMediator`
- Controllers reference only `{Module}.Interfaces` types
- All error responses use `ProblemDetails`
- Every `ResultStatus` the handler can return has an explicit `ProducesResponseType`
- Unexpected `ResultStatus` throws `InvalidOperationException`

MUST NOT:
- Controller action contain business logic, validation, domain rules, or persistence
- Controller reference Application, Domain, Infrastructure, or DbContext
- Controller inject `IRepository<T>` or `IUnitOfWork`
- Multiple `ISender.Send()` calls in one action — except Minimal API system orchestration with explicit justification

# Anti-patterns
- Business logic in controller action
- Multiple `_sender.Send()` calls without system-level justification
- Using `IMediator` instead of `ISender`

# Check list
- [ ] Project references only `{Module}.Interfaces`
- [ ] `/Controllers` folder exists with entity subfolders
- [ ] `/Extensions/ResultExtensions.cs` exists
- [ ] Controller actions dispatch exactly one `ISender.Send()`
- [ ] `ISender` injected — never `IMediator`
