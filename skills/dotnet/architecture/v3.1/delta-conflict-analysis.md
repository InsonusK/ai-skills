# v3.1 delta-conflict analysis

Produced per [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]], run across the **whole catalog** (every solution active = the maximal plateau). Intersections were found by grouping every `Implementation/` file on its `element/{element-name}` tag. Per-plateau `registry/` entries are created during `plateau-create-by-solutions`, placed at the shallowest plateau where all intersecting solutions are simultaneously in `created_by`.

## Classifier (fixed — from the skill, not re-derived)

`Constraint × Category × Kind`: `F/T/-` · `N/D/M/-` · `N/C/-`. Only **`TMC`, `FMC`, `FDC`** ever need resolution; every other code is canonical.

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
| `entity-cs` | 8 | domain-behaviour `.create`; domain-configuration, domain-rules, entity-classification, entity-concurrency-change, entity-edit-timestamp, external-created-entity, value-objects `.extend` | `FMN`/`TMN` | Canonical — no two touch the same method/property (VOs change types, domain-rules redirects method bodies, the entity-* solutions each add a distinct property/interface). **N≥3 architectural-signal note — the one real signal in the catalog.** See [entity-cs](#entity-cs). |
| `module-domain-csproj` | 8 | domain-behaviour `.create`; +7 `.extend` | `FMN`/`TMN` | Canonical, **benign** — same as the other `.csproj` groups: 3 solutions add a `/folder` or a `<ProjectReference>`/`<PackageReference>`, the other 4 `.extend` files are near-empty (they touch the *classes*, not the project file). The high N here is a shadow of `entity-cs`, not an independent concern. |
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

**No `TMC`, no `FDC`. One `FMC` (`command-cs`), resolved by convention rather than a resolver solution — no resolver solutions built, so no fixed-point iteration needed.**

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

## entity-cs — the one real N≥3 signal

`{Entity}.cs` is the widest intersection surface in the catalog. Exactly **who touches what**:

| Solution | VP | Touches `{Entity}.cs` how |
| --- | --- | --- |
| `solution-domain-behaviour` | VP1 | **creates** — `int Id`, guarded behavior methods (`UpdateComment(...)`), `private static` invariant helpers |
| `solution-value-objects` | VP3 | **re-types** properties primitive/`Soft{VO}` → strict `{ValueObject}` (`decimal Total` → `Money Total`); the inline check moves into the VO ctor |
| `solution-domain-rules` | VP4 | **rewrites method bodies** to call `(...).Check()`; deletes the local `private static` helper |
| `solution-entity-concurrency-change` | VP5 | **adds** `public uint Version` + `: IVersioned` |
| `solution-external-created-entity` | VP6 | **adds** `public Guid Guid` + a `Create(Guid guid, …)` factory param |
| `solution-entity-edit-timestamp` | VP7 | **adds** `ServerCreatedDateTime`/`UserCreatedDateTime` (+ update pair) + `: ICreationInfoModel` (+ `IUpdateInfoModel`) + explicit interface impls |
| `solution-entity-classification` | VP5×VP6 | **no code** — narrates which of VP5/VP6 apply per Internal/External × Immutable/Mutable state |
| `solution-domain-configuration` | VP2 | near-nothing on the class — a `private` parameterless ctor for EF, if needed |

**Still `FMN`/`TMN` canonical:** no two touch the same member. VP3 re-types, VP4 edits bodies, VP5/6/7 each add their own distinct property + marker interface.

**Why it is a signal:** a fully-decorated External-Mutable user-initiated entity carries `Id` + domain props + `Version` + `Guid` + 4 timestamp props + 3 marker interfaces — infrastructure state with no domain meaning, in the class a reader consults for domain intent. Three of the eight (VP5/VP6/VP7) are the same shape: "add a property + implement a marker + wire explicit members".

**Proposals** (a design fork — `DECISIONS.md` ⚠️):
- **A (recommended, VP5 + server half of VP7):** move pure-infrastructure state to **EF shadow properties**. `Version` → `builder.Property<uint>("Version").IsRowVersion()` in `{Entity}Config.cs` (which already holds `VersionedEntityName`); `ServerCreated/UpdatedDateTime` → shadow properties assigned in `AppDbContext.OnBeforeSaving`. `{Entity}.cs` then stops being touched by VP5 and by the server half of VP7. Cost: those values are read via `EF.Property<T>(...)` / a projection, not a class member.
- **B (rejected):** a `VersionedEntity`/`AuditedEntity` base class — C# single inheritance ⇒ you cannot stack the concerns, so you would need a base per combination = the `solution-entity-classification` combination-resolver smell one level down.
- **C (keep as-is):** accept the accretion — it is canonical, mechanical, matches v3.

With **A**, `{Entity}.cs` is touched by VP1 (create), VP3 (re-type), VP4 (redirect bodies), VP6 (`Guid` — genuinely part of the identity story), and the *user*-supplied timestamp stays on the command — down from 8 to 4, all domain-relevant.

## Other N≥3 groups — benign

`app-host-csproj` (16), `app-infrastructure-csproj` (11), `shared-csproj` (10), `module-application-csproj` (9), **`module-domain-csproj` (8)**, `module-interfaces-csproj` (6), `buildingblocks-csproj` (6) are **project files or contract buckets** — each solution adds a `<ProjectReference>`, a `<PackageReference>`, a `/folder`, or a distinct contract type. Adding one more entry is never a conflict; a high N is the design working. `module-domain-csproj`'s N=8 is a *shadow* of `entity-cs` (the same solutions add a folder to the project as they touch the class) — not an independent concern.

Two borderline non-csproj groups, still canonical:
- **`pipelineregistration-cs` (6)** — a real class + ordered method, but every `.extend` declares its position relative to named anchors *conditionally* (`solution-external-created-entity` already: "after `ConcurrencyBehavior` if present, else after `ValidationBehavior`"). `FMN` + an ordering-only note; the plateau's `PipelineRegistration` structure records the assembled order.
- **`entity-config-cs` (5)** — a real EF config class, but each solution adds an *isolated* mapping call (`.Property(x => x.Version).IsRowVersion()`, `.HasIndex(x => x.Guid).IsUnique()`, timestamp columns) — no shared line. `TMN` canonical. Under proposal **A** this group grows slightly (the shadow-property config lands here instead of on the entity) — which is the point: config is the right home for infrastructure mapping.

## command-cs — see the section above.
