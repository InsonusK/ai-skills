---
name: solution-conformance-testing
description: Defines one unified approach to writing and running tests across projects — Cucumber scenarios for business and technical/architectural functionality alike, code coverage, mutation testing, and a four-target Makefile contract — so testing quality is controlled the same way regardless of stack
whenToUse: when setting up or reviewing a project's testing strategy, when deciding whether a new test case belongs as a Cucumber scenario or a plain test, or when wiring a project's Makefile test targets
domain: skill
type: architecture
version: 2
updated: 20260924
tags:
  - skill/architecture/solution
  - solution/conformance-testing
  - stack
  - concern/testing
  - concern/testing/bdd
  - concern/testing/mutation
  - cucumber

creates:
  - Makefile
  - report-template/index.html
extends:
depends_on:
built_on_plateau:
adr:
  - "[[skills/common-workflow/test/solution-conformance-testing.skill/adr/mutation-tool-per-stack.md|Mutation-testing tool per stack]]"
  - "[[skills/common-workflow/test/solution-conformance-testing.skill/adr/scenario-report.md|Scenario report from tagged .feature files]]"
---

# Goal
- Create one unified approach to writing tests across projects, to increase control over testing quality.

# Capabilities
- One readable report per project describing which test cases — business and technical/architectural alike — are covered: every scenario with its type, status, and location, plus a type × status summary that makes a missing negative/error case visible — see [## Scenario report](#scenario-report).
- Mutation testing on top of coverage, so a weak assertion shows up as a surviving mutant instead of a passing coverage number.
- Four uniform `make` targets any CI can call without knowing the project's stack.
- A stack-independent, normalized `tmp/result/*.json` result per test kind, so CI/badge generation never has to parse a tool's native report format — see [# Report contract](#report-contract).

# Core Principles
- Every test run produces a report describing covered test cases in a readable form.
- Mutation testing verifies testing quality — coverage alone only proves a code path executed, not that its result was checked.
- Every test case — business and technical/architectural alike — is written as a Cucumber scenario; see [[skills/common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md|cucmber-testing]] for how to author and organize scenarios and step definitions.
- Code coverage is always collected — never optional.
- The mutation-testing tool is chosen per stack, not per project: Stryker for C#/.NET and for Angular/TypeScript, Mutmut for Python — see [[./adr/mutation-tool-per-stack.md|ADR]].
- The project root exposes exactly four `make` targets: `unit-test`, `mutation-test`, `test-report`, `test-and-report`.

# Adr
- [[./adr/mutation-tool-per-stack.md|Mutation-testing tool per stack]]
  - Selected variant: Stryker for C#/.NET and Angular/TypeScript, Mutmut for Python
- [[./adr/scenario-report.md|Scenario report from tagged .feature files]]
  - Selected variant: `unit-test` writes a normalized `tmp/result/scenarios.json` built from the `.feature` files plus the runner's result; `test-report` renders it — no hand-maintained test inventory file

# Report contract
`unit-test` and `mutation-test` each write two kinds of output, so `test-report` — and anything downstream of it, such as CI badge generation — has a stack-independent source to read instead of parsing each tool's native format:

- **Normalized result** — a small JSON file under `tmp/result/`, identical in shape regardless of stack:
  - `tmp/result/unit-test.json` — `{ "total": <int>, "passed": <int>, "failed": <int> }`
  - `tmp/result/coverage-test.json` — `{ "linePct": <number> }` (only written when `WITH_CODE_COVERAGE=true`)
  - `tmp/result/mutation-test.json` — `{ "killed": <int>, "survived": <int>, "timedout": <int>, "noCoverage": <int>, "score": <number> }` — `score` is `killed / (killed+survived+timedout+noCoverage) * 100`, rounded to 1 decimal, `"0.0"` when nothing was mutated.
  - `tmp/result/scenarios.json` — the scenario inventory, see [## Scenario report](#scenario-report).
- **Native report** — the underlying tool's own report, kept as-is (not normalized), under `tmp/report/<kind>/`: `tmp/report/tests/`, `tmp/report/coverage/`, `tmp/report/mutation/`, for a human to open directly.

`make test-report` reads only the normalized `tmp/result/*.json` files to assemble its readable report; it never re-parses a tool's native report format. `mutation-test` still exits with the underlying mutation tool's own exit code after writing its normalized result — normalizing is a side effect, never a reason to swallow a real failure.

## Scenario report
`unit-test` writes `tmp/result/scenarios.json` on every run — also when a test failed:
```json
{ "scenarios": [
  { "feature": "Check a URL", "scenario": "Check a URL", "examples": "malformed",
    "uri": "internal/domain/services/features/check.feature", "line": 18,
    "type": "negative", "status": "passed", "note": "" }
] }
```
- **Entry** — one `Scenario`/`Example`, or one `Examples:` block of a `Scenario Outline`. `examples` is that block's name (`""` for a plain scenario); `line` is the `Scenario` line, or the `Examples:` line for a block; `uri` is the `.feature` path relative to the repository root.
- **Inventory source** — the `.feature` files themselves, so `@todo` entries the runner never executes are listed too. Only the status comes from the runner's own result.
- **`type`** — the one type tag among the entry's own and inherited (`Feature`, `Rule`, `Scenario`, `Examples`) tags, without `@`, per [[skills/common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#one-type-tag-per-scenario|One type tag per scenario]]; `untyped` when there is none or more than one.
- **`status`** — `passed`; `failed` (for an `Examples:` block: any of its rows failed); `todo` (tagged `@todo`, excluded from the run); `missing` (not `@todo`, but the runner reported no result for it — a wiring defect, never a pass).
- **`note`** — the `# todo:` reason of a `@todo` entry, per [[skills/common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#tag-unrunnable-scenarios-todo-and-verify-exclusion|Tag unrunnable scenarios @todo]]; `""` otherwise.

`test-report` renders `public/scenarios/index.html` from `scenarios.json` alone: a type × status count table, then every entry grouped by feature (scenario / examples, type, status, `uri:line`, note). It highlights `untyped` and `missing` entries, and `todo` entries of type `happy`, `negative`, or `error` that have no note.

The report answers, without a separate hand-maintained test inventory file:

| Question | Where it is answered |
| --- | --- |
| Which behaviors are specified, and of which type? | `public/scenarios/` — one row per entry |
| Which kinds of behavior have no scenario at all? | `public/scenarios/` — the type × status table |
| Which scenarios are planned but not implemented, and why? | `public/scenarios/` — `todo` rows with their note |
| What exactly does a scenario assert? | the `Then` step's data table in the `.feature` file at `uri:line` |
| Which scenarios pass but assert too little? | `public/mutation/` — surviving mutants in the code those scenarios exercise |
| Which changed method or branch no scenario reaches? | `public/coverage/` and `public/mutation/` (no-coverage mutants) for the changed files |

## Public site output
`test-report` assembles a stack-independent `public/` directory, the one artifact every CI publishing step (e.g. to GitHub Pages) uploads as-is, without knowing anything about the stack:

- `public/<kind>/` — a copy of `tmp/report/<kind>/` for each kind present (`tests`, `coverage`, `mutation`).
- `public/scenarios/index.html` — rendered from `tmp/result/scenarios.json`, per [## Scenario report](#scenario-report).
- `public/<label>-badge.json` — one per metric, shields.io's [endpoint badge](https://shields.io/badges/endpoint-badge) schema: `{"schemaVersion":1,"label":"<label>","message":"<value>","color":"<color>"}`. `label` is `tests`, `coverage`, or `mutation score`; `color` follows `>=80 brightgreen / >=60 yellowgreen / else red` for percentage metrics, `brightgreen`/`red` for the pass/fail count.
- `public/index.html` — copied verbatim from `report-template/index.html`, a small static landing page the project owns (linking to `scenarios/`, `tests/`, `coverage/`, `mutation/`) — `test-report` never generates its content, only copies it.

`report-template/index.html` lives outside `.github/` — this solution owns no `.github/workflows/*` file; publishing `public/` (e.g. to GitHub Pages) is a CI concern layered on top, not part of this contract.

# Requirements
None at this level — stack-specific packages (the Cucumber runner, the coverage collector, the mutation tool) are declared by each stack's own extending solution.

# Template Skill Mutations
REPOSITORY:
- [[./Implementation/Repository.create.md|Repository]] - create - `Makefile` exposing the four unified testing targets, plus `report-template/index.html`

# Rule

## MUST

### Apply the Repository Implementation
Apply every MUST in [[./Implementation/Repository.create.md#MUST|Repository]].
- Risk: skipping the Implementation file's own rules leaves the Makefile/report-template contract only partially built.
- Fix: follow [[./Implementation/Repository.create.md#MUST|Repository]] in full.

### Delegate Cucumber authoring to cucmber-testing
Follow [[skills/common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md|cucmber-testing]] for how to structure scenarios and step definitions whenever writing a business or technical/architectural test case.
- Violation: writing a scenario as a narrative walkthrough, or mixing a technical/architectural concern into a business `.feature` file, instead of following cucmber-testing's rules.
- Risk: without a single authoring standard, the resulting report is unreadable as "which business functions are covered" — the thing this approach exists to make visible.
- Fix: write every scenario per [[skills/common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md|cucmber-testing]]'s rules.

### Always collect coverage
Collect coverage as part of every `unit-test` run — never make it optional or skip it.
- Risk: a run without coverage gives mutation testing nothing to scope against and leaves "was this even executed" unanswered.
- Fix: wire coverage collection into `unit-test` unconditionally.

### Read only the normalized result files
Have `test-report` (or anything downstream of it) read only the normalized `tmp/result/*.json` files defined in [# Report contract](#report-contract) — never parse a tool's native report format directly.
- Risk: switching the underlying tool later breaks every consumer that learned to parse its specific native format.
- Fix: read `tmp/result/*.json` only; treat `tmp/report/<kind>/` as opaque, human-facing output.

### Write scenarios.json on every run
Have `unit-test` write `tmp/result/scenarios.json` per [## Scenario report](#scenario-report) on every run — including a run where a test failed — with its inventory taken from the `.feature` files, not only from the runner's result.
- Violation: building the list only from what the runner executed, so `@todo` entries vanish; or skipping the write because the runner exited non-zero.
- Risk: the report hides exactly the cases that most need attention — planned-but-missing scenarios and the run that failed.
- Fix: parse every `.feature` file for the inventory, join the runner's per-scenario result onto it, write the file, then exit with the runner's own exit code.

### Propagate the mutation tool's exit code
Propagate the underlying mutation tool's own exit code after writing `tmp/result/mutation-test.json` — never let a `mutation-test` run swallow it while normalizing its result.
- Risk: a real mutation-testing failure gets hidden, and CI reports success on a run that actually found unkilled mutants.
- Fix: exit with the underlying tool's code after the normalized result is written.

### Keep report-template/index.html in place
Keep `report-template/index.html` at that path, copied verbatim by `test-report` — never generated, never placed under `.github/`.
- Risk: nesting a project-owned static asset inside `.github/` implies this solution owns a workflow or Pages configuration it does not — the actual publishing step is a separate, layered CI concern.
- Fix: keep the file at `report-template/index.html` and have `test-report` copy it as-is.

# Check list
- [ ] Every scenario follows [[skills/common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md|cucmber-testing]]'s check list.
- [ ] `make unit-test`, `make mutation-test`, `make test-report`, and `make test-and-report` all exist and work as documented in [[./Implementation/Repository.create.md|Repository]].
- [ ] Coverage is collected on every `unit-test` run.
- [ ] `tmp/result/*.json` follows the schema in [# Report contract](#report-contract); `tmp/report/<kind>/` holds each tool's native report.
- [ ] `tmp/result/scenarios.json` is written on every `unit-test` run, lists `@todo` entries, and follows [## Scenario report](#scenario-report); `public/scenarios/index.html` is rendered from it.
- [ ] `mutation-test` exits with the underlying tool's own exit code after writing its normalized result.
- [ ] `public/` follows [## Public site output](#public-site-output): per-kind report copies, `*-badge.json` files, and `index.html` copied from `report-template/index.html`.
