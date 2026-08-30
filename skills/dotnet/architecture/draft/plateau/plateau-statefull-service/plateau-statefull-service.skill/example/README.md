# Example: statefull-service

A runnable example of the `statefull-service` plateau.

This example builds on top of the [`service-with-validated-module-interaction`](../../plateau-service-with-validated-module-interaction/plateau-service-with-validated-module-interaction.skill/plateau-service-with-validated-module-interaction.skill.md) example. It keeps the same command-chain, value-object, and validation patterns, and adds real persistence, optimistic concurrency, idempotent external-entity creation, creation/update timestamps, and cross-module reads.

## What the example demonstrates

- `App.Host/Program.cs` builds the host and wires `AddInfrastructure()`, `AddModules()`, and `AddPipeline()`.
- `App.Infrastructure` owns the single `AppDbContext`, generic `Repository<T>`, `UnitOfWork`, and `EntityVersionResolverFactory`.
- `App.Queries` owns a cross-aggregate read (`GetTaskWithAttachmentsQuery`) that uses `AppDbContext` directly with `AsNoTracking()`.
- Pipeline order: `ExceptionHandlingBehavior` → `ValidationBehavior` → `ConcurrencyBehavior` → `GuidResolvingBehavior` → `UnitOfWorkBehavior`.
- `Sample.Domain` contains two classified entities:
  - `TaskItem` — Internal Mutable (`IVersioned`, `ICreationInfoModel`, `IUpdateInfoModel`).
  - `Attachment` — External Immutable (`IHasGuid`, `ICreationInfoModel`).
- `Sample.Application` contains command handlers, validators, specifications, `TaskVersionResolver`, and `CreateAttachmentGuidResolver`.
- `Sample.Interfaces` declares commands, queries, and DTOs.

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

> Note: The example is configured for PostgreSQL via `AddInfrastructure()`. A real database connection string is required to run the host; the provided code compiles without one.
