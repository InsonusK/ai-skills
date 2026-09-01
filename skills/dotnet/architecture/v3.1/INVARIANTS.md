# v3.1 catalog invariants

The anchor document for the v3.1 solution-catalog build (per [[skills/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). Every solution in `v3.1/solutions/` must satisfy every invariant here. `check.sh` enforces the mechanical ones; the per-wave audit enforces the rest.

## 1. Baseline (what exists before any optional feature)

```
/ (repo root)
  Directory.Packages.props        ← solution-central-package-management
  Directory.Build.props
  {Solution}.sln
/src
  /Modules/{ModuleName}/
    {ModuleName}.Interfaces        ← solution-sln-structure   (public contracts: commands, queries, notifications, DTOs, Soft VOs)
    {ModuleName}.Application       ← solution-sln-structure   (handlers, validators)
  /App/App.Host                   ← solution-sln-structure
  /Shared                         ← solution-sln-structure
  /BuildingBlocks                 ← solution-sln-structure
```

- **No `{Module}.Domain`** at baseline — created by `solution-domain-behaviour` (VP1).
- **No `{Module}.Api`** at baseline — created by `solution-api-project` (VP8/VP9 prerequisite).
- **No `App.Infrastructure` / `App.Infrastructure.Migrations` / `App.Queries`** — created by the first persistence solution (VP2).
- Every `.csproj` uses **versionless `<PackageReference>`**; versions live in `Directory.Packages.props`.

## 2. Common features → realizing solution (must be 1:1 covered)

| Feature (feature-model.md) | Solution |
| --- | --- |
| CentralPackageManagement | `solution-central-package-management` |
| SoftValueObjects | `solution-soft-value-objects` |
| ValidationPipeline | `solution-validation-behavior` |
| ExceptionHandlingPipeline | `solution-mediator-exception-handler` |
| CrossModuleValidation | `solution-dto-property-validators` |
| MediatorModuleIntegration | `solution-mediator-integration` |
| AppLogging | `solution-app-logging` |
| TestConformance (+ Cucumber / Code Coverage / Mutation / Test Reports) | `solution-dotnet-conformance-testing` |
| (baseline structure) | `solution-sln-structure` |

## 3. Variation Points → realizing solution(s) (must be 1:1 covered)

| VP | Solution(s) | Constraint (from variability-map.md) |
| --- | --- | --- |
| VP1 DomainLogic | `solution-domain-behaviour` (creates `{Module}.Domain`) | — |
| VP2 Persistence | `solution-infrastructure-project`, `solution-domain-configuration`, `solution-repository-integration`, `solution-unit-of-work`, `solution-query-integration` | requires VP1 |
| VP3 ValueObjects | `solution-value-objects` (strict `{VO}`) | requires VP1 |
| VP4 CentralizedRules | `solution-domain-rules` + `solution-cecil-architecture-tests` | — (adoption precondition only) |
| VP5 EntityConcurrencyControl | `solution-entity-concurrency-change` | requires VP2 |
| VP6 ExternalIdentity | `solution-external-created-entity` | requires VP2 |
| VP7 AuditTimestamps | `solution-entity-edit-timestamp` | requires VP2 |
| VP8 SyncInboundApi-HTTP | `solution-api-project` + `solution-http-api-publication` | — |
| VP9 SyncInboundApi-gRPC | `solution-api-project` + `solution-grpc-integration` | — |
| VP10 SyncOutboundApi-HTTP | `solution-http-api-client` *(aspirational — skeleton)* | — |
| VP11 SyncOutboundApi-gRPC | `solution-grpc-client` *(aspirational — skeleton)* | — |
| VP12 AsyncInboundApi | `solution-messaging-infrastructure` + `solution-kafka-consumer` *(aspirational — skeleton)* | — |
| VP13 AsyncOutboundApi | `solution-messaging-infrastructure` + `solution-kafka-producer` *(aspirational — skeleton)* | — |
| VP14 OutboxPattern | `solution-transactional-outbox` *(aspirational — skeleton)* | requires VP13 AND VP2 |

`solution-entity-classification` (VP5×VP6 combination resolver) — kept, reframed per feature-model.md; not itself a VP.

## 4. Link & path conventions

- Every internal link points inside `skills/dotnet/architecture/v3.1/` or `skills/common-workflow/` — **never** `architecture/v3/` or `architecture/draft/`.
- **Carve-out:** `solution-dotnet-conformance-testing` legitimately `depends_on` two skills outside those trees — `skills/common-workflow/test/solution-conformance-testing.skill` (the stack-agnostic parent it implements) and `skills/devops/devops-github-wf-bdd-report-publish.skill` (the CI workflow that consumes its `make` targets). These are the only allowed external `depends_on` in the catalog.
- Wikilink form: `[[skills/dotnet/architecture/v3.1/solutions/solution-x.skill/solution-x.skill.md|solution-x]]`. Frontmatter `depends_on` entries end with `.skill.md` before the `|`.
- Implementation-file links: `[[.../solution-x.skill/Implementation/{File}.{kind}.md#SECTION|label]]`.
- A solution's folder name, its main file name, and its `name:` field are identical: `solution-{name}.skill` / `solution-{name}.skill.md` / `name: solution-{name}`.

## 5. Frontmatter policy

- `version:` — `20260901000000` for every v3.1 solution created in this build (bump only on a later real edit).
- `built_on_plateau:` — **empty** for every solution until v3.1 plateaus exist (they are created after the catalog). State the assumed baseline in `# Boundaries` prose instead.
- `depends_on:` — only real structural dependencies, each resolving to a `solution-*.skill.md` inside `v3.1/solutions/`. A whole-plateau assumption is a `# Boundaries` note, not a `depends_on`.
- `tags:` — per facet-vocabulary: `skill/architecture/solution`, `solution/{name}`, one `stack/dotnet`, ≥1 `concern/*`. Implementation files: `solution/{name}` + `element/{name}`. ADRs: `solution/{name}` + `concern/documentation` + `concern/documentation/adr` + `stack/dotnet`.

## 6. skill-design compliance (current)

- No `## MUST NOT` / `## SHOULD NOT` headings and no `# Anti-patterns` section anywhere — including copied `Implementation/` files. Convert to negative bullets under `## MUST`/`## SHOULD` with nested `Risk:` / `Fix:`.
- Every `## MUST` bullet that states a rule carries `Risk:` + `Fix:`.
- Name any `# Goal`/`# Core Principle`/`# Rule` bullet over ~20 words, or any `# Rule` bullet carrying `Risk:`/`Fix:`, as `**{Name}** - ...`.
- Exactly one `# Goal`, one `# Core Principle` (or `# Core Principles`), one `# Rule` (or `# Rules`), one `# Check list` per skill file.

## 7. Per-classification change checklist

**copy (as-is):** rewrite every `architecture/v3/` link → `architecture/v3.1/`; bump `version`; clear `built_on_plateau` (+ Boundaries note); resolve `depends_on` to v3.1 solutions; skill-design cleanup of Implementation files.

**copy + modify:** all of the above, plus the specific change recorded in `solutions-plan.md` and a v3.1 ADR when the change is a real decision (recorded in `DECISIONS.md`).

**new:** author via `solution-create`; ADR + glossary as needed.

**skeleton (aspirational):** main skill file complete (Goal, Core Principle, Boundaries, Rule, Check list) + at least one Implementation file sketching the shape + an explicit `> Draft contract — no consumer yet` note near the top. Full Implementation deferred.

## 8. Ground truth

The `plateau-v1`-equivalent example built by `plateau-create-by-solutions` must `dotnet build` and `make test` green. Until then the catalog is "plausible", not "verified".
