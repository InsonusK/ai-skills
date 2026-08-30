---
description: Makefile exposing the four unified testing targets
element_kind: repository
change_kind: create
tags:
  - solution/conformance-testing
  - element/repository
---

# Structure

## Project Structure
```
Makefile
report-template/
  index.html
```

## Makefile targets
| Target | Purpose | Toggles |
| --- | --- | --- |
| `make unit-test` | Run every test — Cucumber scenarios and plain technical tests together, in one run | `WITH_CODE_COVERAGE=true` — also collect and report code coverage |
| `make mutation-test` | Run mutation testing | `ONLY_DELTA=true DELTA_BASE=<ref>` — mutate only code changed since `DELTA_BASE`, instead of the whole project |
| `make test-report` | Assemble a readable report from whatever `unit-test`/`mutation-test` already produced | — |
| `make test-and-report` | Run `unit-test` (with coverage), `mutation-test`, and `test-report` in sequence | forwards `ONLY_DELTA`/`DELTA_BASE` to `mutation-test` |

## Report output
Each `*-test` target writes a normalized JSON result under `tmp/result/`, plus the tool's own native report under `tmp/report/<kind>/`:

| File | Schema | Written by |
| --- | --- | --- |
| `tmp/result/unit-test.json` | `{ "total": <int>, "passed": <int>, "failed": <int> }` | `unit-test` |
| `tmp/result/coverage-test.json` | `{ "linePct": <number> }` | `unit-test` (only with `WITH_CODE_COVERAGE=true`) |
| `tmp/result/mutation-test.json` | `{ "killed": <int>, "survived": <int>, "timedout": <int>, "noCoverage": <int>, "score": <number> }` | `mutation-test` |
| `tmp/report/tests/` | tool's native test report | `unit-test` |
| `tmp/report/coverage/` | tool's native coverage report | `unit-test` (only with `WITH_CODE_COVERAGE=true`) |
| `tmp/report/mutation/` | tool's native mutation report | `mutation-test` |

`score` in `mutation-test.json` is `killed / (killed+survived+timedout+noCoverage) * 100`, rounded to 1 decimal, `"0.0"` when nothing was mutated. `test-report` reads only the `tmp/result/*.json` files to build its own report — it never re-parses a native report.

## Public site output
`test-report` assembles `public/` from `tmp/result/*.json` and `tmp/report/<kind>/` — the one stack-independent artifact a CI publishing step (e.g. GitHub Pages) uploads as-is:

| File | Content | Source |
| --- | --- | --- |
| `public/<kind>/` | copy of `tmp/report/<kind>/`, for each kind present | `tmp/report/tests`, `tmp/report/coverage`, `tmp/report/mutation` |
| `public/tests-badge.json`, `public/coverage-badge.json`, `public/mutation-badge.json` | shields.io endpoint-badge schema: `{"schemaVersion":1,"label":"<label>","message":"<value>","color":"<color>"}` | computed from `tmp/result/*.json` |
| `public/index.html` | copied verbatim, never generated | `report-template/index.html` |

`report-template/index.html` is a small static landing page the project owns (links to `tests/`, `coverage/`, `mutation/`) — it lives at the repository root, never under `.github/`, since this solution owns no `.github/workflows/*` file.

# Rule

## MUST
- Expose all four targets at the repository root — a caller must never need the stack's native test/coverage/mutation CLI directly.
  - Risk: every caller (CI, a developer, another script) now needs stack-specific knowledge, and switching the underlying tool later becomes a breaking change for everyone calling it directly.
  - Fix: wrap every stack tool behind `unit-test`/`mutation-test`/`test-report`/`test-and-report`, and never call the underlying tool directly outside the `Makefile`.
- Keep `unit-test` running both Cucumber scenarios and plain technical tests in a single invocation — never split them into two targets.
  - Risk: a caller invoking only one target gets an incomplete picture, and CI has to remember to invoke both in the right combination.
  - Fix: configure the stack's test runner so one `make unit-test` executes everything.
- Make `test-and-report` depend on `unit-test`, then `mutation-test`, then `test-report`, in that order.
  - Risk: running `test-report` before the other two produces a report from stale or missing results.
  - Fix: declare the dependency chain in that exact order.
- Write both the normalized `tmp/result/<name>.json` and the tool's native report under `tmp/report/<kind>/` for every `*-test` target, per [## Report output](#report-output).
  - Risk: without the normalized file, `test-report` and any downstream badge generation have no stack-independent data to read.
  - Fix: write the JSON schema from [## Report output](#report-output) alongside the native report on every run.
- Exit `mutation-test` with the underlying mutation tool's own exit code after writing `tmp/result/mutation-test.json`.
  - Risk: a real mutation-testing failure gets swallowed by the normalization step, and CI reports success on a run with unkilled mutants.
  - Fix: propagate the tool's exit code from the script after it finishes writing the normalized result.
- Have `test-report` assemble `public/` per [## Public site output](#public-site-output): per-kind report copies, `*-badge.json` files, and `index.html` copied verbatim from `report-template/index.html`.
  - Risk: without a uniform `public/` shape, every CI publishing step needs stack-specific knowledge of where reports and badges live.
  - Fix: build `public/` exactly as documented, so a publishing step only ever needs to upload `public/` as-is.
- Keep the caller-facing interface to `WITH_CODE_COVERAGE`/`ONLY_DELTA`/`DELTA_BASE` only — never add another caller-facing flag.
  - Risk: every caller now needs stack-specific knowledge to invoke the targets correctly, defeating the point of a uniform contract.
  - Fix: keep anything stack-specific inside the `Makefile` itself, never in the caller-facing interface.

# Check list
- [ ] `unit-test`, `mutation-test`, `test-report`, and `test-and-report` all exist at the repository root.
- [ ] `unit-test` runs Cucumber scenarios and plain tests together, in one invocation.
- [ ] `test-and-report` runs the other three targets in the correct order.
- [ ] No target accepts a flag outside `WITH_CODE_COVERAGE`/`ONLY_DELTA`/`DELTA_BASE`.
- [ ] `tmp/result/unit-test.json`, `tmp/result/coverage-test.json` (when coverage is on), and `tmp/result/mutation-test.json` all follow the schema in [## Report output](#report-output).
- [ ] `tmp/report/tests/`, `tmp/report/coverage/`, and `tmp/report/mutation/` hold each tool's native report.
- [ ] `mutation-test` exits with the underlying tool's own exit code.
- [ ] `test-report` assembles `public/` per [## Public site output](#public-site-output), and `report-template/index.html` exists at the repository root (not under `.github/`).
