---
description: Add Controllers, MinimalApi, and Extensions folders to {Module}.Api — the module's HTTP adapter layer, thin over ISender
project_name: "{Module}.Api"
name: "{Module}.Api.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/http-api-publication
  - element/module-api-csproj
---

# Goals
- Give the module a real HTTP surface — entity lifecycle via typed Controllers, everything else via Minimal API
- Keep the API layer thin: map input, dispatch exactly one `ISender.Send()`, map output — nothing else

# Core Principles
- `{Module}.Api` references only `{Module}.Interfaces` and `Microsoft.AspNetCore.Mvc` — never `{Module}.Application`, `{Module}.Domain`, or any infrastructure project
- Entity lifecycle operations (create/read/update/delete a resource, its addressable properties, its sub-collections, its relationships) always use one of the five Controller types — never Minimal API
- System-level, webhook, batch, and cross-aggregate operations always use Minimal API — never a sixth Controller type invented to fit them

# Structure

```
/{Module}.Api
  /Controllers
    {Entity}Controller.cs
    Single{Entity}Controller.cs
    Single{Entity}{Property}Controller.cs
    {Entity}{Related}Controller.cs
    Single{Entity}{Related}Controller.cs
  /MinimalApi
    {System}Endpoints.cs
  /Extensions
    ResultExtensions.cs
  {Module}ApiSwaggerRegistration.cs
```

# Rule changes

## MUST
- Reference only `{Module}.Interfaces` and `Microsoft.AspNetCore.Mvc`
- Every controller/endpoint dispatch exactly one `ISender.Send()`
- Never reference `{Module}.Application`, `{Module}.Domain`, `IRepository<T>`, `IUnitOfWork`, `DbContext`, or any domain entity type
- Never contain business logic — a conditional beyond mapping input/output is a defect

# Check list
- [ ] `{Module}.Api.csproj` references only `{Module}.Interfaces`
- [ ] Every action/endpoint method body is: map input → one `Send()` → map output
