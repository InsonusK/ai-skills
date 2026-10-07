---
description: Makefile wiring of the testing contract — test kinds, the report builder, and the shared tools/testing files
element_kind: repository
change_kind: create
tags:
  - solution/conformance-testing
  - element/repository
---

# Structure

## Project Structure
```
Makefile                ← declares the kinds, includes tools/testing/testing.mk; created when missing, otherwise extended
README.md               ← one badge per declared badge
report-template/
  index.html
tools/
  testing/
    testing.mk          ← see tools/testing/testing.mk.create.md
    testing.sh          ← see tools/testing/testing.sh.create.md
  livingdoc/
    package.json        ← pinned renderers, see tools/livingdoc/package.json.create.md
    package-lock.json
    render.mjs          ← see tools/livingdoc/render.mjs.create.md
```

## Makefile
```makefile
TEST_KINDS           := unit mutation
TEST_BADGES_unit     := tests coverage
TEST_BADGES_mutation := mutation
include tools/testing/testing.mk

test-kind-unit:
	@$(test-kind-begin)
	…the stack's recipe, writing only below $$TEST_KIND_DIR

test-kind-mutation:
	@$(test-kind-begin)
	…

test-report-build:
	…the stack's report builder
```

## Targets a caller uses
| Target | Purpose |
| --- | --- |
| `make test-kinds` | Print one line per kind — `{kind} {badge} {badge} …` — and run nothing |
| `make test-kind-{kind}` | Run one test kind. Kinds are independent: any order, in parallel, each from a clean checkout |
| `make test-report` | Build the report from whatever the kinds left in the work directory |
| `make test-readme-check` | Fail when the README lacks a declared badge or shows an undeclared one. Runs no tests |
| `make test-and-report` | Every kind, then the report — for a developer or an agent |

## Variables a caller sets
| Variable | Values | Default |
| --- | --- | --- |
| `TEST_RUN_PURPOSE` | `check` — the run decides whether a change may proceed (a pull request merged, a release published), so it must be fast; `report` — the run builds the full reports and badges for publishing | `report` |
| `DELTA_BASE` | the ref to compare against, when a kind can limit itself to changed code | empty |
| `TEST_WORK_DIR` | where kinds write their results | `tmp/testing` |
| `TEST_REPORT_DIR` | where `test-report` writes the publishable report | `$(TEST_WORK_DIR)/report` |

The variables state facts about the run, never how to test. Each kind decides what they mean for it:

| Kind | `report` | `check` |
| --- | --- | --- |
| `unit` | every test, coverage collected and reported | every test, coverage not reported |
| `mutation` | the whole project | only code changed since `DELTA_BASE`; skipped when `DELTA_BASE` is empty |

## Kind output
A kind writes only below `$TEST_KIND_DIR` = `$TEST_WORK_DIR/kinds/{kind}/`:

| File | Content | Written by |
| --- | --- | --- |
| `mode` | one line: what the kind did because of the run's purpose | every kind that ran — `$(call test-kind-mode,…)` |
| `skipped` | one line: why the kind does not apply to this run | a kind that skipped itself — `$(call test-kind-skip,…)` |
| `result/unit-test.json` | `{ "total": <int>, "passed": <int>, "failed": <int> }` | `unit` |
| `result/coverage-test.json` | `{ "linePct": <number> }` | `unit` (`report` only) |
| `result/scenarios.json` | `{ "scenarios": [ { "feature", "scenario", "examples", "uri", "line", "type", "status", "note" } ] }` — see [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-report|Scenario report]] | `unit` (every run, also when a test failed) |
| `result/mutation-test.json` | `{ "killed": <int>, "survived": <int>, "timedout": <int>, "noCoverage": <int>, "score": <number> }` | `mutation` |
| `report/tests/` | the tool's native test report | `unit` |
| `report/tests/cucumber/` | the runner's standard Cucumber report — `*.json` (classic Cucumber JSON) or `*.ndjson` (Cucumber Messages), one protocol per stack | `unit` |
| `report/tests/livingdoc/` | living-doc HTML rendered from `report/tests/cucumber/` by `tools/livingdoc/render.mjs` | `unit` (skipped when `npm` is unavailable) |
| `report/coverage/` | the tool's native coverage report | `unit` (`report` only) |
| `report/mutation/` | the tool's native mutation report | `mutation` |

`score` in `mutation-test.json` is `killed / (killed+survived+timedout+noCoverage) * 100`, rounded to 1 decimal, `"0.0"` when nothing was mutated.

## Report output
`test-report` empties `$TEST_REPORT_DIR`, runs the stack's `test-report-build`, then `tools/testing/testing.sh report-finish`:

| File | Content | Source |
| --- | --- | --- |
| `index.html` | entry point; copied verbatim, never generated | `report-template/index.html` |
| `reports/{name}/` | one folder per report: a copy of every `kinds/*/report/{name}/`, plus `reports/scenarios/index.html` | the kinds' `report/` folders; `result/scenarios.json` |
| `badges/{name}.json` | shields.io endpoint-badge schema: `{"schemaVersion":1,"label":"<label>","message":"<value>","color":"<color>"}` — `tests`, `coverage`, `mutation` | computed from the kinds' `result/*.json` |
| `run.json` | `{ "purpose", "kinds": [ { "kind", "state": "ran"\|"skipped"\|"missing", "note" } ] }` | `report-finish`, from each kind's `mode` / `skipped` |

A badge and its report share a name; a kind may produce several; a report may have no badge (`scenarios`). Names are unique across kinds. `label` is `tests`, `coverage`, or `mutation score`; `color` follows `>=80 brightgreen / >=60 yellowgreen / else red` for percentage metrics, `brightgreen`/`red` for the pass/fail count.

`report-template/index.html` is a small static landing page the project owns — links to `reports/scenarios/`, `reports/tests/`, `reports/tests/livingdoc/`, `reports/coverage/`, `reports/mutation/`, and a block that shows `run.json`:
```html
<h2>This run</h2>
<pre id="run"></pre>
<script>
  fetch('run.json').then(r => r.json()).then(run => {
    document.getElementById('run').textContent = 'purpose: ' + run.purpose + '\n' +
      run.kinds.map(k => k.kind + ': ' + k.state + (k.note ? ' — ' + k.note : '')).join('\n');
  }).catch(() => {});
</script>
```
It lives at the repository root, never under `.github/`, since this solution owns no `.github/workflows/*` file.

## README badges
One badge per declared badge, its URL ending with `badges/{name}.json` under wherever the report is published, linking `reports/{name}/`:
```markdown
[![tests](https://img.shields.io/endpoint?url={published-report-url}/badges/tests.json)]({published-report-url}/reports/tests/)
```

# Rule

## MUST
- Declare every kind in `TEST_KINDS` and its badges in `TEST_BADGES_{kind}`, include `tools/testing/testing.mk`, and define `test-kind-{kind}` and `test-report-build` — nothing else of the contract in the project's `Makefile`.
  - Risk: a caller-facing target or variable defined per project differs between projects, and every caller needs stack knowledge again.
  - Fix: keep the caller-facing half in the shared file; the project's `Makefile` holds only the declaration and the stack's recipes.
- Add the contract to an existing `Makefile`, leaving its other targets as they are; create the `Makefile` when the repository has none.
  - Risk: replacing a `Makefile` another skill or the project wrote removes its targets.
  - Fix: append the declaration, the `include`, and the recipes.
- Start every `test-kind-{kind}` recipe with `$(test-kind-begin)` and write nothing outside `$TEST_KIND_DIR`.
  - Risk: a kind reading or overwriting another kind's files cannot run in parallel with it, and a CI job cannot hand its result over as one directory.
  - Fix: take every output path from `$TEST_KIND_DIR`.
- Have every kind state what it does because of `TEST_RUN_PURPOSE` / `DELTA_BASE` through `$(call test-kind-mode,…)`, or skip itself through `$(call test-kind-skip,…)` — never branch silently.
  - Violation: a `mutation` recipe that quietly does nothing in a `check` run.
  - Risk: a kind that wrongly decided not to run looks the same as one that does not apply, and a check disappears unnoticed.
  - Fix: one `mode` line per run, or a `skipped` line with the reason; both reach the log and `run.json`.
- Keep `test-kind-unit` running both Cucumber scenarios and plain technical tests in a single invocation — never split them into two kinds.
  - Risk: a caller running one kind gets an incomplete picture of whether the tests pass.
  - Fix: configure the stack's test runner so one `make test-kind-unit` executes everything.
- Write a kind's normalized `result/*.json` and its native `report/{name}/` per [## Kind output](#kind-output), before exiting with the tool's own exit code — also when a test failed.
  - Risk: a failed run leaves no report to read, or a real failure is swallowed while normalizing.
  - Fix: write the results, then `exit` with the code the runner or the mutation tool returned.
- Write `result/scenarios.json` on every `test-kind-unit` run, listing every `.feature` entry, `@todo` ones included.
  - Risk: a report built only from executed scenarios hides planned-but-missing cases.
  - Fix: build the inventory from the `.feature` files and join the runner's result onto it.
- Have `test-report-build` read only the kinds' `result/*.json` and copy their `report/{name}/` folders — never parse a tool's native report — and write `$TEST_REPORT_DIR` per [## Report output](#report-output).
  - Risk: switching a tool breaks the report builder; a non-uniform report forces the publisher to know the stack.
  - Fix: glob `$TEST_WORK_DIR/kinds/*/result/` and `…/report/*/`; compute badges from the normalized results.
- Have `test-kind-unit` make the Cucumber runner write its standard report — the protocol its `cucumber-testing-in-{stack}` skill names — into `$TEST_KIND_DIR/report/tests/cucumber/`, then render it with `npm ci --prefix tools/livingdoc && node tools/livingdoc/render.mjs $TEST_KIND_DIR/report/tests/cucumber $TEST_KIND_DIR/report/tests/livingdoc`.
  - Risk: without a standard report there is no stack-independent input for the living-doc view, and each stack builds its own HTML.
  - Fix: configure the runner's classic-JSON or Messages formatter to write there, and call the shared renderer.
- Skip the living-doc step with a message when `npm` is unavailable, and never let it change the kind's exit code.
  - Risk: a machine without Node fails the test target, or a rendering error masks a red test run.
  - Fix: guard with `command -v npm`, and append `|| echo "livingdoc: render failed"` instead of propagating.
- Add one README badge per declared badge, per [## README badges](#readme-badges), in the same change that declares it.
  - Risk: `make test-readme-check` fails the pull request with "you forgot to add a badge".
  - Fix: add the badge line; remove it when the kind is removed.
- Accept no caller-facing variable beyond `TEST_RUN_PURPOSE`, `DELTA_BASE`, `TEST_WORK_DIR`, `TEST_REPORT_DIR`.
  - Risk: a caller needs stack knowledge to invoke the targets, defeating the uniform contract.
  - Fix: derive anything tool-specific inside the kind from those four.

# Check list
- [ ] `make test-kinds` lists every kind with its badges; `make test-kind-{kind}` exists for each.
- [ ] `tools/testing/testing.mk` and `tools/testing/testing.sh` are verbatim copies.
- [ ] An existing `Makefile` kept its other targets.
- [ ] `make test-kind-unit` runs Cucumber scenarios and plain tests together and writes only below `$TEST_KIND_DIR`.
- [ ] Every kind leaves `mode` or `skipped`; `run.json` shows it.
- [ ] `result/unit-test.json`, `result/coverage-test.json` (`report`), `result/mutation-test.json` follow [## Kind output](#kind-output); `result/scenarios.json` exists after every unit run, green or red, with `@todo` entries.
- [ ] `report/tests/cucumber/` holds the runner's standard report; `report/tests/livingdoc/index.html` exists when `npm` is available, and its absence never fails the kind.
- [ ] Each kind exits with its tool's own exit code after writing its results.
- [ ] `make test-report` writes `index.html`, `reports/`, `badges/`, `run.json` into `$TEST_REPORT_DIR`; a different `TEST_WORK_DIR` / `TEST_REPORT_DIR` moves everything, and nothing is written to `public/`.
- [ ] `make test-and-report TEST_RUN_PURPOSE=check` skips or narrows kinds as the table says and still builds a report.
- [ ] `make test-readme-check` passes; `report-template/index.html` exists at the repository root (not under `.github/`).
