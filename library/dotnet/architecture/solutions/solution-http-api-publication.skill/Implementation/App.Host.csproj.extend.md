---
description: Wire the HTTP API layer into the composition root — controller discovery, ProblemDetails, exception-handler ordering, per-module Swagger documents
project_name: "App.Host"
name: "App.Host.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/http-api-publication
  - element/app-host-csproj
---

# Goals
- Give App.Host one extension method, `AddApi()`, that wires every module's Controllers, ProblemDetails, and per-module Swagger documents — the same centralized-registration discipline `AddModules()`/`AddPipeline()` already establish
- Turn App.Host from a console host into an ASP.NET Core web host, now that there's an HTTP surface to serve

# Rule changes

## MUST
- Add `WebApplication.CreateBuilder`/`builder.Services.AddApi()`/`app.MapControllers()` to `Program.cs`, replacing the console `Host.CreateApplicationBuilder` this project used before any API existed
- `UseExceptionHandler()` registered before `MapControllers()`
- Never reference `{Module}.Application`/`{Module}.Domain` from `App.Host` on account of this solution — it still only touches `{Module}.Api` and `{Module}.Interfaces`, per `solution-sln-structure`

# Check list
- [ ] `Program.cs` uses `WebApplication.CreateBuilder`
- [ ] `AddApi()` called once, from `Program.cs`
- [ ] `UseExceptionHandler()` precedes `MapControllers()`
