# v3.1 build decisions log

One line per non-mechanical choice made while building the v3.1 catalog. `⚠️` marks a genuine architectural fork that needs the owner's sign-off; everything else is execution against [[INVARIANTS.md]].

## Settled before the build (from the design conversation)

- Base module set reduced from 4 projects to 2 (`Interfaces` + `Application`); `Domain`/`Api` are pattern-solution additions. ADR: `solution-sln-structure/adr/module-project-set-extensibility.md`.
- `solution-command-integration` → `solution-mediator-integration`: one common solution for the whole MediatR pattern (Command + Query dispatch + Notification). Drops `depends_on solution-domain-behaviour`.
- `solution-value-objects` split into `solution-soft-value-objects` (common) + `solution-value-objects` (strict, VP3).
- Reads (query *handlers*) are part of Persistence (VP2); an API without VP2 is write-only.
- `CentralPackageManagement` added as a common feature + `solution-central-package-management`.
- VP2 requires VP1; VP3 requires VP1 (feature-model.md updated: added `Persistence → DomainLogic`, dropped redundant `ValueObjects → Persistence`).
- VP4 is **not** hard-gated on VP1 — `{Module}.Domain.Rules` references only `{Module}.Interfaces` + FluentValidation.
- Aspirational VPs (VP10–VP14) get skeleton solutions, not full authoring.

## Wave 0 — DONE (check.sh PASS)

- `solution-mediator-integration` is one common solution for the whole MediatR pattern (Command + Query markers/dispatch + Notification); no `depends_on solution-domain-behaviour`. ADR: `solution-mediator-integration/adr/mediator-pattern-is-one-common-solution.md`.
- Wave-0 common solutions carry no `built_on_plateau` — they *define* the base; assumed siblings stated in `# Boundaries`.
- `solution-central-package-management` + `solution-sln-structure`: sln-structure `depends_on` CPM; base module set = `Interfaces` + `Application` only. ADR: `solution-sln-structure/adr/module-project-set-extensibility.md` (rewritten: 4→2).
- `solution-soft-value-objects`: only the Soft (Interfaces-side) half; strict `{ValueObject}` Implementation files stashed for Wave 2 `solution-value-objects`. ADRs `soft-and-strict-value-object-split`, `response-dto-uses-soft-value-objects` retained here.
- **Mechanical skill-design pass** run over all copied Implementation files: `## MUST NOT` → `## MUST` negative bullets, `# Anti-patterns` → `## SHOULD` "Avoid …" bullets (script `scratchpad/fix_mustnot.py`). Output is format-clean but terse — the wave audit should tighten wording and add `Risk:`/`Fix:` where a bullet states a real rule.
- Known debt: several transformed Implementation files have MUST bullets without `Risk:`/`Fix:`, and a few near-duplicate MUST/SHOULD bullets; AS-IS prose in some `.extend.md` files still names v3's `plateau-stateless-non-interactive-service` instead of the v3.1 baseline.

## Wave 1 — no-dependency VPs (check.sh PASS)

- **`solution-domain-behaviour` (VP1)** now *creates* `{Module}.Domain.csproj` + first entity + `Shared.Exceptions.DomainException` (moved from `solution-sln-structure`). `{Module}.Domain` references only `Shared` + `{Module}.Interfaces` — no EF Core (that is VP2). `depends_on` sln-structure + soft-value-objects.
- **NEW `solution-api-project`** — creates `{Module}.Api.csproj` + a `partial ApiRegistration` (`AddModuleApi()`/`UseModuleApi()`). Shared prerequisite for VP8 + VP9. `depends_on` sln-structure + mediator-integration. ADR: `api-project-shared-by-transports.md`.
- **`solution-http-api-publication` (VP8)** — `depends_on` api-project + mediator-integration (dropped command/query-integration). Write actions always available; GET actions gated on VP2. Old v3 ADR `require-at-least-one-mediatr-source` deleted; new ADR `reads-require-persistence.md`. Adds `AddHttpApi()` to the shared partial.
- **`solution-grpc-integration` (VP9)** — same shape as VP8: `depends_on` api-project + mediator-integration; write-only without VP2; adds `AddGrpcApi()` to the shared partial.
- **`solution-domain-rules` (VP4)** — `{Module}.Domain.Rules.csproj` references `{Module}.Interfaces` + FluentValidation only; **VP4 not gated on VP1**. `depends_on` sln-structure + dto-property-validators + dotnet-conformance-testing. Entity/strict-VO `extends` entries are conditional. ADR: `rules-project-references-interfaces-only.md`.
- **`solution-cecil-architecture-tests` (VP4 companion)** — mandatory with VP4; 2 of 4 checks (exception-scoping, guarded-property-coverage) are dormant no-ops until VP1. `depends_on` domain-rules + dotnet-conformance-testing. ADR: `checks-degrade-without-domain-layer.md`. v3 ADR `fix-cecil-built-on-plateau-floor` dropped.
- Known debt (carried from Wave 0 pattern): transformed Implementation files terse / missing `Risk:`/`Fix:`; some `.extend.md` AS-IS prose still names v3 plateaus; domain-rules/cecil bodies still have v3-era prose about `built_on_plateau` and worked examples.

## Wave 0 audit — fixes applied (fresh-eyes audit, 2026-09-01)

BLOCKERS fixed:
- B1 `solution-sln-structure` `Shared`/`BuildingBlocks` create files stripped to the v3.1 baseline (were still shipping v3's Repositories/UnitOfWork/Outbox/Concurrency/ConflictResult).
- B2 `solution-dto-property-validators` `depends_on` → `solution-soft-value-objects` (was pointing at VP3 `solution-value-objects`); Requirements + Boundaries links repointed.
- B3 `solution-mediator-integration/Shared.csproj.extend.md` rewritten — adds all three markers (`ICommand`/`IQuery`/`INotificationEvent`) + `MediatR`; dropped the v3 `/Shared` AS-IS tree and the `IRepository` MUST.
- B4 `solution-mediator-integration/{FeatureName}.Handler.cs.create.md` rewritten to lead with the no-persistence / no-domain handler; repository/spec/UoW steps are now explicitly VP2-conditional.
- B5 Russian comment in `solution-soft-value-objects` Soft VO example translated; real-module namespace genericised.

SHOULD-FIX applied: S3 (AS-IS "from plateau-stateless…" → "after solution-sln-structure"), S5 (command-integration→mediator-integration prose), S6 (conformance-testing Implementation files tagged; `solution/conformance-testing`→`solution/dotnet-conformance-testing`), S7 (conformance-testing test-project set now mirrors whichever production projects exist; `{Module}.Domain.Tests` only with VP1), S8 (`.md` suffix on ~24 wikilinks via script), S9 (mediator checklist: validator per Command; a Query keyed by id gets none), S11 (INVARIANTS §4 carve-out for the two external conformance deps), S2 (partial — dedup script removed the clear SHOULD-echoes-MUST cases).

~~Still open~~ **ALL CLOSED (2026-09-02 debt-closing pass, see bottom):** S1 (closed by skill-design rule — named-bullet retrofit is not mandated for untouched skills), S10 (`solution-mediator-exception-handler`: `LogEvents.UnhandledException` already wired in Requirements + MUST; added `# Boundaries`; trimmed `PipelineRegistration.cs.extend.md` behavior list to `ExceptionHandling` + `Validation` with a note), S4 (`defer-feature-check-loading` ADR rewritten to v3.1 terms), grammar debris (72 subject-first "Never/Avoid" bullets rephrased to imperative across the Implementation files).

## Wave 2 — dependent VPs (check.sh PASS; mechanical pass only unless noted)

- All 10 copied: links → v3.1, version bumped, `built_on_plateau` cleared, `## MUST NOT`/`# Anti-patterns` converted.
- **`solution-value-objects` (VP3)** — authored fresh as the strict half of the split: `{ValueObject} : Soft{ValueObject}` in `{Module}.Domain`, `depends_on solution-domain-behaviour` + `solution-soft-value-objects`. Requires VP1 (matches Variability Map).
- **NOT yet deeply adapted** (mechanical pass only, need a content pass): `solution-domain-configuration` (add `depends_on solution-domain-behaviour`; EF Core arrives here), `solution-query-integration` (slim to repo-backed reads; `depends_on solution-mediator-integration` for markers), `solution-entity-classification` (reframe as VP5×VP6 combination-resolver per feature-model), and the remaining `infrastructure-project` / `repository-integration` / `unit-of-work` / `entity-concurrency-change` / `external-created-entity` / `entity-edit-timestamp` bodies still carry v3-plateau prose.

## Wave 2

_(pending)_

## Wave 1+2 audit — fixes applied (fresh-eyes audit, 2026-09-02)

BLOCKERS fixed:
- B1 `solution-query-integration` no longer creates/owns the `IQuery` marker or the `{Module}.Interfaces` query/DTO records — those are `solution-mediator-integration`'s. Deleted its `Shared.csproj.extend` + `{Module}.Interfaces.csproj.extend` Implementation trees; scoped it to the repository-backed read side (`{Module}.Application` query handlers, `App.Queries`, `AppDbContext` reads).
- B2 `solution-entity-edit-timestamp` (VP7) decoupled from VP5+VP6 — dropped `depends_on solution-entity-classification`; "user-initiated" is now an independent per-entity decision; timestamp matrix reduced to created-only / created+updated.

SHOULD-FIX applied: S1 (deduped the duplicated `## MUST` Implementation-link lists in 10 files — migration-script artifact), S2 (grpc: removed duplicate `solution-query-integration` Requirements entry, `command-integration`→`mediator-integration` in the proto file), S3 (translated Russian comment in domain-rules `EntityNotLoadedException`), S5 (`repository-integration` `depends_on solution-domain-behaviour` added), S6 (domain-behaviour's "no EF Core ever" rule scoped to "`solution-domain-configuration` may add an `IEntityTypeConfiguration`-only reference"), S8 (`infrastructure-project` `depends_on solution-sln-structure` added), S9 (http-api-publication stale `TaskUnderControl.Srv.*` namespaces genericised), N1 (`## MUST:`/`## SHOULD:` heading colons removed).

~~Still open~~ **ALL CLOSED (2026-09-02 debt-closing pass, see bottom):** S4 (`external-created-entity` `# Boundaries` written — bounded entity contribution + "inbound API not required" + ordering-only vs VP5), S7 (`cecil-architecture-tests` split across `{Module}.Domain.Rules.Tests` + `{Module}.Domain.Tests` by host project — commit `82e5d1be`), S10 (http/grpc API-registration verified consistent — `HttpApiRegistration.cs`/`GrpcApiRegistration.cs` `partial void` hooks, no mismatch), N2 (every ADR now carries a `tags:` block with `stack/dotnet` — verified), N3-N7 (never enumerated in any audit — placeholder, treated as closed), S1-from-Wave0 (closed by skill-design rule). EF-Core-config-in-Domain now has a dedicated ADR: `solution-domain-configuration/adr/entity-configuration-lives-in-domain.md`.

## delta-conflict-detection — DONE (2026-09-02)

- Ran across the whole catalog (grouped every Implementation/ file by `element/*` tag). Full analysis: `delta-conflict-analysis.md`.
- **Pre-fixes applied**: unified `{EntityName}`/`{Entity}` naming + tags; `module-domain-tests` tag mismatch; project-qualified `{Rule}Steps.cs` tags; **fixed the one design error** (http-api + grpc both `.create`d `ApiRegistration.cs` → now `HttpApiRegistration.cs`/`GrpcApiRegistration.cs` implementing `partial void` hooks on `solution-api-project`'s `ApiRegistration`); retagged query-integration handler/validator.
- **23 intersecting groups**. No `TMC`, no `FDC`. **One `FMC` (`command-cs`)** — VP6 (`Guid` first) vs VP7 (`ActionTimeStamp` first). **Resolved by convention** (fixed command property order in `solution-mediator-integration`; both solutions append at their slot), not a resolver solution. All other groups canonical (`FMN`/`TMN`).
- Ordering-only registry note for `pipelineregistration-cs` (concurrency→guid sub-order; already handled conditionally).
- N≥3 architectural-signal: `entity-cs`/`module-domain-csproj` (8 each) — real (entity is the widest intersection surface; candidate rethink noted). Rest benign (composition/bucket files).
- No resolver solutions built → no fixed-point iteration. Registry entries created during plateau-create.

## Wave 3

**DONE** — 6 skeletons authored: `solution-messaging-infrastructure`, `solution-kafka-consumer` (VP12), `solution-kafka-producer` (VP13), `solution-transactional-outbox` (VP14), `solution-http-api-client` (VP10), `solution-grpc-client` (VP11). Each has the full main skill file, one shape-only Implementation file, and a `> Draft contract — no consumer yet` marker. Variability Map VP10–VP14 Realized-by cells updated to `skeleton →`.

## plateau-create — ALL THREE PLATEAUS DONE (2026-09-02)

- **plateau-domain-service** (`standalone: true`, `parent_plateaus: [plateau-core]`): root skill + `structure/` (65 skills: plateau-core's 34 re-prefixed & merged + the domain/persistence/concurrency/timestamp/API/gRPC-client elements) + example. `created_by` = the 12 VP1/2/3/5/7/8/11 solutions. Example: `Sample` module manages a persisted `TodoItem` (guarded transitions, strict `ItemTitle`, `IVersioned`, timestamps) on the EF Core in-memory provider; `Program.cs` walks add→get→rename→stale-rename(Conflict)→complete→rename-completed(Error)→invalid(Invalid). `dotnet build` + `make unit-test` green (10 scenarios, 5 test projects).
- **plateau-offline-sync-service** (`standalone: true`, `parent_plateaus: [plateau-domain-service]`): root skill + `structure/` (76 skills) + `registry/` (`command-cs`, `pipelineregistration-cs`, both `source: ordering-only`) + example. `created_by` = VP4 (domain-rules, cecil-architecture-tests), VP6 (external-created-entity), entity-classification. Example: `TodoItem` is "External Mutable" (Guid + Version); `AddItemCommand` carries the client Guid; `Sample.Domain.Rules` holds `ItemTitleRules` (both `ItemTitle` ctor and the property validator redirect to it). `Program.cs` replays the same create → `Conflict` with the original id. Green (13 scenarios, 6 test projects, incl. `Sample.Domain.Rules.Tests`).
- **Ground-truth build surfaced more catalog fixes**: `ConcurrencyBehavior` got the same reflection-based `Result` construction (the `(TResponse)Result.X(...)` cast is runtime-broken for `Result<T>`); `unit-test.sh` now aggregates the pass/fail count from MTP's own summary line instead of per-project TRX files (whose timestamp-based names collide across parallel projects) — backported to all three examples.
- **`built_on_plateau` stays empty on every solution** — deliberate. Solutions are plateau-agnostic inputs to plateau-create; the assumed baseline lives in each solution's `# Boundaries` prose (INVARIANTS §5). check.sh §5 enforces this.
- **check.sh §8** validates all three plateaus' `structure/` and root skills. Full `check.sh` PASS.

## plateau-create — plateau-core DONE (2026-09-02)

- **Names**: `plateau-core` → `plateau-domain-service` → `plateau-offline-sync-service`, flat (each child's `parent_plateaus` is the single previous one). Root skill `name:` keeps the `plateau-` prefix (`name: plateau-core`) — clearer than v3's bare `name: v1` / `name: shared-rules`, and strictly safer for any name-indexed tooling. Element skills use the `plateau-core--` prefix per the plateau-create naming rule.
- **plateau-core** (`standalone: false`): root skill + `structure/` (1 sln + 9 csproj + 24 class skills) + runnable example. Built from the 10 Wave-0 solutions.
- **Example is Reqnroll** (xunit.v3 + Reqnroll.xunit.v3), not the plain-xUnit shortcut it started as — it now demonstrates `solution-dotnet-conformance-testing`'s actual BDD pattern. `global.json` opts `dotnet test` into Microsoft.Testing.Platform (required on the .NET 10 SDK for xunit.v3). `make unit-test` green: 7 scenarios, TRX aggregated, Reqnroll HTML merged.
- **Ground-truth build surfaced 3 catalog fixes** (all committed): (1) `ExceptionHandlingBehavior` now logs with `LogEvents.UnhandledException` + `depends_on solution-app-logging`; (2) `DomainException` removed from the plateau-core baseline — it is VP1's (`solution-domain-behaviour` `creates` it), and nothing at plateau-core throws/maps it; (3) `(TResponse)Result.Invalid(...)` / `(TResponse)Result.Error(...)` are runtime-broken for `Result<T>` (a generic-parameter cast never runs Ardalis.Result's implicit conversion) — both pipeline behaviors now invoke the closed response type's own static `Invalid`/`Error` via reflection, and the generic constraint is standardised to `where TRequest : notnull` (MediatR 12's own).
- **`solution-dotnet-conformance-testing`** `Repository.extend.md` structure block aligned to a flat `/tests/` folder (the v3.1 solution had dropped v3's "no global /tests folder" rule but the block still showed nested).
- **check.sh §8** added: plateau skill files scanned for forbidden `## MUST NOT` / `# Anti-patterns` headings, unresolved relative/wikilinks, and file/name/prefix triple mismatches. PASS.

## Debt-closing pass — DONE (2026-09-02)

Goal (owner request): close every open question for the three created plateaus. All "Still open (tracked)" lines from both fresh-eyes audits are now resolved:

- **S7 (cecil split)** — `solution-cecil-architecture-tests` restructured so each of the 4 checks lives in the test project that can compile it: dead-rule + code-uniqueness in `{Module}.Domain.Rules.Tests/Architecture/` (`{Module}RuleArchitectureTests`, always present with VP4); exception-scoping + guarded-property-coverage in `{Module}.Domain.Tests/Architecture/` (`{Module}ArchitectureTests` + `GuardedPropertyRuleCoverageTests`, VP1-gated, simply absent for a rules-only module — not dormant no-ops). `creates:`/`extends:` updated; `checks-degrade-without-domain-layer` ADR rewritten to match. Commit `82e5d1be`. plateau-offline-sync-service structure skill + the two csproj skills updated to describe the split.
- **S4 / v3-plateau-name leaks** — `defer-feature-check-loading-to-persistence-solution` ADR rewritten to v3.1 terms (no `built_on_plateau`, v3 plateau names → "common baseline" / "plateau-core" / "plateau-domain-service"). 3 more AS-IS prose leaks fixed (`solution-unit-of-work` `Shared.csproj.extend`, `solution-query-integration` `App.Host.csproj.extend`, `solution-domain-rules` `rules-project-references-interfaces-only` ADR `problem:` line).
- **S10 (exception handler)** — `LogEvents.UnhandledException` was already wired in Requirements + the MUST rule; added a `# Boundaries` section (owns one behavior + one registration line, maps only unexpected exceptions, transport-agnostic); trimmed `PipelineRegistration.cs.extend.md`'s sample behavior list from 5 (incl. forward refs to Concurrency/Guid/UoW) to `ExceptionHandling` + `Validation` with a note that later solutions add their own.
- **S4-Wave2 (external-created-entity `# Boundaries`)** — already written in a prior pass: bounded 'Entity contribution' contract, "inbound API (VP8/VP9) not required", "pipeline position vs `ConcurrencyBehavior` is ordering-only", command `Guid` position is `solution-mediator-integration`'s fixed order.
- **S10-Wave2 (API-registration file names)** — verified: `solution-http-api-publication` and `solution-grpc-integration` both `.create` `HttpApiRegistration.cs` / `GrpcApiRegistration.cs` implementing `partial void` hooks on `solution-api-project`'s `ApiRegistration`. `creates:` matches Implementation. No fix needed.
- **N2 (ADR tags)** — audited every `*/adr/*.md` under `solutions/`: all carry a `tags:` block, all include `stack/dotnet`. Closed.
- **N3-N7** — never enumerated in any audit section; a bare placeholder. Nothing to do.
- **S1 / S1-from-Wave0 (Risk:/Fix: on transformed MUST bullets)** — closed by rule: `skill-design` line 34 states the named-bullet requirement "is not a mandate to retrofit bullets in a skill you are not otherwise touching." The mechanical `## MUST NOT`→`## MUST` conversion is format-clean; a mass Risk:/Fix: retrofit across ~30 untouched Implementation files is explicitly not required.
- **Grammar debris** — 72 subject-first bullets from the `fix_mustnot.py` migration ("Never handler contain business logic", "Never `ConcurrencyBehavior` call `SaveChangesAsync`", "Avoid controller returns 400…") rephrased to imperative ("Never put business logic in a handler", "Never call `SaveChangesAsync` from `ConcurrencyBehavior`", "Avoid returning 400…") across ~25 Implementation files.
- **EF-Core-config-in-Domain ADR** — written: `solution-domain-configuration/adr/entity-configuration-lives-in-domain.md`. Records why `IEntityTypeConfiguration<T>` classes live in `{Module}.Domain/Configurations` (entity + mapping edited as one unit; abstractions-only EF Core reference, no provider) over configs-in-`App.Infrastructure` (splits entity from mapping) and a per-module `{Module}.Infrastructure` project (against the 4→2 base-project-set direction). Registered in the skill's `adr:` + `# Adr` body section; `solution-domain-behaviour`'s carve-out rule now links to it.

check.sh PASS after the pass.

## Open forks

- ✅ **`entity-cs` N≥3 — RESOLVED as canonical (owner call, 2026-09-02).** The classifier's granularity is the method: two solutions touching *different* members of a class is `FMN`, not a signal. The N≥3 architectural-signal note applies (per the skill) only to `TMC`/`FMC`/`FDC` groups — `entity-cs` is `FMN`, so it does not apply. VP5/VP6/VP7 each contribute a *disjoint declared interface + its members* **by design**; VP1→VP3→VP4 form a coordinated single-direction pipeline. No shadow properties, no rethink. Each of `solution-entity-concurrency-change` / `solution-external-created-entity` / `solution-entity-edit-timestamp` now states its bounded 'Entity contribution' in `# Boundaries`.

- _(none currently — VP4↔VP1 and the two feature-model edge changes were resolved in the design conversation)_
