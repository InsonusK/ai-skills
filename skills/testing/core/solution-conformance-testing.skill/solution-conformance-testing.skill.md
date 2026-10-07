---
name: solution-conformance-testing
description: Defines one unified approach to writing and running tests across projects — Cucumber scenarios for business and technical/architectural functionality alike, code coverage, mutation testing, and a Makefile contract of independent test kinds and one report — so testing quality is controlled the same way regardless of stack
whenToUse: when setting up or reviewing a project's testing strategy, when deciding whether a new test case belongs as a Cucumber scenario or a plain test, or when wiring a project's Makefile test targets
domain: skill
type: architecture
version: 2
updated: 20261007
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
  - tools/livingdoc/package.json
  - tools/livingdoc/render.mjs
  - tools/testing/testing.mk
  - tools/testing/testing.sh
extends:
depends_on:
built_on_plateau:
adr:
  - "[[skills/testing/core/solution-conformance-testing.skill/adr/mutation-tool-per-stack.md|Mutation-testing tool per stack]]"
  - "[[skills/testing/core/solution-conformance-testing.skill/adr/scenario-report.md|Scenario report from tagged .feature files]]"
  - "[[skills/testing/core/solution-conformance-testing.skill/adr/livingdoc-renderer-per-protocol.md|Living-doc renderer per Cucumber report protocol]]"
  - "[[skills/testing/core/solution-conformance-testing.skill/adr/caller-contract.md|Caller contract: independent test kinds, purpose of the run, caller-chosen directories]]"
---

# Goal
- Create one unified approach to writing tests across projects, to increase control over testing quality.

# Capabilities
- One readable report per project describing which test cases — business and technical/architectural alike — are covered: every scenario with its type, status, and location, plus a type × status summary that makes a missing negative/error case visible — see [## Scenario report](#scenario-report).
- Mutation testing on top of coverage, so a weak assertion shows up as a surviving mutant instead of a passing coverage number.
- Uniform `make` targets and variables a caller uses without knowing the project's stack, which test kinds exist, or how they run — see [# Caller contract](#caller-contract).
- A stack-independent, normalized `result/*.json` per test kind, so the report builder never has to parse a tool's native report format — see [# Report contract](#report-contract).
- A living-doc HTML view of every executed scenario — filterable by tag and status, identical in every stack — rendered from the runner's standard Cucumber report, see [## Living-doc report](#living-doc-report).

# Core Principles
- Every test run produces a report describing covered test cases in a readable form.
- Mutation testing verifies testing quality — coverage alone only proves a code path executed, not that its result was checked.
- Every test case — business and technical/architectural alike — is written as a Cucumber scenario; see [[skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md|cucumber-testing]] for how to author and organize scenarios and step definitions.
- Code coverage is always collected; it is reported in a `report` run.
- The mutation-testing tool is chosen per stack, not per project: Stryker for C#/.NET and for Angular/TypeScript, Mutmut for Python — see [[./adr/mutation-tool-per-stack.md|ADR]].
- Tests run as independent kinds (`make test-kind-{kind}`) and `make test-report` builds one report; a caller states what the run is for, never how to test.

# Adr
- [[./adr/mutation-tool-per-stack.md|Mutation-testing tool per stack]]
  - Selected variant: Stryker for C#/.NET and Angular/TypeScript, Mutmut for Python
- [[./adr/scenario-report.md|Scenario report from tagged .feature files]]
  - Selected variant: the `unit` kind writes a normalized `result/scenarios.json` built from the `.feature` files plus the runner's result; `test-report` renders it — no hand-maintained test inventory file
- [[./adr/livingdoc-renderer-per-protocol.md|Living-doc renderer per Cucumber report protocol]]
  - Selected variant: one pinned renderer per protocol in a shared `tools/livingdoc/` — `multiple-cucumber-html-reporter` for classic JSON, `@cucumber/html-formatter` for Messages
- [[./adr/caller-contract.md|Caller contract]]
  - Selected variant: `test-kind-{kind}` targets discovered through `test-kinds`, `TEST_RUN_PURPOSE` / `DELTA_BASE` as facts about the run, caller-chosen work and report directories, badges checked against the README

# Caller contract
Whoever runs the tests — a developer, an agent, a CI workflow — uses only the targets and variables in [[./Implementation/Repository.create.md#targets-a-caller-uses|Repository]], shipped identically to every stack by [[./Implementation/tools/testing/testing.mk.create.md|tools/testing/testing.mk]]: `make test-kinds` to learn the kinds, `make test-kind-{kind}` to run one, `make test-report` to build the report, `make test-readme-check` for the README badges, and `TEST_RUN_PURPOSE`, `DELTA_BASE`, `TEST_WORK_DIR`, `TEST_REPORT_DIR`. A caller never names a test tool, a coverage switch, or a report file other than `index.html`, `reports/{name}/`, and `badges/{name}.json`. Decision in [[./adr/caller-contract.md|adr/caller-contract]].

# Report contract
Every kind writes two kinds of output below its own `$TEST_KIND_DIR`, per [[./Implementation/Repository.create.md#kind-output|Kind output]], so the report builder has a stack-independent source instead of each tool's native format:

- **Normalized result** — small JSON files under `result/`, identical in shape regardless of stack: `unit-test.json`, `coverage-test.json`, `scenarios.json`, `mutation-test.json`.
- **Native report** — the underlying tool's own report, kept as-is under `report/{name}/` (`tests`, `coverage`, `mutation`), for a human to open directly.

`make test-report` computes badges and the scenario page from the normalized results only and copies the native reports unchanged. A kind exits with its tool's own exit code after writing its results — normalizing is a side effect, never a reason to swallow a real failure.

## Scenario report
`test-kind-unit` writes `result/scenarios.json` on every run — also when a test failed:
```json
{ "scenarios": [
  { "feature": "Check a URL", "scenario": "Check a URL", "examples": "malformed",
    "uri": "internal/domain/services/features/check.feature", "line": 18,
    "type": "negative", "status": "passed", "note": "" }
] }
```
- **Entry** — one `Scenario`/`Example`, or one `Examples:` block of a `Scenario Outline`. `examples` is that block's name (`""` for a plain scenario); `line` is the `Scenario` line, or the `Examples:` line for a block; `uri` is the `.feature` path relative to the repository root.
- **Inventory source** — the `.feature` files themselves, so `@todo` entries the runner never executes are listed too. Only the status comes from the runner's own result.
- **`type`** — the one type tag among the entry's own and inherited (`Feature`, `Rule`, `Scenario`, `Examples`) tags, without `@`, per [[skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#one-type-tag-per-scenario|One type tag per scenario]]; `untyped` when there is none or more than one.
- **`status`** — `passed`; `failed` (for an `Examples:` block: any of its rows failed); `todo` (tagged `@todo`, excluded from the run); `missing` (not `@todo`, but the runner reported no result for it — a wiring defect, never a pass).
- **`note`** — the `# todo:` reason of a `@todo` entry, per [[skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#tag-unrunnable-scenarios-todo-and-verify-exclusion|Tag unrunnable scenarios @todo]]; `""` otherwise.

`test-report` renders `reports/scenarios/index.html` from `scenarios.json` alone: a type × status count table, then every entry grouped by feature (scenario / examples, type, status, `uri:line`, note). It highlights `untyped` and `missing` entries, and `todo` entries of type `happy`, `negative`, or `error` that have no note.

The report answers, without a separate hand-maintained test inventory file:

| Question | Where it is answered |
| --- | --- |
| Which behaviors are specified, and of which type? | `reports/scenarios/` — one row per entry |
| Which kinds of behavior have no scenario at all? | `reports/scenarios/` — the type × status table |
| Which scenarios are planned but not implemented, and why? | `reports/scenarios/` — `todo` rows with their note |
| What exactly does a scenario assert? | the `Then` step's data table in the `.feature` file at `uri:line` |
| Which scenarios pass but assert too little? | `reports/mutation/` — surviving mutants in the code those scenarios exercise |
| Which changed method or branch no scenario reaches? | `reports/coverage/` and `reports/mutation/` (no-coverage mutants) for the changed files |

## Living-doc report
`test-kind-unit` makes the Cucumber runner write its standard report into `report/tests/cucumber/`, in the one protocol its `cucumber-testing-in-{stack}` skill names:
- **classic Cucumber JSON** (`*.json`, `features[].elements[].steps[]`) — rendered by `multiple-cucumber-html-reporter`;
- **Cucumber Messages** (`*.ndjson` envelope stream) — rendered by `@cucumber/html-formatter`.

Then it renders `report/tests/livingdoc/` with the stack-independent [[./Implementation/tools/livingdoc/render.mjs.create.md|tools/livingdoc/render.mjs]], from the isolated, pinned install in [[./Implementation/tools/livingdoc/package.json.create.md|tools/livingdoc/package.json]] — never from the project's own dependency manifest. `test-report` publishes it unchanged as `reports/tests/livingdoc/`, next to the `reports/scenarios/` inventory, which it does not replace. The step needs Node 22+ and is skipped, never failed, where `npm` is missing.

## Report output
`test-report` builds `$TEST_REPORT_DIR` — `index.html`, `reports/{name}/`, `badges/{name}.json`, `run.json` — per [[./Implementation/Repository.create.md#report-output|Report output]]: the one stack-independent artifact a publishing step uploads as-is. Where it is published, and under which path, is the publisher's choice; this solution owns no `.github/workflows/*` file and writes nothing outside the two directories the caller names.

# Requirements
None at this level — stack-specific packages (the Cucumber runner, the coverage collector, the mutation tool) are declared by each stack's own extending solution.

# Template Skill Mutations
REPOSITORY:
- [[./Implementation/Repository.create.md|Repository]] - create - `Makefile` declaring the test kinds, `README.md` badges, `report-template/index.html`
- [[./Implementation/tools/testing/testing.mk.create.md|tools/testing/testing.mk]] - create - caller-facing targets and variables, identical in every stack
- [[./Implementation/tools/testing/testing.sh.create.md|tools/testing/testing.sh]] - create - README and report checks
- [[./Implementation/tools/livingdoc/package.json.create.md|tools/livingdoc/package.json]] - create - pinned living-doc renderers, isolated npm install
- [[./Implementation/tools/livingdoc/render.mjs.create.md|tools/livingdoc/render.mjs]] - create - renders `report/tests/cucumber/` by protocol

# Rule

## MUST

### Apply the Repository Implementation
Apply every MUST in [[./Implementation/Repository.create.md#MUST|Repository]].
- Risk: skipping the Implementation file's own rules leaves the caller contract only partially built.
- Fix: follow [[./Implementation/Repository.create.md#MUST|Repository]] in full.

### Delegate Cucumber authoring to cucumber-testing
Follow [[skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md|cucumber-testing]] for how to structure scenarios and step definitions whenever writing a business or technical/architectural test case.
- Violation: writing a scenario as a narrative walkthrough, or mixing a technical/architectural concern into a business `.feature` file, instead of following cucumber-testing's rules.
- Risk: without a single authoring standard, the resulting report is unreadable as "which business functions are covered" — the thing this approach exists to make visible.
- Fix: write every scenario per [[skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md|cucumber-testing]]'s rules.

### Always collect coverage
Collect coverage as part of every `test-kind-unit` run; report it in a `report` run.
- Risk: a run without coverage gives mutation testing nothing to scope against and leaves "was this even executed" unanswered.
- Fix: wire coverage collection into `test-kind-unit` unconditionally; only writing `result/coverage-test.json` and `report/coverage/` depends on the purpose.

### Read only the normalized result files
Have `test-report-build` read only the kinds' normalized `result/*.json` files defined in [# Report contract](#report-contract) — never parse a tool's native report format directly.
- Risk: switching the underlying tool later breaks every consumer that learned to parse its specific native format.
- Fix: read `result/*.json` only; treat `report/{name}/` as opaque, human-facing output.

### Write scenarios.json on every run
Have `test-kind-unit` write `result/scenarios.json` per [## Scenario report](#scenario-report) on every run — including a run where a test failed — with its inventory taken from the `.feature` files, not only from the runner's result.
- Violation: building the list only from what the runner executed, so `@todo` entries vanish; or skipping the write because the runner exited non-zero.
- Risk: the report hides exactly the cases that most need attention — planned-but-missing scenarios and the run that failed.
- Fix: parse every `.feature` file for the inventory, join the runner's per-scenario result onto it, write the file, then exit with the runner's own exit code.

### Propagate the mutation tool's exit code
Propagate the underlying mutation tool's own exit code after writing `result/mutation-test.json` — never let a `test-kind-mutation` run swallow it while normalizing its result.
- Risk: a real mutation-testing failure gets hidden, and CI reports success on a run that actually found unkilled mutants.
- Fix: exit with the underlying tool's code after the normalized result is written.

### State what the run's purpose changes
Have every kind say what it does because of `TEST_RUN_PURPOSE` / `DELTA_BASE` through `test-kind-mode`, or skip itself through `test-kind-skip` with the reason — in the log and in `run.json`.
- Violation: a kind that silently narrows or drops its work in a `check` run.
- Risk: a check that wrongly decided not to run cannot be told apart from one that does not apply.
- Fix: one `mode` line per run, or a `skipped` line; never a silent branch.

### Keep a caller ignorant of the kinds
Never require a caller to name a kind, a tool, or a file beyond [# Caller contract](#caller-contract); a new kind is added by declaring it in `TEST_KINDS` / `TEST_BADGES_{kind}` and adding its README badge.
- Violation: a workflow that runs `make test-kind-mutation` by name, or reads `result/mutation-test.json`.
- Risk: every added kind needs a change in every caller.
- Fix: callers iterate `make test-kinds` and publish `$TEST_REPORT_DIR` as-is.

### Render the living doc from the standard Cucumber report
Apply the living-doc MUSTs in [[./Implementation/Repository.create.md#MUST|Repository]], [[./Implementation/tools/livingdoc/package.json.create.md#MUST|tools/livingdoc/package.json]], and [[./Implementation/tools/livingdoc/render.mjs.create.md#MUST|tools/livingdoc/render.mjs]] per [## Living-doc report](#living-doc-report).
- Violation: a stack renders its own HTML from a tool-specific format, or installs the renderer into the project's `package.json`.
- Risk: the living-doc view differs per stack and its renderer versions drift.
- Fix: write the standard protocol to `report/tests/cucumber/` and call the shared renderer.

### Keep report-template/index.html in place
Keep `report-template/index.html` at that path, copied verbatim by `test-report-build` — never generated, never placed under `.github/`.
- Risk: nesting a project-owned static asset inside `.github/` implies this solution owns a workflow or Pages configuration it does not — the actual publishing step is a separate, layered CI concern.
- Fix: keep the file at `report-template/index.html` and have `test-report-build` copy it as-is.

# Check list
- [ ] Every scenario follows [[skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md|cucumber-testing]]'s check list.
- [ ] Every item of [[./Implementation/Repository.create.md#check-list|Repository]]'s check list holds.
- [ ] Coverage is collected on every `test-kind-unit` run and reported in a `report` run.
- [ ] No caller — workflow, script, documentation — names a kind, a tool, or a file beyond [# Caller contract](#caller-contract).
- [ ] `reports/scenarios/index.html` is rendered from `result/scenarios.json` and lists `` entries.
