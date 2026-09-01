# v3.1 solution catalog — migration plan

Working document. Produced from step 1 of the full cycle (verify no-dependency VPs). Once agreed, execute wave by wave, then run delta-conflict-detection, then plateau-create-by-solutions.

## Root cause of most corrections

v3's `solution-sln-structure` always creates **4 module projects** (`Domain`, `Interfaces`, `Application`, `Api`). The v3.1 Feature Model baseline is **2** (`Interfaces` + `Application`). So in v3.1:
- `{Module}.Domain` is created by **VP1 (DomainLogic)**
- `{Module}.Api` is created by a new shared **`solution-api-project`** (used by VP8 + VP9)
- `solution-sln-structure` is reworked: base set = `Interfaces` + `Application`

## Feature Model / Variability Map corrections from step 1

| # | Change | Status |
|---|---|---|
| 1 | Reads (query integration) are part of **Persistence (VP2)**; a module with an API but no VP2 gets a **write-only** API. Add to feature-model.md. | confirmed (user #5) |
| 2 | `MediatorModuleIntegration` (common) = the **Command-dispatch + Notification-dispatch mechanism** only (ICommand/INotification markers, handler/validator co-location, DI self-registration, App.Host wiring). "Load an entity and call its guarded method from a handler" is **VP1**, not common. | needs confirm |
| 3 | v3's `solution-command-integration` `depends_on solution-domain-behaviour` — **dropped** for v3.1 (that coupling is VP1). Record as a v3.1 ADR. | needs confirm |
| 4 | VP4 (CentralizedRules) is **not** hard-gated on VP1: `{Module}.Domain.Rules` references only `{Module}.Interfaces` + FluentValidation. Entity/strict-VO redirects apply only when VP1/VP3 are present. The Cecil companion runs its applicable subset (dead-rule + code-uniqueness always; exception-scoping + guarded-property-coverage only with VP1). | needs confirm |
| 5 | `solution-command-integration` becomes **`solution-mediator-integration`** — one common solution for "this family uses the MediatR pattern": `ICommand`/`IQuery`/`INotification` markers, handler + validator co-location, module DI self-registration, App.Host wiring, the dispatch mechanism. Folds in the old `solution-command-integration`, the notification half, and the *marker + dispatch* part of `solution-query-integration`. | resolved (user #1) |
| 6 | **Central Package Management** — new common feature `CentralPackageManagement` (added to feature-model.md) + new `solution-central-package-management` in Wave 0: repo-root `Directory.Packages.props`, `<ManagePackageVersionsCentrally>`, versionless `<PackageReference>` everywhere. | resolved (user) |

## The catalog

Legend: **copy** = copy from v3, rebind links only · **copy+mod** = copy then change as noted · **NEW** = author from scratch

### Wave 0 — common baseline

| Solution | Origin | Realizes | Change |
|---|---|---|---|
| `solution-central-package-management` | NEW | CentralPackageManagement | repo-root `Directory.Packages.props`, `<ManagePackageVersionsCentrally>true`, versionless `<PackageReference>` in every csproj; other solutions add `<PackageVersion>` entries here instead of inline versions |
| `solution-sln-structure` | copy+mod | baseline | base set = Interfaces + Application; remove Domain/Api creation and `{Entity}.cs`; ADR for the reduced base set |
| `solution-pipeline-registration` | copy | (infra for pipeline) | rebind links |
| `solution-validation-behavior` | copy | ValidationPipeline | rebind links |
| `solution-mediator-exception-handler` | copy+mod | ExceptionHandlingPipeline | rebind; align with `solution-app-logging` for the "log critical" step |
| `solution-soft-value-objects` | copy+**split** | SoftValueObjects | the `Soft{VO}` (Interfaces-side) half of v3's `solution-value-objects` |
| `solution-dto-property-validators` | copy+mod | CrossModuleValidation, boundary validation | `depends_on solution-soft-value-objects` only (not strict) |
| `solution-mediator-integration` | copy+mod (**rename** from `solution-command-integration`) | MediatorModuleIntegration | one solution for the MediatR pattern: `ICommand`/`IQuery`/`INotification` markers, handler+validator co-location, module DI self-registration, App.Host wiring, dispatch. Drop `depends_on solution-domain-behaviour`. Absorbs the marker+dispatch part of v3's `solution-query-integration` and adds `INotification`. |
| `solution-dotnet-conformance-testing` | copy | TestConformance + 4 children | rebind links |
| `solution-app-logging` | NEW | AppLogging | console structured logging (Microsoft.Extensions.Logging), extensible to file; no read-back |

### Wave 1 — no-dependency VPs

| Solution | Origin | VP | Change |
|---|---|---|---|
| `solution-domain-behaviour` | copy+mod | VP1 | **also creates** `{Module}.Domain.csproj` + first `{Entity}.cs` (moved from sln-structure); guarded state transitions |
| `solution-api-project` | NEW | (VP8+VP9 prereq) | creates `{Module}.Api.csproj`, App.Host wiring skeleton |
| `solution-http-api-publication` | copy+mod | VP8 | `depends_on solution-api-project`; GET actions gated on VP2 (write-only otherwise) |
| `solution-grpc-integration` | copy+mod | VP9 | `depends_on solution-api-project`; read RPCs gated on VP2 |
| `solution-domain-rules` | copy+mod | VP4 | requirements: Application-only consumers OK; entity/VO redirect optional; `depends_on` cleaned up |
| `solution-cecil-architecture-tests` | copy+mod | VP4 companion | applicable-subset of the 4 checks by which of VP1/VP3 are present |

### Wave 2 — dependent VPs

| Solution | Origin | VP | Change |
|---|---|---|---|
| `solution-infrastructure-project` | copy | VP2 | rebind |
| `solution-domain-configuration` | copy+mod | VP2 | needs `{Module}.Domain` (VP1) — `depends_on solution-domain-behaviour` |
| `solution-repository-integration` | copy | VP2 | rebind |
| `solution-unit-of-work` | copy | VP2 | rebind |
| `solution-query-integration` | copy+mod | VP2 | slimmed to the **repository-backed** read side only: `IReadRepository` single-module query handlers, `App.Queries` cross-module read models + `DbContext`; builds on the common `solution-mediator-integration` (markers/dispatch) |
| `solution-value-objects` (strict half) | copy+**split** | VP3 | strict `{VO}` in `{Module}.Domain`; `depends_on solution-domain-behaviour` + `solution-soft-value-objects` |
| `solution-entity-concurrency-change` | copy | VP5 | rebind |
| `solution-external-created-entity` | copy | VP6 | rebind |
| `solution-entity-edit-timestamp` | copy | VP7 | rebind |
| `solution-entity-classification` | copy+mod | VP5×VP6 resolver | reframe as combination-resolver per feature-model |

### Wave 3 — aspirational (all NEW)

| Solution | VP | Notes |
|---|---|---|
| `solution-http-api-client` | VP10 | typed HttpClient, resilience, Result mapping |
| `solution-grpc-client` | VP11 | generated client, Result mapping |
| `solution-messaging-infrastructure` | VP12/13 prereq | Kafka connection/config, like `solution-infrastructure-project` |
| `solution-kafka-consumer` | VP12 | consumer host, dispatch to MediatR |
| `solution-kafka-producer` | VP13 | producer, publish from handler |
| `solution-transactional-outbox` | VP14 | outbox table + relay; `depends_on` messaging + persistence |

## value-objects split — RESOLVED (Option A)

Split v3's `solution-value-objects` into:
- `solution-soft-value-objects` — common, `Soft{VO}` in `{Module}.Interfaces` (Wave 0)
- `solution-value-objects` — VP3, strict `{VO}` in `{Module}.Domain` (Wave 2)

## Decisions — RESOLVED

1. ✅ MediatR — one common `solution-mediator-integration` (rename of `solution-command-integration`), covers commands + queries dispatch + notifications.
2. ✅ value-objects — Option A (split).
3. ✅ Corrections #2/#3/#4 — proceed with the interpretations in the table above (owner delegated).
4. ✅ Central Package Management — common feature + Wave 0 solution.
5. ✅ Stay on `plateau-map-v3`.
6. ✅ Wave order confirmed. Executing Wave 0 first.

## Wave 0 status (in progress)

| Solution | State | Notes |
|---|---|---|
| `solution-central-package-management` | ✅ done | skill + Implementation + ADR authored |
| `solution-app-logging` | ✅ done | skill + 4 Implementation files + ADR authored |
| `solution-sln-structure` | ✅ reworked | base set = Interfaces + Application; Domain/Api Implementation files removed (stashed for Wave 1); Repository.create.md rewritten; ADR rewritten; `depends_on solution-central-package-management` |
| `solution-pipeline-registration` | 🟡 copied | links → v3.1, version bumped. Body needs a read-through. |
| `solution-validation-behavior` | 🟡 copied | links → v3.1, version bumped, `built_on_plateau` cleared. Body read-through pending. |
| `solution-mediator-exception-handler` | 🟡 copied | links → v3.1, version bumped. Needs: align "log critical" with `solution-app-logging`; body read-through. |
| `solution-dto-property-validators` | 🟡 copied | links → v3.1, version bumped, `built_on_plateau` cleared. Needs: `depends_on` → `solution-soft-value-objects` (currently still points at old value-objects); body read-through. |
| `solution-dotnet-conformance-testing` | 🟡 copied | links → v3.1, version bumped. Body read-through pending. |
| `solution-mediator-integration` | 🟡 partial | renamed from `solution-command-integration`; frontmatter reworked (IQuery/INotification in `creates`, dropped `depends_on solution-domain-behaviour`, cleared `built_on_plateau`). **TODO:** body still describes commands only — add Query + Notification sections + Implementation files (`IQuery.cs`, `INotificationEvent.cs`, query handler, event); ADR for dropping the domain-behaviour dependency. |
| `solution-soft-value-objects` | 🟡 partial | split from `solution-value-objects`; frontmatter reduced to the Soft half. **TODO:** delete the strict-`{ValueObject}` + `{Entity}.cs.extend` Implementation files (they move to `solution-value-objects` in Wave 2); drop/relocate the `response-dto-uses-soft-value-objects` ADR; body read-through. |

Remaining Wave-0 cleanup pass, all copied solutions: convert `## MUST NOT` / `# Anti-patterns` in Implementation files to negative bullets per current `skill-design`; verify `depends_on` chains resolve inside v3.1.

## Execution notes

- Each copied solution: `cp -r` the v3 folder into `v3.1/solutions/`, then rewrite every `skills/dotnet/architecture/v3/...` link to `.../v3.1/...`, bump `version`, adjust `built_on_plateau`/`depends_on` to v3.1 targets, apply the change noted above, add a v3.1 ADR when the change is a real decision.
- New solutions: follow [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]].
- After each wave: update `variability-map.md` Realized-by cells to point at `v3.1/solutions/`.
