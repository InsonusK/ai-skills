# Example: shared-rules

A runnable example of the `shared-rules` plateau.

This example builds on top of the [`statefull-service`](../../plateau-statefull-service/plateau-statefull-service.skill/plateau-statefull-service.skill.md) example. It keeps real persistence, the command chain, value objects, validation pipeline, and adds a centralized, portable rule mechanism (`Sample.Domain.Rules`) with shared Gherkin sources and Mono.Cecil structural checks.

## What the example demonstrates

- `App.Host/Program.cs` builds the host and wires `AddInfrastructure()`, `AddModules()`, and `AddPipeline()`.
- `App.Infrastructure` owns the single `AppDbContext`, generic `Repository<T>`, `UnitOfWork`, and `EntityVersionResolverFactory`.
- `App.Queries` owns a cross-aggregate read (`GetTaskWithAttachmentsQuery`) that uses `AppDbContext` directly with `AsNoTracking()`.
- Pipeline order: `ExceptionHandlingBehavior` → `ValidationBehavior` → `ConcurrencyBehavior` → `GuidResolvingBehavior` → `UnitOfWorkBehavior`.
- `Sample.Domain.Rules` holds centralized, reusable predicates (`TitleRules`, `ScheduleRules`) that are referenced from:
  - `Sample.Domain` fail-fast adapters (`TaskItem.UpdateTitle`, `TaskItem.UpdateSchedule`).
  - `Sample.Application` collect-all adapters (`TitlePropertyValidator`, `SchedulePropertyValidator`, `CreateTaskValidator`, `UpdateTaskValidator`).
- `Sample.Domain.Rules.Spec` contains the shared Gherkin sources (`TitleRules.feature`, `ScheduleRules.feature`) linked into three test projects:
  - `Sample.Domain.Rules.Tests` proves the rule itself.
  - `Sample.Domain.Tests` (only `@format` scenarios) proves the VO/Entity adapter.
  - `Sample.Application.Tests` (only `@semantic` scenarios) proves the DTO/Command validator adapter.
- `Sample.Domain.Tests/Architecture` contains four Mono.Cecil structural checks:
  - Dead rule detection.
  - `DomainException` scoping.
  - Rejection-code uniqueness and format.
  - Guarded property rule coverage.

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
