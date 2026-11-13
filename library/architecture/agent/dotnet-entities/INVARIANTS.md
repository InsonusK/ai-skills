# dotnet entities — invariants

The anchor document for mapping the domain's entities in dotnet with EF Core and reaching them through Ardalis's repository, in place of rows and our repository (per [[library/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). Owner's decisions of 2026-11-05/06/07 are folded in and logged in `DECISIONS.md`; the plan is in `STATUS.md`.

## 1. Two forms of vp-postgres

| | Rows (Go, Python) | Entities (dotnet) |
|---|---|---|
| What is mapped | a row per table, below the service | the domain's entities, one per table, configured in their module |
| Who calls the repository | the service, which builds entities from rows | the handler |
| Contract | ours: `Insert`, `Update`, `Delete`, `SelectBy` and a selector | Ardalis's `IRepositoryBase<T>`, order and page in the specification |
| Where the database's failures become kinds | the service, from the repository's outcomes | one command middleware |
| Shared scenarios | `conformance/` and `conformance/postgres-selector/` | `conformance/` |

- Go and Python do not change. The spec `vp-postgres` names both forms; the rules of one contract, of the selector, and of the service over the repository hold for the rows form only.
- `conformance/` keeps what any form must do (repository, outcomes, migrations, readiness, settings); `conformance/postgres-selector/` holds the scenarios of our selector and its specifications (selector, specification).

## 2. Layers in dotnet

| What | Where |
|---|---|
| `IRepository<T>`, `IReadRepository<T>`, `IStoreFailures` (Ardalis's bases, `T : class`) | `src/Shared.Persistence`, a project beside `Shared` referencing `Ardalis.Specification` (no EF Core); `Shared` keeps no package |
| Entity, a row of its table | `{Module}.Domain/Entities` |
| Mapping | `{Module}.Domain/Configurations/{Entity}Configuration.cs`; `{Module}.Domain` references `Microsoft.EntityFrameworkCore.Relational`, no provider; `AppDbContext` applies each module's configurations, a line per module |
| Specification | `{Module}.Application/Specifications`, `Specification<TEntity>` |
| Repository | `PostgresRepository<T> : RepositoryBase<T>`, one for every entity, registered open generic |
| Repository in memory, for the handlers' scenarios | `Postgres.Testing/InMemoryRepository<T>`, the same interfaces |

- A handler takes `IRepository<T>` (or `IReadRepository<T>` when it only reads), loads, calls the entity's methods, and saves. No service stands between a handler and its own database; a service stays for another system's API (`Client`).
- The HTTP variation point adds a way to call the application, not a store: its proto example keeps the lists in the process (`KeptLists`), with no repository and no Ardalis. Attaching the database turns those sample handlers into handlers over `IRepository<Item>`; a handler written over the repository changes for no store.
- A rule over several rows (a title unique in its list) is a unique index; the handler checks it first for the rule's message.
- `Postgres.Abstractions`, the rows, and `IItemLists` go away.

## 3. The repository

- A write saves at once (Ardalis calls `SaveChangesAsync`); inside `vp-unit-of-work`'s transaction as before.
- A failed save leaves nothing tracked: `PostgresRepository` clears the change tracker and rethrows.
- Reads track: a handler loads, changes, and saves in one command, so EF Core writes only what changed. The context lives one command (`ScopedSender`).
- `CountAsync`, `AnyAsync`, and `DeleteRangeAsync` refuse a specification with a page: a page holds for a list only.
- A statement past `SaveChanges` (`ExecuteUpdate`, raw SQL) is a method of a repository of its own, derived from `PostgresRepository<T>`, run through its `WriteAsync`.

## 4. Failures

One command middleware, `StoreFailuresMiddleware<,>` of `Middleware/StoreFailures`, listed after logging and before the unit of work, asks `IStoreFailures` of `Shared.Persistence`, which `PostgresFailures` of the `Postgres` project implements, and turns a failure of the database into a kind:

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
| 1a | the HTTP proto example (`vp-http-api-in-dotnet` `examples-proto`): the lists in the process (`KeptLists`, a title unique in its list), no store |
| 1b | `vp-postgres-in-dotnet` (example, templates, skill, ADRs), the spec's two forms and `conformance/postgres-selector/`, `app-layers-in-dotnet` |
| 2 | `vp-unit-of-work-in-dotnet` and `vp-row-version-in-dotnet` over the new base |

- Wave 1 leaves `check-vp-attach` failing for the two examples of wave 2; the PR does not merge before wave 2.

## 6. Done when

- Every dotnet example in scope passes `make test` on PostgreSQL; Go and Python examples pass unchanged.
- `test/test.sh` and `library/testing/agent/check.sh` pass.

## 7. The CLI pivot (owner, 2026-11-12)

The database examples of dotnet stop demonstrating through the HTTP API: their API changes only ever exercised the sample, and a CLI is simpler to run. The chain becomes `base-plateau → vp-cli-api → vp-postgres → {vp-unit-of-work, vp-row-version}`; Go and Python keep their HTTP chain until a catalog-wide call.

- `vp-cli-api-in-dotnet` is written first (wave CLI-1): `Api/Cli/Pipeline` copied from templates, one file per resource in `Api/Cli/Endpoint`, `Commands.cs` with the resources and the entry factory; it replaces the startup entry, as [vp-cli-api](library/architecture/spec/vp-cli-api.skill/vp-cli-api.skill.md) says.
- The shared scenarios of `vp-postgres` (`postgres-application`) name no entry point after wave CLI-2, so one text runs over HTTP (Go, Python) and over the CLI (dotnet); each stack's steps bind it.
- The DB examples then hold no gRPC/HTTP entry point, no proto, no swagger: their test suites shrink to the CLI, the application over its database, and the conformance copies that stay.
- `vp-unit-of-work-in-dotnet` is re-based with `vp-postgres-in-dotnet`: its example is a full copy of it.
- `vp-row-version-in-dotnet`'s sample is `CompleteItem` as a CLI action; the contract, the entry points, and the scoped contract override of the HTTP wave go away.
