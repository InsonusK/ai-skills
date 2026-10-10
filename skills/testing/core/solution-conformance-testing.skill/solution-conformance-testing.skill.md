---
name: solution-conformance-testing
description: Defines one unified approach to writing and running tests across projects — Cucumber scenarios for business and technical/architectural functionality alike, code coverage, mutation testing, and a Makefile contract of independent test kinds and one report — so testing quality is controlled the same way regardless of stack
whenToUse: when setting up or reviewing a project's testing strategy, when deciding whether a new test case belongs as a Cucumber scenario or a plain test, or when wiring a project's Makefile test targets
domain: skill
type: architecture
version: 10
updated: 20261010
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
  - tools/testing/kind.sh
  - tools/testing/test-report.sh
  - tools/testing/normalize-scenarios.sh
  - tools/testing/messages-results.jq
extends:
depends_on:
built_on_plateau:
adr:
  - adr/one-livingdoc-view.md
  - "[[skills/testing/core/solution-conformance-testing.skill/adr/mutation-tool-per-stack.md|Mutation-testing tool per stack]]"
  - "[[skills/testing/core/solution-conformance-testing.skill/adr/scenario-report.md|Scenario report from tagged .feature files]]"
  - "[[skills/testing/core/solution-conformance-testing.skill/adr/livingdoc-renderer-per-protocol.md|Living-doc renderer per Cucumber report protocol]]"
  - "[[skills/testing/core/solution-conformance-testing.skill/adr/caller-contract.md|Caller contract: independent test kinds, purpose of the run, caller-chosen directories]]"
---

# Goal
- Create one unified approach to writing tests across projects, to increase control over testing quality.

# Capabilities
- One readable living doc per project listing business and technical scenarios with their tags, results, and exclusion reasons; the normalized inventory also drives the tag check — see [## Scenario inventory](#scenario-inventory).
- Mutation testing on top of coverage, so a weak assertion shows up as a surviving mutant instead of a passing coverage number.
- Uniform `make` targets and variables a caller uses without knowing the project's stack, which test kinds exist, or how they run — see [# Caller contract](#caller-contract).
- A report every kind fills itself — its own report folder and its own badge — so a new test kind is published with no change to the report builder; see [# Report contract](#report-contract).
- A living-doc HTML view of every scenario, the excluded ones with their reason — filterable by tag and status, identical in every stack — rendered from the runner's standard Cucumber report, see [## Living-doc report](#living-doc-report).

# Core Principles
- Every test run produces a report describing covered test cases in a readable form.
- Mutation testing verifies testing quality — coverage alone only proves a code path executed, not that its result was checked.
- Every test case — business and technical/architectural alike — is written as a Cucumber scenario; see [[skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md|cucumber-testing]] for how to author and organize scenarios and step definitions.
- Code coverage is always collected; it is reported in a `report` run.
- The mutation-testing tool is chosen per stack, not per project: Stryker for C#/.NET and for Angular/TypeScript, Mutmut for Python, Gremlins for Go — see [[./adr/mutation-tool-per-stack.md|ADR]].
- Tests run as independent kinds (`make test-kind-{kind}`) and `make test-report` builds one report; a caller states what the run is for, never how to test.
- One `Makefile`, one report builder, and one script per test kind: only `tools/testing/kinds/{kind}.sh` differs between stacks — it runs the stack's tool and writes normalized JSON; everything before and after it is shared.

# Adr
- [[./adr/mutation-tool-per-stack.md|Mutation-testing tool per stack]]
  - Selected variant: Stryker for C#/.NET and Angular/TypeScript, Mutmut for Python, Gremlins for Go
- [[./adr/scenario-report.md|Scenario report from tagged .feature files]]
  - Selected variant: the `unit` kind writes a normalized `result/scenarios.json` built from the `.feature` files plus the runner's result; the unit kind checks its tags and the living doc reads excluded entries — no hand-maintained test inventory file
- [[./adr/livingdoc-renderer-per-protocol.md|Living-doc renderer per Cucumber report protocol]]
  - Selected variant: one shared pinned classic-JSON renderer in `tools/livingdoc/`, with Messages converted first; [[./adr/one-livingdoc-view.md|W16 decision]] supersedes the per-protocol view
- [[./adr/caller-contract.md|Caller contract]]
  - Selected variant: `test-kind-{kind}` targets discovered through `test-kinds`, `TEST_RUN_PURPOSE` / `DELTA_BASE` as facts about the run, caller-chosen work and report directories, badges checked against the README

# Caller contract
Whoever runs the tests — a developer, an agent, a CI workflow — uses only the targets and variables in [[./Implementation/Repository.create.md#targets-a-caller-uses|Repository]], shipped identically to every stack as `tools/testing/`: `make test-kinds` to learn the kinds, `make test-kind-{kind}` to run one, `make test-report` to build the report, `make test-readme-check` for the README badges, and `TEST_RUN_PURPOSE`, `DELTA_BASE`, `TEST_WORK_DIR`, `TEST_REPORT_DIR`. A caller never names a test tool, a coverage switch, or a report file other than `index.html`, `reports/{name}/`, and `badges/{name}.json`. Decision in [[./adr/caller-contract.md|adr/caller-contract]].

# Report contract
Every kind writes its whole output below its own `$TEST_KIND_DIR`, per [[./Implementation/Repository.create.md#kind-output|Kind output]], and decides what that output is:

- **Report** — `report/{name}/`: what a person opens. The tool's own report where it has one, a page the kind renders where it has none.
- **Badge** — `badges/{name}.json`: the one-line result of the report of the same name, written through the `kind_badge` functions of `tools/testing/kind.sh`, which fix its schema and colors.
- **Result** — `result/*.json`: the kind's data. The `unit` and `mutation` kinds keep the same four files in every stack — `unit-test.json`, `coverage-test.json`, `scenarios.json`, `mutation-test.json`; only the tag check and the living-doc renderer read the scenario inventory; the report builder reads none of them.

`make test-report` computes nothing: it gathers every kind's reports and badges, adds an entry page to a report that has none, and lists them on the landing page. A kind exits with its tool's own exit code after writing its output — writing it is a side effect, never a reason to swallow a real failure.

## Scenario inventory
`test-kind-unit` writes `result/scenarios.json` on every run — also when a test failed:
```json
{ "scenarios": [
  { "feature": "Check a URL", "type": "service",
    "scenario": "Check a URL", "examples": "malformed", "category": "negative",
    "status": "passed", "validated": false,
    "tags": ["@category/negative", "@type/service"],
    "uri": "internal/domain/services/features/check.feature", "line": 18, "note": "" }
] }
```
- **Entry** — one `Scenario`/`Example`, or one `Examples:` block of a `Scenario Outline`. `examples` is that block's name (`""` for a plain scenario); `line` is the `Scenario` line, or the `Examples:` line for a block; `uri` is the `.feature` path relative to the repository root.
- **Inventory source** — the `.feature` files themselves, so `@status/todo` and `@status/broken` entries the runner never executes are listed too. Only `passed` / `failed` come from the runner's own result.
- **`type`** — the value of the one `@type/…` tag on the `Feature:` line, per [[skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#one-type-tag-per-feature|One type tag per feature]]; `none` when there is none, more than one, or an unknown value.
- **`category`** — the value of the one `@category/…` tag among the entry's own and inherited (`Feature`, `Rule`, `Scenario`, `Examples`) tags, per [[skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#one-category-tag-per-scenario|One category tag per scenario]]; `none` when there is none, more than one, or an unknown value.
- **`status`** — what the run says about the entry:

  | Status | Meaning |
  | --- | --- |
  | `passed` | it ran and passed |
  | `failed` | it ran and failed — for an `Examples:` block: any of its rows failed |
  | `todo` | tagged `@status/todo`: planned, excluded from the run |
  | `broken` | tagged `@status/broken`: known to fail, excluded from the run |
  | `not-run` | not excluded, yet the runner reported no result for it — a wiring defect, never a pass |

- **`validated`** — `true` when the entry carries `@status/validated`: a person has checked it, per [[skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#only-a-person-sets-statusvalidated|Only a person sets @status/validated]].
- **`tags`** — every tag the entry carries or inherits, with `@`, sorted.
- **`note`** — the `# todo:` or `# broken:` reason of an excluded entry, per [[skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#exclude-an-unrunnable-scenario-with-a-status-tag-and-its-reason|Exclude an unrunnable scenario with a status tag and its reason]]; `""` otherwise.

`kind_scenarios_check` reads this inventory and fails the unit kind when a feature has no single type tag or a scenario/Examples block has no single category tag, naming every offender. The shared living-doc renderer reads the same inventory to add excluded and not-run scenarios with their tags and reasons. The living doc is the only scenario view a reader opens; there is no second inventory HTML page.

The `.feature` file at `uri:line` supplies the assertions; surviving mutants in `reports/mutation/` expose weak assertions, and `reports/coverage/` shows unreached code.

## Living-doc report
`test-kind-unit` makes the Cucumber runner write its standard report into `report/tests/cucumber/`, in the one protocol its `cucumber-testing-in-{stack}` skill names:
- **classic Cucumber JSON** (`*.json`, `features[].elements[].steps[]`) — rendered by `multiple-cucumber-html-reporter`;
- **Cucumber Messages** (`*.ndjson` envelope stream) — converted to classic JSON with actual executed steps and results, then rendered by the same renderer.

Then it renders `report/tests/livingdoc/` with the shared [[./Implementation/tools/livingdoc/render.mjs.create.md|tools/livingdoc/render.mjs]], from the isolated, pinned install in [[./Implementation/tools/livingdoc/package.json.create.md|tools/livingdoc/package.json]] — never from the project's own dependency manifest. Both protocols show every scenario with all its tags; excluded and not-run entries are added from `result/scenarios.json` with their reasons. Every report ends with the status legend. `kind_livingdoc` makes `reports/tests/` forward directly to this view, and `test-report` gathers it unchanged. The step needs Node 22+ and is skipped where npm is missing. Decision: [[./adr/one-livingdoc-view.md|One living-doc view]].

## Report output
`test-report` builds `$TEST_REPORT_DIR` — `index.html`, `reports/{name}/`, `badges/{name}.json`, `run.json` — per [[./Implementation/Repository.create.md#report-output|Report output]]: the one stack-independent artifact a publishing step uploads as-is. Where it is published, and under which path, is the publisher's choice; this solution owns no `.github/workflows/*` file and writes nothing outside the two directories the caller names.

# Requirements
None at this level — stack-specific packages (the Cucumber runner, the coverage collector, the mutation tool) are declared by each stack's own extending solution.

# Template Skill Mutations
REPOSITORY:
- [[./Implementation/Repository.create.md|Repository]] - create - the `Makefile` include line, `tools/testing/` (the Makefile side, the runner, the report builder, the kind-script library), `README.md` badges, `report-template/index.html`
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

### Gather, never compute, in test-report
Have `tools/testing/test-report.sh` copy the kinds' `report/` and `badges/` folders and build the landing page from them — never read a `result/` file, never parse a tool's native report.
- Risk: a builder that computes a badge knows which kinds exist; the next kind — a component test, a pixel test — cannot publish its badge without the shared builder being changed in every project.
- Fix: the kind writes its badge with `kind_badge*` and renders its own page; `report/{name}/` and `badges/{name}.json` stay opaque to the builder.

### Write scenarios.json on every run
Have `test-kind-unit` write `result/scenarios.json` per [## Scenario inventory](#scenario-inventory) on every run — including a run where a test failed — with its inventory taken from the `.feature` files, not only from the runner's result.
- Violation: building the list only from what the runner executed, so `@status/todo` entries vanish; or skipping the write because the runner exited non-zero.
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
Never require a caller to name a kind, a tool, or a file beyond [# Caller contract](#caller-contract); a new kind is added by writing `tools/testing/kinds/{kind}.sh` with its `# badges:` line and adding its README badge.
- Violation: a workflow that runs `make test-kind-mutation` by name, or reads `result/mutation-test.json`.
- Risk: every added kind needs a change in every caller.
- Fix: callers iterate `make test-kinds` and publish `$TEST_REPORT_DIR` as-is.

### Render the living doc from the standard Cucumber report
Apply the living-doc MUSTs in [[./Implementation/Repository.create.md#MUST|Repository]], [[./Implementation/tools/livingdoc/package.json.create.md#MUST|tools/livingdoc/package.json]], and [[./Implementation/tools/livingdoc/render.mjs.create.md#MUST|tools/livingdoc/render.mjs]] per [## Living-doc report](#living-doc-report).
- Violation: a stack renders its own HTML from a tool-specific format, or installs the renderer into the project's `package.json`.
- Risk: the living-doc view differs per stack and its renderer versions drift.
- Fix: write the standard protocol to `report/tests/cucumber/` and call the shared renderer.

### Keep report-template/index.html in place
Keep `report-template/index.html` at that path — `tools/testing/test-report.sh` publishes it with only its `<!-- test-reports -->` line filled in — never placed under `.github/`.
- Risk: nesting a project-owned static asset inside `.github/` implies this solution owns a workflow or Pages configuration it does not — the actual publishing step is a separate, layered CI concern.
- Fix: keep the file at `report-template/index.html`; everything but the marker line reaches the report unchanged.

# Check list
- [ ] Every scenario follows [[skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md|cucumber-testing]]'s check list.
- [ ] Every item of [[./Implementation/Repository.create.md#check-list|Repository]]'s check list holds.
- [ ] Coverage is collected on every `test-kind-unit` run and reported in a `report` run.
- [ ] No caller — workflow, script, documentation — names a kind, a tool, or a file beyond [# Caller contract](#caller-contract).
- [ ] `result/scenarios.json` exists even after a failed unit run; the unit kind checks its tags, and the living doc lists every entry with its tags, exclusion reasons and status legend.
