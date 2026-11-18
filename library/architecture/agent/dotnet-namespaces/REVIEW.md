# .NET infrastructure naming: decision brief

Status: investigation prepared; naming rule and migration scope await owner
agreement. Baseline: `080ba3c5` (develop). No projects, namespaces, templates or
legacy consumers are renamed by this investigation.

## Recommendation

Use capability names for projects and namespace roots, independently of the
`Infrastructure` grouping folder. Keep the current `Postgres`, `Postgres.Testing`,
`Client.Http`, `TodoApi` and `Retry` names in the current catalog (option A).
Keep `.Tests` for the project's test suite and `.Testing` for reusable test
helpers; keep the present folder layout and the old catalog outside this pass.

The inspected sources show a consistent separation between grouping folders
and capability identity, rather than an isolated Postgres exception. Prefixing
everything with `Infrastructure` would describe the layer again without
distinguishing two PostgreSQL implementations or two clients of the same API.
Use a more specific capability name if that ambiguity actually arises. No
build failure or concrete name collision has been established by this review.

The cost of A is that a short name such as `Retry` gives less context at an
import site. If that context is the owner's priority, choose C: qualifying
both the project/assembly and namespace keeps their identities aligned. I do
not recommend B as a universal rule: splitting those identities introduces a
second mapping and explicit namespace configuration for a cosmetic benefit.

This recommendation is a proposal, not a recorded architecture decision.
Merging the investigation alone does not approve A or any rename.

## What exists

[INVENTORY.json](INVENTORY.json) is the source-path-level evidence. Regenerate
it from the repository root:

```sh
python3 library/architecture/agent/dotnet-namespaces/inventory.py > library/architecture/agent/dotnet-namespaces/INVENTORY.json
```

The script reads tracked project XML, namespace declarations, infrastructure
imports, project-reference edges, template identity tokens, proto namespace
inputs and old-catalog guidance consumers. It records declarations rather than
evaluating MSBuild conditions/imports, generated namespaces or external projects.
`project_counts` covers all projects so that identity overrides outside the
infrastructure folder are visible too.

| Area | Project files | Infrastructure files | Scope detail |
|---|---:|---:|---|
| Current `library/architecture/dotnet` | 227 | 43 | 35 example projects, 8 template project files; inherited copies counted separately |
| Old `library/dotnet/architecture` | 35 | 2 | Domain-service and offline-sync examples each contain `App.Infrastructure` |

The 43 current infrastructure project files use 15 distinct project names:

| Capability | Project names | Explicit namespace declarations |
|---|---|---|
| PostgreSQL | `Postgres`, `Postgres.Tests`, `Postgres.Testing` | `Postgres`, `.Repositories`, `.Migrations`, `.UnitOfWork`; `Postgres.Tests.Steps`; `Postgres.Testing` |
| Client core | `Client.Abstractions`, `Client.Pipeline`, `Client.Http`, `Client.Grpc` | Same namespace roots as project names |
| Client tests | `Client.Pipeline.Tests`, `Client.Http.Tests`, `Client.Grpc.Tests` | Corresponding `.Tests.Steps` |
| Sample API adapter | `TodoApi`, `TodoApi.Abstractions`, `TodoApi.Tests` | `TodoApi`, `TodoApi.Mock`, `TodoApi.Abstractions`, `TodoApi.Tests.Steps` |
| Client retry | `Retry`, `Retry.Tests` | `Retry`, `Retry.Tests.Steps` |

No current infrastructure project declares `AssemblyName` or `RootNamespace`,
and none is declared in the scanned current shared `.props`/`.targets` files.
The only four identity declarations in the current catalog are
`AssemblyName=migrate` in `App.Migrate` (three examples and its template).
That executable identity is a separate deployment concern; do not rename it as
a side effect of an infrastructure rule.

Local `dotnet msbuild ... -getProperty:AssemblyName,RootNamespace` evaluations
of the example `Postgres.csproj` and `Retry.csproj` both returned their current
short project name for both properties. This checks two representative
evaluations, not every SDK/property combination or a build.

### Current propagation boundaries

- PostgreSQL: the [owning skill](../../dotnet/vp-postgres-in-dotnet.skill/vp-postgres-in-dotnet.skill.md),
  its templates, and its copies in UoW and row-version. The UoW template adds
  `Postgres.UnitOfWork` inside the existing project rather than a new project.
- Client HTTP and gRPC: their fixed `Client.*` templates and examples; the
  retry example also inherits the HTTP client projects and adds `Retry`.
  `TodoApi` is sample adapter code; renaming a fixed core is different from
  prescribing the identity of every application's API adapter.
- [Shared.Persistence](../../dotnet/vp-postgres-in-dotnet.skill/examples/src/Shared.Persistence/Shared.Persistence.csproj)
  is outside `Infrastructure` and owns contracts used by modules. A new
  infrastructure naming rule must not pull those contracts into the provider's
  namespace or change their dependency direction.
- The gRPC client's proto explicitly uses `Contracts.Todo.V1`. Qualifying
  infrastructure does not imply changing a transport-contract namespace.
- The [layer convention](../../../develop/dotnet/app-layers-in-dotnet.skill/app-layers-in-dotnet.skill.md)
  already describes short project paths beneath `Infrastructure`; a changed
  convention belongs there, with an owning ADR, rather than only in a VP.

The inventory includes all 29 tracked current infrastructure template files,
including snippets without their own project template. Their namespace roots
are literal capability names, not `{Namespace}` placeholders. The path-token
list records only tokens in paths; it deliberately does not mistake C# string
interpolation or `{API}` in a comment for a namespace parameter.

`ProjectReference` paths, solution entries, explicit `using` directives and
namespace declarations are separate migration surfaces, recorded or identified
here. A project rename does not by itself rewrite the explicit namespaces.

### Old catalog: separate consumers

The old domain-service and offline-sync examples use `App.Infrastructure`, with
namespaces such as `App.Infrastructure.Persistence`, `.Repositories`,
`.Concurrency` and `.UnitOfWork`. Their project references and the 105 Markdown
files mentioning `App.Infrastructure` are listed separately in the inventory.
Those are repository guidance consumers, not proof of 105 deployed applications.
External consumers cannot be inventoried from this checkout.

Representative old guidance:

- [Infrastructure project](../../../dotnet/architecture/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md).
- [gRPC client](../../../dotnet/architecture/solutions/solution-grpc-client.skill/solution-grpc-client.skill.md)
  and [HTTP client](../../../dotnet/architecture/solutions/solution-http-api-client.skill/solution-http-api-client.skill.md).
- [Repository integration](../../../dotnet/architecture/solutions/solution-repository-integration.skill/solution-repository-integration.skill.md).

Do not turn a naming pass into migration from the old architecture to the new
one. Their consolidation in `App.Infrastructure` is a different project
boundary, not just a missing prefix in the current catalog.

## Options and concrete outcomes

| Axis | A. Capability identity (recommended) | B. Qualified namespaces only | C. Qualified project and namespace |
|---|---|---|---|
| Project / assembly | Keep `Postgres`, `Client.Http`, `TodoApi`, `Retry` | Keep short project/assembly names | `Infrastructure.Postgres`, `Infrastructure.Client.Http`, `Infrastructure.Client.TodoApi`, `Infrastructure.Client.Retry` |
| C# namespace root | Same capability identity | Use C's qualified roots, retaining short assembly names | Same qualified identity as project |
| Folders | Keep present grouping and project folders | Keep present folders | Keep grouping folders; rename leaf project folders to their project identity |
| Test suite | `Postgres.Tests`, `Client.Http.Tests`, etc. | Short test projects; qualified `.Tests` namespaces | `Infrastructure.Postgres.Tests`, `Infrastructure.Client.Http.Tests`, etc. |
| Test helper | `Postgres.Testing` | Short helper project; qualified `.Testing` namespace | `Infrastructure.Postgres.Testing` |
| Existing current catalog | No rename | Change namespace declarations/imports; configure namespace roots explicitly | Change namespace/imports, project identities/paths, solution and project-reference paths together |
| Old catalog | Excluded | Excluded unless separately approved | Excluded unless separately approved |

For B/C the client abstraction and pipeline follow the same rule as their
implementation; `.Abstractions`, `.Pipeline`, `.Grpc`, `.Tests` and `.Testing`
remain distinct roles. The table specifies namespace roots rather than asking
them to mirror every directory segment such as `Core` or `Middleware`.

| Option | Benefits | Costs |
|---|---|---|
| A | Matches current layer convention and owner-preserved names; project, namespace and capability stay aligned; no migration | Less layer context in short imports; future ambiguous capability identities need a local naming choice |
| B | Qualified C# imports without project/path rename | Project/assembly and namespace disagree; explicit namespace configuration plus all source/import changes; project references still show short identities |
| C | Qualified context in both imports and dependency lists; project and namespace stay aligned | Widest migration: 43 current project files including template copies, 29 infrastructure template files, inherited examples and their consumers; paths, diagrams, manifests and docs must follow |

The file counts identify surfaces, not the number of unique implementations;
not all 29 templates are project files. A concrete patch inventory is still
required if the owner selects B or C. Do not infer a rename of `Shared`,
`BuildingBlocks`, module projects, `Shared.Persistence` or `App.Migrate` from
any option here.

## Implementation after agreement

1. Record the chosen rule and its axes in an ADR under the owning .NET layer
   convention; link the ADR from that skill. Clarify the preserved namespace
   decision in the entities log by reference rather than rewriting its history.
2. For A, document the capability-based convention and its test/helper suffixes;
   verify current examples/templates against it. For B/C, prepare a bounded
   migration plan covering the PostgreSQL/UoW/row-version and HTTP/gRPC/retry
   chains, fixed templates, sample adapters and source consumers together.
3. Any path migration must update `attach.allow`, `conformance.copy`, template
   checks, solution/Makefile paths and diagrams. Build and test every changed
   .NET example, with an isolated PostgreSQL for database scenarios. Preserve
   generated proto namespaces and executable identity unless separately agreed.
4. Run shared catalog checks. Keep the task `Needs decision` until the rule and
   scope are agreed; close it only after the approved documentation/migration
   and its checks are complete.

## Validation and limits

The inventory and local property evaluations are read-only. No runtime code or
template was changed, so no restore, build, database or runtime test suite is
claimed here. `python3 test/check-architecture-backlog.py` and
`bash test/test.sh` pass, including catalog structural checks and sync of 809
skills to two targets. Inventory regeneration matches the snapshot; script
syntax, inventory counts and brief links pass. The approved implementation must
add the owning ADR; there is no selected variant to record as an ADR yet.
