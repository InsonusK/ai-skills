# Example: service-with-api

A runnable example of the `service-with-api` plateau — the first plateau in this hierarchy where a module is genuinely reachable from outside the process.

This example is built on top of the [`service-with-validated-module-interaction`](../../plateau-service-with-validated-module-interaction/plateau-service-with-validated-module-interaction.skill/plateau-service-with-validated-module-interaction.skill.md) example — it keeps the same project shape, module internals (`Sample.Domain`, `Sample.Application`, `Sample.Interfaces`, and their command chain, `CreateTaskCommand`/`GreetCommand` both included, unchanged), composition root, and conformance-testing contract, and adds a real external surface on top: HTTP REST and gRPC, both dispatching the exact same commands.

## What the example demonstrates

- `App.Host` is now an ASP.NET Core web host (`Microsoft.NET.Sdk.Web`, `WebApplication.CreateBuilder`) instead of the parent's plain console host — required by `Microsoft.AspNetCore.Mvc` and `Grpc.AspNetCore`.
- `Sample.Api` publishes `CreateTaskCommand` over both protocols, independently of each other:
  - HTTP: `TasksController` (`POST /api/tasks`), `ResultExtensions.ToProblemDetails()`, `SampleApiSwaggerRegistration` for a per-module Swagger document.
  - gRPC: `Task.proto` + `TaskGrpcService.CreateTask`, `RpcExceptionExtensions.ToRpcException()`.
- `App.Host/DependencyInjection/ApiRegistration.cs` (`AddApi()`/`UseApi()`) and `GrpcRegistration.cs` (`AddGrpcApi()`/`UseGrpcApi()`) are independent extension pairs — removing either's call from `Program.cs` never breaks the other.
- `GreetCommand` (from the parent plateau) is dispatched only via MediatR internally — it has no HTTP/gRPC surface in this example, since publishing it isn't the point; only `CreateTaskCommand` is exposed externally, to keep the two protocol adapters' worked example focused on one operation.

## Known gap

Only `Create` (`POST`/`CreateTask` RPC) is published — no `List`/`Get` REST action or RPC method exists yet, because `solution-query-integration` is not composed at this plateau's own `built_on_plateau`. This is a complete, valid write-only application per this plateau's own "Add a write-only endpoint before persistence exists" usecase, not a missing piece — a read side arrives once persistence and query-integration are composed too (`plateau-v1`). Because there is no `Single{Entity}Controller`/`GetTask` RPC to point at, `TasksController.Create` returns `201 Created` with the resource body directly rather than via `CreatedAtAction`.

`Sample.Api` still has no dedicated test project (carried over from the parent, where `Sample.Api` was a placeholder with nothing to test) — `ResultExtensions`/`RpcExceptionExtensions` are now real, pure, testable mapping functions with zero coverage anywhere in this plateau. Disclosed, not silent — see the sln-level structure skill's own note.

## Run the example

```bash
cd example
dotnet run --project src/App/App.Host
```

Then browse `http://localhost:<port>/swagger` for the REST API, or call `TaskGrpcService.CreateTask` with a gRPC client against the same host.

## Run tests

```bash
make unit-test
make mutation-test
make test-report
make test-and-report
```
