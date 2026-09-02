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

Still open (tracked): S1 (many transformed Implementation-file `## MUST` bullets lack `Risk:`/`Fix:`), S10 (exception-handler: honour `LogEvents.UnhandledException` in its code sample, add `# Boundaries`, trim the v3 behavior list in `PipelineRegistration.cs.extend.md`), S4 (`defer-feature-check-loading` ADR still in v3-plateau terms), NICE-TO-HAVE grammar debris from the `## MUST NOT`→"Never" script.

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

Still open (tracked): S4 (`external-created-entity` needs a `# Boundaries` section — assumes HTTP API + concurrency without depending on either), S7 (`cecil-architecture-tests` Implementation incomplete — Rules.Tests file missing, always-run checks in a VP1-gated project), S10 (API-registration file-name mismatches between `creates:` and Implementation/body across http/grpc), N2 (some ADRs missing `stack/dotnet` or a `tags:` block), N3-N7, S1-from-Wave0 (Risk/Fix on transformed Implementation MUST bullets). EF-Core-config-in-Domain still wants a dedicated ADR (currently only in domain-behaviour prose + here).

## Wave 3

**DONE** — 6 skeletons authored: `solution-messaging-infrastructure`, `solution-kafka-consumer` (VP12), `solution-kafka-producer` (VP13), `solution-transactional-outbox` (VP14), `solution-http-api-client` (VP10), `solution-grpc-client` (VP11). Each has the full main skill file, one shape-only Implementation file, and a `> Draft contract — no consumer yet` marker. Variability Map VP10–VP14 Realized-by cells updated to `skeleton →`.

## Open forks

- _(none currently — VP4↔VP1 and the two feature-model edge changes were resolved in the design conversation)_
