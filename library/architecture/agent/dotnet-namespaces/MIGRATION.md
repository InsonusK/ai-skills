# Service-qualified .NET namespaces

## Contract

- Owner-approved direction: `{ServiceName}.{Layer}.{LogicalName}` for all
  current .NET architecture layers, not only infrastructure.
- Use `TodoExampleService` as the namespace root of the catalog's variants of
  the same Todo example. Real applications supply their own service name.
- Preserve project/assembly names, folders, solution paths, wire contracts and
  runtime behavior. Namespace hierarchy need not repeat project filenames.
- Qualify `App.Host`, `App.Migrate`, `Shared`, `Shared.Persistence`,
  `BuildingBlocks`, `Middleware` and `MaskedLogging` beneath the service root.
- Modules use `{ServiceName}.Modules.{Module}.{Domain|Commands|Application}`.
- PostgreSQL uses `{ServiceName}.Infrastructure.Database.Postgres`, preserving
  its `.Repositories`, `.Migrations`, `.Testing`, `.Tests` and `.UnitOfWork` roles.
- Client core uses `{ServiceName}.Infrastructure.Client.Core.{Http|Grpc|Pipeline|Abstractions}`;
  API adapters use `.Infrastructure.Client.{Api}`; client middleware uses
  `.Infrastructure.Client.Middleware.{Name}`. Use C# `Http`/`Grpc` casing.
- Include explicit imports, fully qualified type references, test bindings and
  locally generated contract namespaces. Do not change protobuf package or
  service names, routes, DTO fields, serialization or error/retry semantics.
- Namespace-dependent delivered templates use one identifier placeholder,
  `{service-namespace}`, filled consistently once per application. Checks fill
  it with `TodoExampleService`; no filled example retains the placeholder.
- Old `library/dotnet` catalogs, Go/Python implementations and Client/Service
  reorganization are outside this migration.

## Waves and artifacts

| Wave | Artifacts | Classification | Gate |
|---|---|---|---|
| 0 | This anchor, decisions/status, mechanical checker, one sample | new / copy-and-modify | Review the contract before bulk |
| 1 | 14 current .NET examples and their shared delivered templates; template checker; attach declarations | copy-and-modify | Namespace/template/attach checks; all example builds and tests |
| 2 | Owning layer convention/ADR, stack guidance, glossary/diagram identifiers, backlog outcome | new / copy-and-modify | Full structural/sync check and fresh-eyes audit |

Unchanged conformance feature text and external package references are
copy-as-is. No new library or runtime mechanism is introduced.

## Decisions

- Owner: service-qualified namespaces replace the short global names; proceed
  with the migration in the existing task branch/worktree.
- Project/assembly identity is preserved because the authorized change is
  namespace qualification, not project renaming. `migrate` stays the job binary.
- The Todo examples share one service root so attaching a VP compares changes
  to one application; the template parameter supports unrelated service names.
- Namespace categories follow the owner's Database and Client/Core examples;
  this does not move directories or choose the separate Client organization task.

## Status

- Preparation: namespace inventory and migration contract written.
- Wave 0: sample `AppVersion.cs` qualified and compiled in isolation (zero
  warnings/errors); `migrate.py --sample-only` passes. A separate conformance
  review confirms that the sample changes only its namespace. Contract review
  completed: owner explicitly approved this contract before wave 1.
- Wave 1: all namespaces, generated roots and templates migrated. Namespace,
  template (242 pairs), attach and reference checks pass. All 14 examples pass
  their builds and tests, including PostgreSQL/UoW/row-version against the
  isolated PostgreSQL fixture. A temporary HTTP-client example also builds and
  passes tests with `HttpClientExampleService` as its root.
- Wave 1 fresh-eyes audit: independent inverse-rename/XML review found no
  changes to behavior, wire contracts, assembly identities or reference paths.
  Its checker-gap finding was fixed: declarations must match their owning
  project's RootNamespace. A deliberately wrong layer was rejected; the
  restored source passes. Assembly-only attributes remain outside namespaces.
- Wave 2: pending.
- Evidence and fresh-eyes audit findings will be recorded here per wave.
- All 14 examples' dependencies restored from the local cache. NuGet's remote
  endpoint is unreachable; restore used `--ignore-failed-sources` and
  `-p:NuGetAudit=false`, without changing package versions or source files.
- An isolated PostgreSQL 18 fixture is available for database example tests.
