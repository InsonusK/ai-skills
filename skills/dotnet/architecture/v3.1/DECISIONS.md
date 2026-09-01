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

## Wave 1

_(pending)_

## Wave 2

_(pending)_

## Wave 3

_(pending)_

## Open forks

- _(none currently — VP4↔VP1 and the two feature-model edge changes were resolved in the design conversation)_
