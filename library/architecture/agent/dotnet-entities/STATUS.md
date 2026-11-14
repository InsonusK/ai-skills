# dotnet entities — status

The plan, per `INVARIANTS.md` §5. One line per wave; the decisions log is `DECISIONS.md`.

| Wave | What | State |
|---|---|---|
| 1a | the HTTP proto example keeps its lists in the process (`KeptLists`), no store | done (6d2fa5f3) |
| 1b | `vp-postgres-in-dotnet` over the entities form; the spec's two forms; `app-layers-in-dotnet` | done (8b009c65) + audit (00d2717f) |
| 2 stage 1 | `vp-unit-of-work-in-dotnet` over the new base | done (00d865b1) + review (7960b67e, 15d2e100, 35024df1, 9cc27cd1) |
| 2 stage 2 | `vp-row-version-in-dotnet` over the new base | done |
| CLI-1 | `vp-cli-api-in-dotnet` off the base plateau (spec `vp-cli-api` gains the dotnet stack) | done (5e98544e), audited; output-failure classification fixed |
| CLI-2 | the shared `postgres-application` scenarios name no entry point; Go/Python steps rebind | done (6c7bdadb), audited |
| CLI-3 | `vp-postgres-in-dotnet` re-based on the CLI chain | done; audited; 155 scenarios pass on PostgreSQL |
| CLI-4 | `vp-unit-of-work-in-dotnet` re-based (full copy of the PostgreSQL example) | done; audited; 178 scenarios pass on PostgreSQL |
| CLI-5 | `vp-row-version-in-dotnet` re-based; `CompleteItem` a CLI action | done; audited; 180 scenarios pass on PostgreSQL |

The CLI pivot (owner, 2026-11-12): the database examples of dotnet run via a CLI, not the HTTP API — see `INVARIANTS.md` §7.

Wave 2 stage 2 is delivered as planned below; the decisions it added are in `DECISIONS.md` (2026-11-12): the save under the caller's version by an untracked read and `UpdateAsync`, `INeverChanges` in `Shared.Persistence`, the recording repository of the handler's scenarios, the scoped override of the HTTP contract scenario, and the sample's whole-application scenarios in the example's own feature files.

## Historical delivery before the CLI pivot — wave 2 stage 2

Attaches to `vp-postgres-in-dotnet/examples` (a sibling of `vp-unit-of-work`, as the rows form's #292 did). The mechanics of #292 carry over; the layers change with the entities form: no rows, no service — the handler works over `IRepository<Item>`, and `DbUpdateConcurrencyException` becomes `ConcurrentChange` in `StoreFailuresMiddleware` (already there, `INVARIANTS.md` §4).

| # | Artifact | Class |
|---|---|---|
| 1 | `Item.Version` (`uint`), `ItemConfiguration`: `IsRowVersion()`; migration `AddItemVersion` (no DDL, snapshot knows the version) | copy-and-modify + new |
| 2 | `INeverChanges` mark — new file in `Shared.Persistence` (the persistence contract); the "every row is marked" scenario checks the model | new |
| 3 | `CompleteItemCommand`/`Result`, `CompleteItemHandler`; `ItemView` gains `Id` and `Version` (text); a specification that reads the item untracked | new + copy-and-modify |
| 4 | the save under the caller's version: untracked read, the caller's version set on the entity, `UpdateAsync` — EF Core compares it in the `WHERE`; an unparseable version becomes 0 and matches no row (a concurrent change) | design decision A, see DECISIONS.md |
| 5 | contract: proto 1.2.0 (`CompleteItem`, `Item.id`, `Item.version`), regenerated swagger; gRPC override, HTTP route | copy-and-modify |
| 6 | scenarios: the shared copy `row-version.feature` in `Postgres.Tests` over a `notes` table (steps on Ardalis's API, a changed row is `DbUpdateConcurrencyException`); `CompleteItem.feature` in `Todo.Application.Tests` (over `InMemoryRepository`); id/version in the entry-point features; `persistence.feature` — a completion under a moved version answers 409 ABORTED | new + copy-and-modify |
| 7 | the skill: `vp-row-version-in-dotnet.skill.md`, `attach.allow`, `conformance.copy`, `diagrams/example-flow.mmd`, `examples/README.md`; the spec links the extension | new |
| 8 | verification: `make test` of the example on PostgreSQL, `make conformance-sync`, `test/test.sh`, `library/testing/agent/check.sh` | — |

## Done when

- Every dotnet example in scope passes `make test` on PostgreSQL; Go and Python examples pass unchanged.
- `test/test.sh` and `library/testing/agent/check.sh` pass.

## Verification — 2026-11-14

Every example below passed `make test`. Database scenarios ran on an isolated
PostgreSQL 18 instance; each persistence CLI action launched a separate OS process.

| dotnet example | Passed | Skipped / failed |
|---|---:|---|
| base plateau | 53 | 0 / 0 |
| HTTP proto | 159 | 0 / 0 |
| CLI | 73 | 0 / 0 |
| PostgreSQL | 155 | 0 / 0 |
| unit of work | 178 | 0 / 0 |
| row version | 180 | 0 / 0 |

Go's PostgreSQL and unit of work examples passed `make test`. Python's same
examples passed 240 and 272 scenarios, respectively. Their entry points remain HTTP.

Repository gates passed:

- `bash library/architecture/agent/dotnet-entities/check.sh`: chain, allow lists,
  skill links, fixed templates, conformance copies, CLI resource coverage and diagrams.
- `bash test/test.sh`: link direction, attach checks, 51 diagrams, 240 template
  pairs, 771 conformance copies and `aism sync` (807 skills, two targets).
- `bash library/testing/agent/check.sh`: all checks passed.

Every CLI wave had a separate fresh-eyes audit. Confirmed issues fixed: output
FormatException classification, fake-handler coverage of the lists resource,
its CLI tag, and obsolete documentation paths. Infrastructure namespaces stay
unchanged by the owner's decision of 2026-11-14.
