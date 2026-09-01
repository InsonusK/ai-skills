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

## Wave 1 — dependent VPs

_(pending — VP2/VP3/VP5-7: infrastructure-project, domain-configuration, repository-integration, unit-of-work, query-integration, value-objects, entity-concurrency-change, external-created-entity, entity-edit-timestamp, entity-classification)_

## Wave 2

_(pending)_

## Wave 3

_(pending)_

## Open forks

- _(none currently — VP4↔VP1 and the two feature-model edge changes were resolved in the design conversation)_
