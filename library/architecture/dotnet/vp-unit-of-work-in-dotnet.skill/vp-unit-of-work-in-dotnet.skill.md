---
name: vp-unit-of-work-in-dotnet
description: The unit of work variation point in dotnet — IUnitOfWork in Shared, UnitOfWorkMiddleware listed last in the command middleware, and PostgresUnitOfWork, a SaveChanges interceptor scoped with the command that opens the transaction on the first save, with nested commands and notification handlers joining through ScopedSender and stores left unchanged
whenToUse: when a dotnet application with vp-postgres-in-dotnet has a command that writes in several places, runs nested commands, or handles a notification that writes, or when you review a dotnet unit of work
updated: 20261031
tags:
  - stack/dotnet
  - concern/architecture
  - app-type/service
  - app-type/cli
adr:
  - adr/saves-watched-by-an-interceptor.md
---

# Goal
- **The unit of work variation point in dotnet** - A dotnet application that keeps every property of [vp-unit-of-work](library/architecture/spec/vp-unit-of-work.skill/vp-unit-of-work.skill.md); this skill details its implementation for dotnet and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only what [attach.allow](./attach.allow) names, which the repository's attach check verifies against [vp-postgres-in-dotnet](library/architecture/dotnet/vp-postgres-in-dotnet.skill/vp-postgres-in-dotnet.skill.md)'s example: no store, service, or handler changes.

# Core Principle
- **The scope is the unit** - `ScopedSender` gives a command, its nested commands, and its notification handlers one scope, so one `AppDbContext` and one `PostgresUnitOfWork`.
- **The saves open the transaction** - `PostgresUnitOfWork` watches `SaveChanges`, so a store stays unaware of it and a command that only reads holds nothing. Decision recorded in [saves-watched-by-an-interceptor](./adr/saves-watched-by-an-interceptor.md).

```
src/
├── Shared/UnitOfWork/                       copied: IUnitOfWork, UnitOfWorkErrors
├── Middleware/UnitOfWork/                   copied: UnitOfWorkMiddleware<,>
└── Infrastructure/Postgres/Postgres/UnitOfWork/
    └── PostgresUnitOfWork.cs                copied: the interceptor, AddPostgresUnitOfWork
```

# Rule

## MUST

### Copy the fixed code from templates
Copy [templates/src](./templates/src/) over the application, and let `Postgres.Abstractions` reference `Shared`.
- Violation: the transaction opened in a handler or a service; `IUnitOfWork` given to a handler.
- Risk: a commit inside a nested command ends the transaction of its parent; a service that commits breaks the unit.
- Fix: only `UnitOfWorkMiddleware` calls `IUnitOfWork`.

### Register it and list its middleware last
Call `AddPostgresUnitOfWork()` after `AddPostgres` in `// [services]`, and add `typeof(UnitOfWorkMiddleware<,>)` as the last of the command middleware.

```csharp
// [services]
services.AddPostgres(postgres);
services.AddPostgresUnitOfWork();

// [handlers]
services.AddCommandMiddleware(
    typeof(LoggingMiddleware<,>),
    typeof(UnitOfWorkMiddleware<,>));
```

- Violation: `UnitOfWorkMiddleware` before the logging or an access check.
- Risk: a failed commit is missing from the command's log record; a rejected command opens a unit.
- Fix: last in the list; a commit that fails then reaches the logging as the command's failure.

### Run nested commands through ISender, one at a time
Let a handler run its nested commands by `await sender.Send(...)`, one after another, and let it keep going after a failure only by catching the `ErrorKindException` of the nested command.
- Violation: `Task.WhenAll` over nested commands; a handler that builds `AddItemHandler`.
- Risk: a context serves one call at a time; a handler built by hand joins nothing.
- Fix: EF Core wraps each save in a savepoint, so a duplicate key the handler catches leaves the transaction usable.

### Write only through SaveChanges
Let a store write only through the context's change tracker, so the interceptor sees every write; a bulk `ExecuteUpdate`, `ExecuteDelete`, or raw SQL write needs its own decision recorded in the store.
- Violation: `ExecuteDeleteAsync` in a store of an application with this variation point, with no comment.
- Risk: the statement runs outside the transaction when it comes first, so a rollback does not undo it.
- Fix: load and change through the tracker, or save first so the transaction is open.

# Check list
- [ ] `src/Shared/UnitOfWork`, `src/Middleware/UnitOfWork`, and `Postgres/UnitOfWork` are unchanged copies of the templates.
- [ ] `AddPostgresUnitOfWork()` follows `AddPostgres`; `UnitOfWorkMiddleware` is last in `AddCommandMiddleware`.
- [ ] Only the middleware calls `IUnitOfWork`; nested commands go through `ISender`, one at a time.
- [ ] Every write of a store passes through `SaveChanges`.
- [ ] `make test` passes with `TEST_DATABASE_DSN` set; `bash test/check-vp-attach.sh` in this repository reports no file changed beyond [attach.allow](./attach.allow).

# Example
One import of the example, with its real classes:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the PostgreSQL example with the unit of work attached: `ImportItems` keeps all its titles or none, proven in `src/App/App.Host.Tests/features/UnitOfWork.feature`; it has no entry point, its scenarios send it.
