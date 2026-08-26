# Example: v1

A runnable example of the `v1` plateau — the deployable baseline that unions `statefull-service`, `shared-rules`, and `service-with-api`.

This example builds on top of the [`shared-rules`](../../plateau-shared-rules/plateau-shared-rules.skill/plateau-shared-rules.skill.md) example. It keeps real persistence, the centralized rule mechanism, shared Gherkin sources, Cecil structural checks, and adds a real external surface: HTTP REST controllers and a gRPC service.

## What the example demonstrates

- `App.Host/Program.cs` builds an ASP.NET Core web host and wires `AddInfrastructure()`, `AddModules()`, `AddPipeline()`, `AddApi()`, and `AddGrpcApi()`.
- `App.Infrastructure` owns the single `AppDbContext`, generic `Repository<T>`, `UnitOfWork`, and `EntityVersionResolverFactory`.
- `App.Queries` owns a cross-aggregate read (`GetTaskWithAttachmentsQuery`) that uses `AppDbContext` directly with `AsNoTracking()`, dispatched from `Sample.Api`'s Minimal API endpoint (`GET /api/tasks/{taskId}/full`) — no single entity owns that shape, so it lives outside the Controllers.
- Pipeline order: `ExceptionHandlingBehavior` → `ValidationBehavior` → `ConcurrencyBehavior` → `GuidResolvingBehavior` → `UnitOfWorkBehavior`.
- `Sample.Domain.Rules` holds centralized, reusable predicates (`TitleRules`, `ScheduleRules`) with the same three-layer proof and four Cecil architecture checks as `shared-rules`.
- `Sample.Application` demonstrates both cross-module-resolvable validation patterns: `TaskContactDtoValidator` (a `{Dto}Validator` composing `SoftTitle`/`SoftEmail`/`SoftSchedule`) and `AttachmentTaskExistsCheck` (a `{Feature}Check` that loads the referenced `TaskItem` via `IReadRepository<T>` before `CreateAttachmentHandler` runs — this plateau's own real, repository-backed proof of the pattern).
- `Sample.Api` exposes the module externally, all five REST controller archetypes plus Minimal API:
  - HTTP: `TasksController`/`SingleTaskController` (task lifecycle), `SingleTaskScheduleController` (schedule as an independently addressable property), `TaskAttachmentsController`/`SingleTaskAttachmentController` (attachments as a task-scoped sub-collection), `SampleSystemEndpoints` (the `App.Queries` cross-aggregate read), `ResultExtensions` → `ProblemDetails`, per-module Swagger document.
  - gRPC: `Task.proto` + `TaskGrpcService` with `GetTask`/`CreateTask`, `RpcExceptionExtensions` → `RpcException`/`StatusCode`.
- Both adapters dispatch the exact same commands/queries from `Sample.Interfaces` — never two parallel definitions.

## Build the example

```bash
cd example
make build
```

## Run tests

```bash
make unit-test
make mutation-test
make test-report
make test-and-report
```

> Note: `make unit-test` runs each test project with the tag filter that matches its layer contract.
> The example is configured for PostgreSQL via `AddInfrastructure()`. A real database connection string is required to run the host; the provided code compiles without one.

## Known gap

`Sample.Api` has no dedicated test project, carried forward from `plateau-service-with-api`. `ResultExtensions.ToProblemDetails()` and `RpcExceptionExtensions.ToRpcException()` are real, pure mapping functions with zero coverage anywhere in this plateau — the gap is disclosed, not silent.
