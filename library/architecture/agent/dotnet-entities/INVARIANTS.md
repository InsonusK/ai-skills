# dotnet entities — invariants

The anchor document for mapping the domain's entities in dotnet with EF Core and reaching them through Ardalis's repository, in place of rows and our repository (per [[library/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). Owner's decisions of 2026-11-05/06 are folded in and logged in `DECISIONS.md`; the plan is in `STATUS.md`.

## 1. Two forms of vp-postgres

| | Rows (Go, Python) | Entities (dotnet) |
|---|---|---|
| What is mapped | a row per table, below the service | the domain's entity; an aggregate's root with its children |
| Who calls the repository | the service, which builds entities from rows | the handler |
| Contract | ours: `Insert`, `Update`, `Delete`, `SelectBy` and a selector | Ardalis's `IRepositoryBase<T>`, order and page in the specification |
| Where the database's failures become kinds | the service, from the repository's outcomes | one command middleware |
| Shared scenarios | `conformance/` and `conformance/postgres-selector/` | `conformance/` |

- Go and Python do not change. The spec `vp-postgres` names both forms; the rules of one contract, of the selector, and of the service over the repository hold for the rows form only.
- `conformance/` keeps what any form must do (repository, outcomes, migrations, readiness, settings); `conformance/postgres-selector/` holds the scenarios of our selector and its specifications (selector, specification).

## 2. Layers in dotnet

| What | Where |
|---|---|
| `IAggregateRoot`, `IRepository<T>`, `IReadRepository<T>` (Ardalis's bases, `T : class, IAggregateRoot`) | `src/Shared/Persistence/`, `Shared` referencing `Ardalis.Specification` (no EF Core) |
| Entity, aggregate root | `{Module}.Domain/Entities`; a root implements `IAggregateRoot` |
| Specification | `{Module}.Domain/Specifications`, `Specification<TEntity>` |
| Mapping | `src/Infrastructure/Postgres/Postgres/Configurations/{Module}/{Entity}Configuration.cs`; `Postgres` references `{Module}.Domain` |
| Repository | `PostgresRepository<T> : RepositoryBase<T>`, one for every root, registered open generic |
| Repository in memory, for the handlers' scenarios | `Postgres.Testing/InMemoryRepository<T>`, the same interfaces |

- A handler takes `IRepository<T>` (or `IReadRepository<T>` when it only reads), loads, calls the entity's methods, and saves. No service stands between a handler and its own database; a service stays for another system's API (`Client`).
- The HTTP variation point adds a way to call the application, not a store: its proto example keeps the lists in the process (`KeptLists`), with no repository and no Ardalis. Attaching the database turns those sample handlers into handlers over `IRepository<ItemList>`; a handler written over the repository changes for no store.
- A repository exists only for a root (`IAggregateRoot`): a child is saved with its root.
- `Postgres.Abstractions`, the rows, and `IItemLists` go away.

## 3. The repository

- A write saves at once (Ardalis calls `SaveChangesAsync`); inside `vp-unit-of-work`'s transaction as before.
- A failed save leaves nothing tracked: `PostgresRepository` clears the change tracker and rethrows.
- Reads track: a handler loads, changes, and saves in one command, so EF Core sees removed children. The context lives one command (`ScopedSender`).
- `CountAsync`, `AnyAsync`, and `DeleteRangeAsync` refuse a specification with a page: a page holds for a list only.
- A statement past `SaveChanges` (`ExecuteUpdate`, raw SQL) is a method of a repository of its own, derived from `PostgresRepository<T>`, run through its `WriteAsync`.

## 4. Failures

One command middleware, `StoreFailuresMiddleware<,>` of `Middleware/StoreFailures`, listed after logging and before the unit of work, asks `IStoreFailures` of `Shared/Persistence`, which `PostgresFailures` of the `Postgres` project implements, and turns a failure of the database into a kind:

| Failure | Kind | Message |
|---|---|---|
| unique violation (`23505`) | `Conflict` | `{entity} already exists` |
| foreign key violation (`23503`) | `NotFound` | `{entity} refers to something that is not there` |
| `DbUpdateConcurrencyException` | `ConcurrentChange` | `{entity} was changed meanwhile` |
| `NpgsqlException { IsTransient: true }` | `Unavailable` | `the database is unavailable` |
| anything else | no kind | — |

- `{entity}` is the type of the failed entry when the failure names one, else `a row`. The failure stays the cause; the log writes it, the caller never sees it.
- A handler that must tell a rule apart says it before the write (`AnyAsync` for a title already in the list, with the rule's own message); the unique index stays the guard under a race, and the middleware answers that one.

## 5. Scope and order

| Wave | What |
|---|---|
| 1a | the HTTP proto example (`vp-http-api-in-dotnet` `examples-proto`): `ItemList` with its rule, the lists in the process (`KeptLists`), no store |
| 1b | `vp-postgres-in-dotnet` (example, templates, skill, ADRs), the spec's two forms and `conformance/postgres-selector/`, `app-layers-in-dotnet` |
| 2 | `vp-unit-of-work-in-dotnet` and `vp-row-version-in-dotnet` over the new base |

- The aggregate with children is the example's list from wave 1a on, so no wave of its own.
- Wave 1 leaves `check-vp-attach` failing for the two examples of wave 2; the PR does not merge before wave 2.

## 6. Done when

- Every dotnet example in scope passes `make test` on PostgreSQL; Go and Python examples pass unchanged.
- `test/test.sh` and `library/testing/agent/check.sh` pass.
