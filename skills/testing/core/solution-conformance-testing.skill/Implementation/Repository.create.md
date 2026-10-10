---
description: The testing contract in a repository — one include line in the Makefile, the shared tools/testing files, one script per test kind
element_kind: repository
change_kind: create
tags:
  - solution/conformance-testing
  - element/repository
---

# Structure

## Project Structure
```
Makefile                ← one line: include tools/testing/testing.mk (created when missing)
README.md               ← one badge per declared badge
report-template/
  index.html
tools/
  testing/
    testing.mk          ← the Makefile side: caller targets and variables
    testing.sh          ← runs a kind, builds and checks the report, checks the README
    kind.sh             ← sourced by every kind script: kind_mode, kind_skip, kind_badge*, kind_scenarios_check, kind_livingdoc
    test-report.sh      ← gathers report/ and badges/ of every kind into the report directory
    normalize-scenarios.sh, messages-results.jq   ← helpers a kind script may call
    kinds/
      unit.sh           ← one script per test kind — the only stack-specific part
      mutation.sh
  livingdoc/
    package.json, package-lock.json, render.mjs
```

## Files
- Copy verbatim, as a folder, to `tools/testing/`: [`assets/tools/testing/`](../assets/tools/testing/) — `testing.mk`, `testing.sh`, `kind.sh`, `test-report.sh`, `normalize-scenarios.sh`, `messages-results.jq`. Identical in every stack.
- Copy verbatim, as a folder, to `tools/livingdoc/`: [`assets/tools/livingdoc/`](../assets/tools/livingdoc/).
- Add to the `Makefile`: `include tools/testing/testing.mk`.
- Add to the repository's `.gitignore` (create it when missing): `tmp/` — the default work directory — and `tools/livingdoc/node_modules/`, plus the lines the stack's `solution-conformance-testing-in-{stack}` skill names.
- `tools/testing/kinds/{kind}.sh` comes from the stack's `solution-conformance-testing-in-{stack}` skill.

## A kind script
```bash
#!/usr/bin/env bash
# badges: tests coverage            ← the badges this kind produces in a report run
set -euo pipefail
source tools/testing/kind.sh        # RESULT_DIR, REPORT_DIR, BADGE_DIR and the kind_* functions

if [ "$TEST_RUN_PURPOSE" = report ]; then kind_mode "every test with coverage reported"
else kind_mode "every test - coverage not reported"; fi

…run the stack's tool, keeping its exit code in $status; write $REPORT_DIR/tests/…
kind_badge_count tests tests "$passed" "$total"        # badges/tests.json
kind_badge_percent coverage coverage "$line_pct"      # badges/coverage.json, report run only
exit "$status"                      # the tool's own exit code
```
A kind decides everything about its own output — what it measures, which badge shows it, which report explains it; `test-report` only gathers. Its badges go through `kind.sh`, so their schema and colors are the same in every kind and stack:

| Function | Badge |
| --- | --- |
| `kind_badge_count {name} {label} {passed} {total}` | `{passed}/{total} passed` — `brightgreen` when all passed, else `red` |
| `kind_badge_percent {name} {label} {percent}` | `{percent}%` — `>=80` `brightgreen`, `>=60` `yellowgreen`, else `red` |
| `kind_badge {name} {label} {message} {color}` | anything else |

A kind exists because its script exists: `make test-kinds` lists `tools/testing/kinds/*.sh`, and `make test-kind-{kind}` runs one of them in an emptied `$TEST_KIND_DIR`.

## Targets a caller uses
| Target | Purpose |
| --- | --- |
| `make test-kinds` | Print one line per kind — `{kind} - badges: {badge} {badge} …`, or `{kind} - no badge` — and run nothing. The kind is the first word of the line |
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
| `mode` | one line: what the kind did because of the run's purpose | every kind that ran — `kind_mode "…"` |
| `skipped` | one line: why the kind does not apply to this run | a kind that skipped itself — `kind_skip "…"` |
| `exit-code` | the kind script's exit code | `testing.sh`, after the script ends |
| `badges/{name}.json` | the badge of the report of the same name — shields.io endpoint schema: `{"schemaVersion":1,"label":"<label>","message":"<value>","color":"<color>"}` | the kind, through `kind_badge*`; `unit`: `tests`, and `coverage` in a `report` run; `mutation`: `mutation` |
| `result/unit-test.json` | `{ "total": <int>, "passed": <int>, "failed": <int> }` | `unit` |
| `result/coverage-test.json` | `{ "linePct": <number> }` | `unit` (`report` only) |
| `result/scenarios.json` | `{ "scenarios": [ { "feature", "type", "scenario", "examples", "category", "status", "validated", "tags", "uri", "line", "note" } ] }` — see [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-inventory|Scenario inventory]] | `unit` (every run, also when a test failed) |
| `result/mutation-test.json` | `{ "killed": <int>, "survived": <int>, "timedout": <int>, "noCoverage": <int>, "score": <number> }` | `mutation` |
| `report/tests/` | the tool's native test report; its entry page forwards to `livingdoc/` when the tool wrote none | `unit` — `kind_livingdoc` |
| `report/tests/cucumber/` | the runner's standard Cucumber report — `*.json` (classic Cucumber JSON) or `*.ndjson` (Cucumber Messages), one protocol per stack | `unit` |
| `report/tests/livingdoc/` | living-doc HTML rendered from `report/tests/cucumber/` by `tools/livingdoc/render.mjs` | `unit` (skipped when `npm` is unavailable) |
| `report/coverage/` | the tool's native coverage report | `unit` (`report` only) |
| `report/mutation/` | the tool's native mutation report | `mutation` |

`result/` is the kind's own data: nothing outside the kind reads it. The four files above keep one shape in every stack so a person or a tool can compare runs; a new kind names its result files as it likes.

`score` in `mutation-test.json` is `killed / (killed+survived+timedout+noCoverage) * 100`, rounded to 1 decimal, `"0.0"` when nothing was mutated.

## Report output
`test-report` empties `$TEST_REPORT_DIR`, runs `tools/testing/test-report.sh` — the same script in every stack — then records and checks the result. It exits `0` when a kind failed: that kind's own exit code is the signal, and the report of a red run must still be built and published. It exits non-zero only for a broken report — no `index.html`, a badge without a report or without a declaring kind, or in a `report` run a kind that exited `0` without a badge it declares.

| File | Content | Source |
| --- | --- | --- |
| `index.html` | entry point: the project's landing page with its `<!-- test-reports -->` line replaced by one item per report of this run — the badge, then the link to `reports/{name}/`; reports with a badge first | `report-template/index.html`; `badges/`, `reports/` |
| `reports/{name}/` | one folder per report: a copy of every `kinds/*/report/{name}/` | the kinds' `report/` folders |
| `reports/{name}/index.html` | when the tool wrote none: a list of what the folder holds, so `reports/{name}/` — the target of the landing page and of a README badge — opens on a static host | `test-report.sh` |
| `badges/{name}.json` | shields.io endpoint-badge schema: `{"schemaVersion":1,"label":"<label>","message":"<value>","color":"<color>"}` | a copy of every `kinds/*/badges/{name}.json` |
| `run.json` | `{ "purpose", "kinds": [ { "kind", "state": "ran"\|"failed"\|"skipped"\|"missing", "note" } ] }` — `failed`: the kind exited non-zero or never finished | `testing.sh`, from each kind's `mode` / `skipped` / `exit-code` |

A badge and its report share a name; a kind may produce several; a report may have no badge (`scenarios`). Names are unique across kinds. The labels in use are `tests`, `coverage` and `mutation score`.

`report-template/index.html` is a small landing page the project owns: the `<!-- test-reports -->` line `test-report.sh` fills, and a block that shows `run.json`. The `tests` item opens the living doc. The list holds only what the run produced, so a `check` run links no coverage and no mutation report, and a new kind's report appears with no change to the page. Fill and copy [`templates/report-template/index.html`](../templates/report-template/index.html) — `{project-name}` = the project's name. A page without the marker line is published as it is.
It lives at the repository root, never under `.github/`, since this solution owns no `.github/workflows/*` file.

## README badges
One badge per declared badge, its URL ending with `badges/{name}.json` under wherever the report is published, linking `reports/{name}/`:
```example
[![tests](https://img.shields.io/endpoint?url={published-report-url}/badges/tests.json)]({published-report-url}/reports/tests/)
```

# Rule

## MUST
- Put nothing of the contract into the project's `Makefile` but `include tools/testing/testing.mk`; a test kind is one script `tools/testing/kinds/{kind}.sh` with a `# badges:` line.
  - Risk: a target or recipe written per project differs between projects, and the report builder or a caller needs stack knowledge again.
  - Fix: keep `tools/testing/` verbatim; write only the kind scripts, and only what a stack's tool needs.
- Produce the report with the shared `tools/testing/test-report.sh` in every stack — never a stack's own report builder.
  - Risk: two builders drift, and the same results give different reports per stack.
  - Fix: a kind writes `report/{name}/` and `badges/{name}.json`; gathering them, the landing page and the checks are shared.
- Add the include line to an existing `Makefile` after its first target, leaving everything else as it is; create the `Makefile` when the repository has none.
  - Risk: replacing a `Makefile` removes the project's targets; an include placed first makes `test-kinds` the default goal.
  - Fix: append the line at the end.
- Source `tools/testing/kind.sh` first in every kind script and write nothing outside `$TEST_KIND_DIR`.
  - Risk: a kind reading or overwriting another kind's files cannot run in parallel with it, and a CI job cannot hand its result over as one directory.
  - Fix: take every output path from `$TEST_KIND_DIR`.
- Have every kind state what it does because of `TEST_RUN_PURPOSE` / `DELTA_BASE` through `kind_mode`, or skip itself through `kind_skip` — never branch silently.
  - Violation: a `mutation` script that quietly does nothing in a `check` run.
  - Risk: a kind that wrongly decided not to run looks the same as one that does not apply, and a check disappears unnoticed.
  - Fix: one `mode` line per run, or a `skipped` line with the reason; both reach the log and `run.json`.
- Keep `test-kind-unit` running both Cucumber scenarios and plain technical tests in a single invocation — never split them into two kinds.
  - Risk: a caller running one kind gets an incomplete picture of whether the tests pass.
  - Fix: configure the stack's test runner so one `make test-kind-unit` executes everything.
- Write a kind's `report/{name}/`, its `badges/{name}.json` and its `result/` data per [## Kind output](#kind-output), before exiting with the tool's own exit code — also when a test failed.
  - Risk: a failed run leaves no report to read, or a real failure is swallowed while normalizing.
  - Fix: write the results, then `exit` with the code the runner or the mutation tool returned.
- Exit non-zero from a kind only for a failed check — a red test, an untagged scenario or feature, a tool that could not run, or in a `check` run a threshold the kind enforces — never for a score in a `report` run.
  - Violation: a mutation kind that fails a `report` run because a mutant survived.
  - Risk: a caller reads the exit code as "the tests failed"; a run that is red over a score every time hides the run that is red over a broken test.
  - Fix: in a `report` run give the tool its never-break setting (`--break-at 0`, `thresholds.break = 0`) and return its exit code; a red test always returns non-zero.
- Call `kind_scenarios_check` in `test-kind-unit` after the results are written, and exit non-zero when it fails.
  - Violation: a unit kind that exits `0` while its inventory lists a feature without a type or a scenario without a category.
  - Risk: a missing tag is visible only to someone who opens the report; an agent that runs the tests and sees them green never learns that it left a scenario or a feature unclassified.
  - Fix: `kind_scenarios_check || status=1` after writing the inventory — it prints each feature without exactly one `@type/…` tag and each scenario without exactly one `@category/…` tag to stderr.
- Write `result/scenarios.json` on every `test-kind-unit` run, listing every `.feature` entry, `@status/todo` and `@status/broken` ones included.
  - Risk: a report built only from executed scenarios hides planned-but-missing cases.
  - Fix: build the inventory from the `.feature` files and join the runner's result onto it.
- Have `test-kind-unit` make the Cucumber runner write its standard report — the protocol its `cucumber-testing-in-{stack}` skill names — into `$TEST_KIND_DIR/report/tests/cucumber/`, then call `kind_livingdoc`.
  - Risk: without a standard report there is no stack-independent input for the living-doc view, and each stack builds its own HTML.
  - Fix: configure the runner's classic-JSON or Messages formatter to write there; `kind_livingdoc` renders it, is skipped without `npm`, and never changes the exit code.
- Add one README badge per declared badge, per [## README badges](#readme-badges), in the same change that declares it.
  - Risk: `make test-readme-check` fails the pull request with "you forgot to add a badge".
  - Fix: add the badge line; remove it when the kind is removed.
- Add `tmp/`, `tools/livingdoc/node_modules/` and the stack's own lines to the repository's `.gitignore` in the same change that adds the kinds.
  - Violation: the skill's files copied into a project whose `.gitignore` was left as it was.
  - Risk: the first `make test-and-report` leaves hundreds of untracked report files, and one `git add -A` commits them.
  - Fix: append the missing lines to the existing `.gitignore`; `git status --short` is empty after a run.
- Write every badge through `kind_badge_count`, `kind_badge_percent` or `kind_badge`, under a name the script declares in its `# badges:` line, and write a `report/{name}/` of the same name.
  - Violation: a kind script that prints the JSON itself; a badge computed in `test-report.sh`.
  - Risk: a hand-written badge drifts in schema and colors from the others; a builder that computes badges knows a fixed list of kinds, and a new kind's badge needs the builder changed.
  - Fix: call the function that fits; `test-report` fails when a badge has no report, no declaring kind, or when a kind that ran produced no badge it declares.
- Accept no caller-facing variable beyond `TEST_RUN_PURPOSE`, `DELTA_BASE`, `TEST_WORK_DIR`, `TEST_REPORT_DIR`.
  - Risk: a caller needs stack knowledge to invoke the targets, defeating the uniform contract.
  - Fix: derive anything tool-specific inside the kind from those four.

# Check list
- [ ] `make test-kinds` lists every kind with its badges; `make test-kind-{kind}` exists for each.
- [ ] `tools/testing/` (except `kinds/`) and `tools/livingdoc/` are byte-for-byte copies of the assets; the `Makefile` has the include line and no testing recipe.
- [ ] An existing `Makefile` kept its other targets.
- [ ] `make test-kind-unit` runs Cucumber scenarios and plain tests together and writes only below `$TEST_KIND_DIR`.
- [ ] Every kind leaves `mode` or `skipped`; `run.json` shows it.
- [ ] `result/unit-test.json`, `result/coverage-test.json` (`report`), `result/mutation-test.json` follow [## Kind output](#kind-output); `result/scenarios.json` exists after every unit run, green or red, with `@status/todo` entries.
- [ ] `report/tests/cucumber/` holds the runner's standard report; `report/tests/livingdoc/index.html` exists when `npm` is available, and its absence never fails the kind.
- [ ] `make test-kind-unit` exits non-zero, naming the place, when a feature has no single `@type/…` tag or a scenario no single `@category/…` tag — with every test green.
- [ ] Each kind exits with its tool's own exit code after writing its results. `make test-kind-unit` exits non-zero when a test failed; no kind exits non-zero over a score in a `report` run.
- [ ] `make test-report` writes `index.html`, `reports/`, `badges/`, `run.json` into `$TEST_REPORT_DIR`; a different `TEST_WORK_DIR` / `TEST_REPORT_DIR` moves everything, and nothing is written to `public/`.
- [ ] `make test-and-report TEST_RUN_PURPOSE=check` skips or narrows kinds as the table says and still builds a report.
- [ ] After `make test-and-report`, `git status --short` shows nothing: `.gitignore` holds `tmp/`, `tools/livingdoc/node_modules/` and the stack's lines.
- [ ] `make test-readme-check` passes; `report-template/index.html` exists at the repository root (not under `.github/`).
