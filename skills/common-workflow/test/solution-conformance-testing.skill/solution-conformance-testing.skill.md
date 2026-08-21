---
name: solution-conformance-testing
description: Defines one unified approach to writing and running tests across projects — Cucumber scenarios for business and technical/architectural functionality alike, code coverage, mutation testing, and a four-target Makefile contract — so testing quality is controlled the same way regardless of stack
whenToUse: when setting up or reviewing a project's testing strategy, when deciding whether a new test case belongs as a Cucumber scenario or a plain test, or when wiring a project's Makefile test targets
domain: skill
type: architecture
version: 1
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
---

# Goal
- Create one unified approach to writing tests across projects, to increase control over testing quality.

# Capabilities
- One readable report per project describing which test cases — business and technical/architectural alike — are covered.
- Mutation testing on top of coverage, so a weak assertion shows up as a surviving mutant instead of a passing coverage number.
- Four uniform `make` targets any CI can call without knowing the project's stack.
- A stack-independent, normalized `tmp/result/*.json` result per test kind, so CI/badge generation never has to parse a tool's native report format — see [# Report contract](#report-contract).

# Core Principles
- Every test run produces a report describing covered test cases in a readable form.
- Mutation testing verifies testing quality — coverage alone only proves a code path executed, not that its result was checked.
- Cucumber is used to write every test case:
  - A business scenario is written as a matrix of input values and expected results.
  - A technical or architectural concern gets its own `.feature` file describing what is being tested, kept separate from business scenarios.
- Code coverage is always collected — never optional.
- The mutation-testing tool is chosen per stack, not per project: Stryker for C#/.NET and for Angular/TypeScript, Mutmut for Python — see [[./adr/mutation-tool-per-stack.md|ADR]].
- The project root exposes exactly four `make` targets: `unit-test`, `mutation-test`, `test-report`, `test-and-report`.

# Adr
- [[./adr/mutation-tool-per-stack.md|Mutation-testing tool per stack]]
  - Selected variant: Stryker for C#/.NET and Angular/TypeScript, Mutmut for Python

# Report contract
`unit-test` and `mutation-test` each write two kinds of output, so `test-report` — and anything downstream of it, such as CI badge generation — has a stack-independent source to read instead of parsing each tool's native format:

- **Normalized result** — a small JSON file under `tmp/result/`, identical in shape regardless of stack:
  - `tmp/result/unit-test.json` — `{ "total": <int>, "passed": <int>, "failed": <int> }`
  - `tmp/result/coverage-test.json` — `{ "linePct": <number> }` (only written when `WITH_CODE_COVERAGE=true`)
  - `tmp/result/mutation-test.json` — `{ "killed": <int>, "survived": <int>, "timedout": <int>, "noCoverage": <int>, "score": <number> }` — `score` is `killed / (killed+survived+timedout+noCoverage) * 100`, rounded to 1 decimal, `"0.0"` when nothing was mutated.
- **Native report** — the underlying tool's own report, kept as-is (not normalized), under `tmp/report/<kind>/`: `tmp/report/tests/`, `tmp/report/coverage/`, `tmp/report/mutation/`, for a human to open directly.

`make test-report` reads only the normalized `tmp/result/*.json` files to assemble its readable report; it never re-parses a tool's native report format. `mutation-test` still exits with the underlying mutation tool's own exit code after writing its normalized result — normalizing is a side effect, never a reason to swallow a real failure.

## Public site output
`test-report` assembles a stack-independent `public/` directory, the one artifact every CI publishing step (e.g. to GitHub Pages) uploads as-is, without knowing anything about the stack:

- `public/<kind>/` — a copy of `tmp/report/<kind>/` for each kind present (`tests`, `coverage`, `mutation`).
- `public/<label>-badge.json` — one per metric, shields.io's [endpoint badge](https://shields.io/badges/endpoint-badge) schema: `{"schemaVersion":1,"label":"<label>","message":"<value>","color":"<color>"}`. `label` is `tests`, `coverage`, or `mutation score`; `color` follows `>=80 brightgreen / >=60 yellowgreen / else red` for percentage metrics, `brightgreen`/`red` for the pass/fail count.
- `public/index.html` — copied verbatim from `report-template/index.html`, a small static landing page the project owns (linking to `tests/`, `coverage/`, `mutation/`) — `test-report` never generates its content, only copies it.

`report-template/index.html` lives outside `.github/` — this solution owns no `.github/workflows/*` file; publishing `public/` (e.g. to GitHub Pages) is a CI concern layered on top, not part of this contract.

# Requirements
None at this level — stack-specific packages (the Cucumber runner, the coverage collector, the mutation tool) are declared by each stack's own extending solution.

# Template Skill Mutations
REPOSITORY:
- [[./Implementation/Repository.create.md|Repository]] - create - `Makefile` exposing the four unified testing targets, plus `report-template/index.html`

# Rule

## MUST
- [[./Implementation/Repository.create.md#MUST|Repository]]
- Write a business scenario as a matrix of input values and expected results, not a narrative walkthrough.
  - Risk: a narrative-style scenario hides which specific input/output pairs are actually being proven, making it easy to think a case is covered when it is not.
  - Fix: structure the scenario (or its `Examples:` table) as explicit input-value/expected-result rows.
- Keep a technical or architectural concern in its own `.feature` file, separate from business scenarios.
  - Risk: mixing the two makes the report unreadable as "which business functions are covered" — the thing this approach exists to make visible.
  - Fix: one `.feature` file per business capability, a separate one per technical/architectural concern.

## MUST NOT
- Make code coverage optional or skip it on any run of `unit-test`.
  - Risk: a run without coverage gives mutation testing nothing to scope against and leaves "was this even executed" unanswered.
  - Fix: always collect coverage as part of `unit-test`.
- Let `test-report` (or anything downstream of it) parse a tool's native report format directly.
  - Risk: switching the underlying tool later breaks every consumer that learned to parse its specific native format.
  - Fix: read only the normalized `tmp/result/*.json` files defined in [# Report contract](#report-contract).
- Let a `mutation-test` run swallow the underlying mutation tool's own failure exit code while normalizing its result.
  - Risk: a real mutation-testing failure gets hidden, and CI reports success on a run that actually found unkilled mutants.
  - Fix: propagate the underlying tool's exit code after writing `tmp/result/mutation-test.json`.
- Let `report-template/index.html` live under `.github/`.
  - Risk: nesting a project-owned static asset inside `.github/` implies this solution owns a workflow or Pages configuration it does not — the actual publishing step is a separate, layered CI concern.
  - Fix: keep it at `report-template/index.html`, copied by `test-report` — never generated, never placed under `.github/`.

# Check list
- [ ] Every business scenario is structured as input/expected-result pairs.
- [ ] Every technical/architectural `.feature` file is separate from business scenario files.
- [ ] `make unit-test`, `make mutation-test`, `make test-report`, and `make test-and-report` all exist and work as documented in [[./Implementation/Repository.create.md|Repository]].
- [ ] Coverage is collected on every `unit-test` run.
- [ ] `tmp/result/*.json` follows the schema in [# Report contract](#report-contract); `tmp/report/<kind>/` holds each tool's native report.
- [ ] `mutation-test` exits with the underlying tool's own exit code after writing its normalized result.
- [ ] `public/` follows [## Public site output](#public-site-output): per-kind report copies, `*-badge.json` files, and `index.html` copied from `report-template/index.html`.
