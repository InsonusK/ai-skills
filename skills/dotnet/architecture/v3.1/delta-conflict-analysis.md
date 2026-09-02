# v3.1 delta-conflict analysis

Produced per [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]] (**re-run 2026-09-02** after `solution-grpc-client` VP11 was promoted from skeleton to full — 5 new single-solution elements, joins 3 canonical `.csproj` buckets, **no new conflicts, no new design errors, group count unchanged at 23**), run across the **whole catalog** (every solution active = the maximal plateau). Intersections were found by grouping every `Implementation/` file on its `element/{element-name}` tag. Per-plateau `registry/` entries are created during `plateau-create-by-solutions`, placed at the shallowest plateau where all intersecting solutions are simultaneously in `created_by`.

## Classifier (fixed — from the skill, not re-derived)

`Constraint × Category × Kind`: `F/T/-` · `N/D/M/-` · `N/C/-`. Only **`TMC`, `FMC`, `FDC`** ever need resolution; every other code is canonical.

**The granularity is the method.** The bad signal is *two solutions changing the same method body with no coordination* (`FMC`/`TMC`). Two solutions touching **different** methods/members of the same class, or extending a method in a single direction (create → refine → redirect), is `FMN`/`TMN` — canonical, by design. A solution's footprint on a shared class is a **bounded, declared contract** (e.g. "implement `IVersioned` + its one member"); the analysis checks those footprints are member-disjoint, not that a class has few contributors.

The skill's **N≥3 architectural-signal note applies only to `TMC`/`FMC`/`FDC` groups** (skill rule, line ~97). A canonical `FMN`/`TMN` group with a high N gets no such note — a widely-extended `.csproj` or a cleanly-partitioned class is the design working.

## Pre-analysis fixes applied to the catalog

| Fix | Why |
| --- | --- |
| `element/entityname-cs` → `element/entity-cs`, `{EntityName}` → `{Entity}` everywhere; `element/entityname-config-cs` → `element/entity-config-cs` | v3-copied solutions used `{EntityName}`, new ones `{Entity}` — the same element was split into two groups |
| `element/module-domain-tests` → `element/module-domain-tests-csproj` | conformance-testing's `.create` tag did not match the `.extend` tags |
| `{Rule}Steps.cs` tags project-qualified (`element/{project}-rulesteps`) | conformance-testing creates one per test project; a shared tag would collide in plateau-create |
| `solution-http-api-publication` + `solution-grpc-integration`: `ApiRegistration.cs` `.create` → separate files (`HttpApiRegistration.cs` / `GrpcApiRegistration.cs`) implementing `partial void` hooks on `solution-api-project`'s `ApiRegistration` | **two `.create` on `element/api-registration-cs` = design error**; fixed by the shared-partial pattern (only `solution-api-project` `.create`s the file) |
| `solution-query-integration`'s query handler/validator retagged `element/query-handler-cs` / `element/query-validator-cs` | its `.create` shared a tag with `solution-mediator-integration`'s command handler/validator — different files (`/Queries` vs `/Features`), different kind |

## Intersecting groups (23) and their classification

| Element | N | Solutions | Code | Status |
| --- | --- | --- | --- | --- |
| **`command-cs`** | 4 | mediator-integration `.create`; entity-concurrency-change, entity-edit-timestamp, external-created-entity `.extend` | **`FMC`** (VP6 × VP7, no constraint, both add a field claiming "first") | **Resolved by convention** — see [command-cs](#command-cs). No resolver solution. |
| **`pipelineregistration-cs`** | 6 | pipeline-registration `.create`; validation-behavior, mediator-exception-handler, unit-of-work, entity-concurrency-change, external-created-entity `.extend` | `FMN` + **ordering-only** | Canonical — each `.extend` declares its position relative to named anchors, conditionally (`after ConcurrencyBehavior if present, else after ValidationBehavior`). Plateau `PipelineRegistration` structure owns the assembled order. See [pipelineregistration-cs](#pipelineregistration-cs). |
| `entity-cs` | 8 | domain-behaviour `.create`; domain-configuration, domain-rules, entity-classification, entity-concurrency-change, entity-edit-timestamp, external-created-entity, value-objects `.extend` | `FMN`/`TMN` | **Canonical — no rethink.** Every contribution is member-disjoint or a single-direction pipeline (see [entity-cs](#entity-cs)); the N≥3 note does not apply to a canonical group. |
| `module-domain-csproj` | 8 | domain-behaviour `.create`; +7 `.extend` | `FMN`/`TMN` | Canonical, **benign** — a `.csproj` group: solutions add a `/folder` or a `<ProjectReference>`/`<PackageReference>`. Shadow of `entity-cs`, not an independent concern. |
| `entity-config-cs` | 5 | domain-configuration `.create`; entity-classification, entity-concurrency-change, entity-edit-timestamp, external-created-entity `.extend` | `TMN` | Canonical — each adds a distinct column mapping. N≥3 note. |
| `module-api-csproj` | 5 | api-project `.create`; entity-concurrency-change, external-created-entity, grpc-integration, http-api-publication `.extend` | `FMN`/`TMN` | Canonical + **conditional-applicability**: VP5/VP6 `.extend` this only when the module also has VP8/VP9. Their skills now say so. |
| `single-entity-controller-cs` | 2 | http-api-publication `.create`; entity-concurrency-change `.extend` (ETag/If-Match) | `FMN` | Canonical + conditional — already guarded ("only once an HTTP API layer exists"). |
| `feature-check-cs` | 3 | dto-property-validators `.create` (stub `Load`); domain-rules `.extend` (redirect); repository-integration `.extend` (implement `Load`) | `TMN` | Canonical — the **deferred-stub pattern**: `Load` is intentionally left empty by `dto-property-validators` (its own ADR) and filled by VP2. Different parts of the file. |
| `dto-validator-cs` | 2 | dto-property-validators `.create`; domain-rules `.extend` (redirect `Must()` → shared extension) | `FMN` | Canonical — ordered redirect. |
| `valueobject-cs` | 2 | value-objects `.create`; domain-rules `.extend` (redirect predicate → `Check()`) | `FMN` | Canonical — ordered redirect. |
| `valueobject-propertyvalidator-cs` | 2 | dto-property-validators `.create`; domain-rules `.extend` | `FMN` | Canonical — ordered redirect. |
| `appdbcontext-cs` | 2 | repository-integration `.create`; entity-edit-timestamp `.extend` (`OnBeforeSaving` server timestamps) | `TMN` | Canonical. |
| `module-applicationregistration-cs` | 2 | mediator-integration `.create`; external-created-entity `.extend` (`GuidResolver` DI) | `TMN` | Canonical — adds a registration line. |
| `repositoryregistration-cs` | 2 | repository-integration `.create`; unit-of-work `.extend` (`IUnitOfWork` DI) | `TMN` | Canonical. |
| `featurename-handler-cs` | 2 | mediator-integration `.create`; entity-edit-timestamp `.extend` (user-timestamp assignment) | `TMN` | Canonical. |
| `featurename-validator-cs` | 2 | mediator-integration `.create`; entity-edit-timestamp `.extend` (`ActionTimeStamp` not-future rule) | `TMN` | Canonical. |
| `module-domain-tests-csproj` | 3 | conformance-testing `.create`; cecil-architecture-tests, domain-rules `.extend` (test classes) | `FMN` | Canonical — distinct test classes. |
| `app-host-csproj` | 16 | sln-structure `.create`; +15 `.extend` (`AddX()` / project ref) | `FMN`/`TMN` | Canonical — the composition root. **N≥3 note (benign — touched by every feature by design).** |
| `app-infrastructure-csproj` | 11 | infrastructure-project `.create`; +10 `.extend` | `FMN`/`TMN` | Canonical. N≥3 note (benign — infra bucket). |
| `shared-csproj` | 10 | sln-structure `.create`; +9 `.extend` (contract folders) | `FMN` | Canonical. N≥3 note (benign — contracts bucket). |
| `module-application-csproj` | 9 | sln-structure `.create`; +8 `.extend` | `FMN`/`TMN` | Canonical. N≥3 note (benign — project file). |
| `module-interfaces-csproj` | 6 | sln-structure `.create`; +5 `.extend` | `FMN` | Canonical. N≥3 note (benign). |
| `buildingblocks-csproj` | 6 | sln-structure `.create`; +5 `.extend` (behavior classes) | `FMN` | Canonical. N≥3 note (benign — pattern bucket). |

**No `TMC`, no `FDC`. One `FMC` (`command-cs`), resolved by convention rather than a resolver solution — no resolver solutions built, so no fixed-point iteration needed. The N≥3 architectural-signal note applies only to `command-cs` (the sole `FMC`); every canonical high-N group is the design working.**

## command-cs

**`FMC`**: `solution-external-created-entity` (VP6) requires the command's `Guid` to be the first property; `solution-entity-edit-timestamp` (VP7) requires `ActionTimeStamp` to be first. No Feature-Model constraint between VP6 and VP7 (independent per-entity axes). This is the v3 registry entry `plateau-statefull-service/registry/command-cs.md`, which recommended a not-yet-built resolver.

**Resolution — a fixed convention, not a resolver** (the `FDC → collection` style: adapt the shared slot so contributors compose deterministically). `solution-mediator-integration`'s `{Command}.cs.create.md` now defines the fixed property order: **business fields → `Guid` (VP6) → `ActionTimeStamp` (VP7) → version token (VP5)**, each present only if its solution is applied, none claiming "first". `solution-external-created-entity` and `solution-entity-edit-timestamp` were edited to append at their slot. Collapses to `FMN`.

Registry entry (per plateau where VP6 + VP7 co-occur): `Ordering source: ordering-only` — the sub-order is a convention, not a `depends_on`-carried constraint.

**N≥3 signal:** the command record is extended by 3 per-entity solutions. Worth reconsidering whether per-entity infrastructure fields belong on the command DTO at all, or whether they should be carried by a header/marker interface (`ICommandWithTimestamp` already exists — the same pattern could absorb `Guid` and the version token).

## pipelineregistration-cs

Six solutions edit `AddPipeline()`. Each inserts one `AddBehavior<X>()` at a position relative to named anchors:
`ExceptionHandlingBehavior` (first) → `ValidationBehavior` → `ConcurrencyBehavior` (VP5, if applied) → `GuidResolvingBehavior` (VP6, if applied) → … → `UnitOfWorkBehavior` (VP2, last).

`solution-external-created-entity` already registers `GuidResolvingBehavior` **conditionally** ("after `ConcurrencyBehavior` if that solution is applied, else after `ValidationBehavior`") — so this is `FMN` with an ordering-only concern, not a hard `FMC`. The plateau's `PipelineRegistration` structure skill records the assembled canonical order for that plateau's actual solution set.

Registry entry: `Ordering source: ordering-only` for the `ConcurrencyBehavior → GuidResolvingBehavior` sub-order.

## entity-cs — canonical, member-disjoint

`{Entity}.cs` has the most contributors in the catalog (8). It is still `FMN`/`TMN` canonical because **no two touch the same method body without coordination** — every footprint is a bounded, declared contract:

| Solution | VP | Footprint on `{Entity}.cs` | Overlap |
| --- | --- | --- | --- |
| `solution-domain-behaviour` | VP1 | **creates** — `int Id`, guarded behavior methods, `private static` invariant helpers, `Create(...)` | — |
| `solution-value-objects` | VP3 | **re-types** properties primitive/`Soft{VO}` → strict `{ValueObject}` | mechanical type substitution over VP1's members — single direction |
| `solution-domain-rules` | VP4 | **redirects** the *condition* inside VP1's guarded methods to `(...).Check()`; deletes the local helper | single-direction refactor of VP1's methods; VP4 by definition centralises *already-existing* conditions, VP1 never re-edits them |
| `solution-entity-concurrency-change` | VP5 | **contract:** `: IVersioned` + `uint Version` — one property, one interface. Touches no method. | disjoint |
| `solution-external-created-entity` | VP6 | **contract:** `: IHasGuid` + `Guid Guid` + one `Guid` param on the `Create(...)` factory | disjoint property/interface; extends VP1's factory signature (single direction) |
| `solution-entity-edit-timestamp` | VP7 | **contract:** `: ICreationInfoModel` (+ `: IUpdateInfoModel` if the user edits) + the timestamp properties + a `SetTimestamps(...)` method it owns | disjoint |
| `solution-entity-classification` | VP5×VP6 | **no code** — narrates which of VP5/VP6 apply per Internal/External × Immutable/Mutable state | — |
| `solution-domain-configuration` | VP2 | near-nothing — a `private` parameterless ctor for EF, if needed | disjoint |

Two shapes, both fine per the classifier:
- **VP1 → VP3 → VP4** form a *coordinated single-direction pipeline* on the same methods (create → re-type the data → redirect the condition). Not uncoordinated concurrent edits — no `FMC`.
- **VP5 / VP6 / VP7** each contribute a *disjoint declared interface + its members*, by design. A per-entity infrastructure feature's whole entity footprint is "implement interface X". `solution-entity-classification` is the combination-resolver that says which of them a given entity takes.

**No rethink, no shadow properties, no resolver.** The catalog's own contract — "a per-entity VP's footprint on the entity is a named interface + its members" — is now stated explicitly in each of `solution-entity-concurrency-change`, `solution-external-created-entity`, `solution-entity-edit-timestamp` (see their `# Boundaries`).

## High-N groups — all benign

`app-host-csproj` (16), `app-infrastructure-csproj` (11), `shared-csproj` (10), `module-application-csproj` (9), `module-domain-csproj` (8), `module-interfaces-csproj` (6), `buildingblocks-csproj` (6) are **project files or contract buckets** — each solution adds a `<ProjectReference>`, a `<PackageReference>`, a `/folder`, or a distinct contract type. Adding one more entry is never a conflict. `module-domain-csproj`'s N=8 is a *shadow* of `entity-cs` (the same solutions add a folder as they touch the class).

Two real classes with a high N, both canonical:
- **`pipelineregistration-cs` (6)** — a real class + ordered method. Every `.extend` inserts one distinct `AddBehavior<X>()` and declares its position relative to named anchors *conditionally* (`solution-external-created-entity`: "after `ConcurrencyBehavior` if present, else after `ValidationBehavior`"). No two edit the same statement. `FMN` + an ordering-only registry note; the plateau's `PipelineRegistration` structure records the assembled order for its actual solution set.
- **`entity-config-cs` (5)** — a real EF config class. Each solution adds an *isolated* mapping call (`.Property(x => x.Version).IsRowVersion()`, `.HasIndex(x => x.Guid).IsUnique()`, timestamp columns) on a *different* member — no shared line. `TMN` canonical. This is where per-entity infrastructure *mapping* belongs (the entity class carries the interface + members; the config carries the storage mapping).
