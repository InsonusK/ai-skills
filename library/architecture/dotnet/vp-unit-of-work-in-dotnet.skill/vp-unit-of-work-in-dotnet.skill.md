---
name: vp-unit-of-work-in-dotnet
description: The unit of work variation point in dotnet — IUnitOfWork in Shared, UnitOfWorkMiddleware listed last in the command middleware, and PostgresUnitOfWork, a SaveChanges interceptor scoped with the command that opens the transaction on the first save, with nested commands and notification handlers joining through ScopedSender and repositories left unchanged
whenToUse: when a dotnet application with vp-postgres-in-dotnet has a command that writes in several places, runs nested commands, or handles a notification that writes, or when you review a dotnet unit of work
updated: 20261114
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
- **Attached by the listed changes only** - Attaching it adds files and changes only what [attach.allow](./attach.allow) names, which the repository's attach check verifies against [vp-postgres-in-dotnet](library/architecture/dotnet/vp-postgres-in-dotnet.skill/vp-postgres-in-dotnet.skill.md)'s example: no repository, service, or handler changes.

# Core Principle
- **The scope is the unit** - `ScopedSender` gives a command, its nested commands, and its notification handlers one scope, so one `AppDbContext` and one `PostgresUnitOfWork`.
- **The saves open the transaction** - `PostgresUnitOfWork` watches `SaveChanges`, so a repository stays unaware of it and a command that only reads holds nothing. Decision recorded in [saves-watched-by-an-interceptor](./adr/saves-watched-by-an-interceptor.md).

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
Copy [templates/src](./templates/src/) over the application.
- Violation: the transaction opened in a handler or a service; `IUnitOfWork` given to a handler.
- Risk: a commit inside a nested command ends the transaction of its parent; a service that commits breaks the unit.
- Fix: only `UnitOfWorkMiddleware` calls `IUnitOfWork`.

### Register it and list its middleware last
Call `AddPostgresUnitOfWork()` after `AddPostgres` in `// [services]`, and add `typeof(UnitOfWorkMiddleware<,>)` as the last of the command middleware, after the store failures middleware.

```csharp
// [services]
services.AddPostgres(postgres);
services.AddPostgresUnitOfWork();

// [handlers]
services.AddCommandMiddleware(
    typeof(LoggingMiddleware<,>),
    typeof(StoreFailuresMiddleware<,>),
    typeof(UnitOfWorkMiddleware<,>));
```

- Violation: `UnitOfWorkMiddleware` before the logging, the store failures middleware, or an access check.
- Risk: a failed commit is missing from the command's log record, or reaches the caller untold (a deferred constraint fails at the commit); a rejected command opens a unit.
- Fix: last in the list; a commit that fails then passes the store failures middleware and reaches the logging as the command's failure.

### Run nested commands through ISender, one at a time
Let a handler run its nested commands by `await sender.Send(...)`, one after another, and let it keep going after a failure only by catching the `ErrorKindException` of the nested command.
- Violation: `Task.WhenAll` over nested commands; a handler that builds `AddItemHandler`.
- Risk: a context serves one call at a time; a handler built by hand joins nothing.
- Fix: EF Core wraps each save in a savepoint, so a duplicate key the handler catches leaves the transaction usable; the failed save cleared the tracker, so the handler loads again what it goes on with.

### Write past SaveChanges only through WriteAsync
Run `ExecuteUpdate`, `ExecuteDelete`, raw SQL, and a bulk copy in `PostgresRepository.WriteAsync`, as [vp-postgres-in-dotnet](library/architecture/dotnet/vp-postgres-in-dotnet.skill/vp-postgres-in-dotnet.skill.md) requires of every statement past SaveChanges.
- Violation: `ExecuteDeleteAsync` called directly in a repository.
- Risk: the statement runs outside the transaction when it comes first, so a rollback does not undo it, and the compiler does not stop it.
- Fix: the empty save of `WriteAsync` opens the transaction, and its savepoint keeps a failure from breaking it; Ardalis's `DeleteRangeAsync` saves through SaveChanges and needs none.

### Run the shared scenarios
Name both `conformance/` folders in [conformance.copy](./conformance.copy): the example is a full copy of the PostgreSQL one, so it runs those scenarios too.
- Risk: a change of the repository breaks the unit of work, and only a run of the two together shows it.
- Fix: `make conformance-sync` writes the copies; the steps of `unit-of-work.feature` are in `Postgres.Tests/Steps/UnitOfWorkSteps.cs`, on the sample table of tasks, written with no check before its unique index, so a duplicate key reaches the database inside the transaction.

# Check list
- [ ] `src/Shared/UnitOfWork`, `src/Middleware/UnitOfWork`, and `Postgres/UnitOfWork` are unchanged copies of the templates.
- [ ] `AddPostgresUnitOfWork()` follows `AddPostgres`; `UnitOfWorkMiddleware` is last in `AddCommandMiddleware`.
- [ ] Only the middleware calls `IUnitOfWork`; nested commands go through `ISender`, one at a time.
- [ ] Every write of a repository passes through `SaveChanges` or `WriteAsync`.
- [ ] `make test` passes with `TEST_DATABASE_DSN` set; `bash test/check-vp-attach.sh` in this repository reports no file changed beyond [attach.allow](./attach.allow); `bash test/check-conformance.sh` reports no copy changed.

# Example
One import of the example, with its real classes:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the PostgreSQL example with the unit of work attached: `ImportItems` keeps all its titles or none, proven in `src/App/App.Host.Tests/features/nested-commands.feature`; it has no entry point, its scenarios send it.
