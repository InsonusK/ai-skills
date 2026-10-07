# skills/testing — invariants

The anchor document for gathering every testing skill into `skills/testing/` and making testing a self-contained concern (per [[skills/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). Reviewed by the owner 2026-10-07; progress is in `STATUS.md`. Choices are logged in `DECISIONS.md`; names marked *(proposed)* are the agent's and open to change.

**Problem being fixed.** How a service is tested is stated in three places — `skills/common-workflow/test/`, `skills/{stack}/test/`, and inside architecture catalogs (test solutions, plateau `*.Tests` structure skills) — so no one place answers it, and a failing test is traced across all three. DevOps workflows know the test kinds, the `tmp/` layout, and the badge file names.

**Goal.** An agent takes the testing skills (stack-agnostic + stack-specialized) and applies them to any program in that stack, with a stable result.

## 1. What lives where

| Content | Home |
| --- | --- |
| Method: what to test, assertion strength, TDD, test plan | `skills/testing/` |
| Tooling: runner, coverage, mutation, report, `make` targets | `skills/testing/` |
| Layout by convention: where tests sit, derived from the program's own structure (dotnet: a `{Project}.Tests` project beside every production project, referencing what that project may reference) | `skills/testing/` |
| Tests of a pluggable module: a test project/package beside each infrastructure project/package | `skills/testing/` says where and how; the module says how to bring its dependency up |
| Architecture tests that check one catalog's rules; what counts as a unit for a project role | the architecture catalog |
| Test detail specific to one VP (e.g. dotnet domain logic) | with that VP — mechanism not decided, see §6 |

A base plateau names the testing skills it uses; no plateau restates them. Plateau `*.Tests` structure skills are removed once the layout rule covers them.

## 2. Directory layout

```
skills/testing/
  core/        {skill-name}.skill[.md]              every stack-agnostic skill
  {stack}/     {skill-name}-in-{stack}.skill[.md]   every skill of that stack
```

`ai-skill-manager` selects skills by path, so a project lists `skills/testing/core` and `skills/testing/{stack}`. A stack-specific skill always carries `-in-{stack}`, base or not; a base and its extensions are matched by name. `angular/` holds the Angular-framework skills on `stack/typescript`. Rule and ADR: `skill-design` — "Keep testing skills together".

## 3. Isolation

- A file under `skills/testing/` links only to files under `skills/testing/`. A skill outside is named as plain backticked text.
- A stack-specialized skill may link its stack-agnostic base, and nothing in another stack's folder. A stack-agnostic skill never links a stack-specialized one — it says a stack skill must be found for the detail — because `ai-skill-manager` loads every skill a loaded skill links.
- Links into `skills/testing/` from outside are allowed.
- `check.sh` enforces all three. A conflict is not worked around: it is listed in `isolation-exceptions.tsv` and decided with the owner.

## 4. Applying the skills to an existing program

The agent fixes small deviations itself (targets, report, layout, tool wrappers). A large change — replacing the test framework, rewriting existing tests — is done only as a separate task or on the user's direct instruction; until then the deviation is reported. Each skill states its deviations as a checkable list.

## 5. Contract with DevOps

DevOps skills use only this contract and never know which test kinds exist or how they run.

| Part | Contract |
| --- | --- |
| Targets | `make test-kind-{kind}` — one test kind, independent of every other kind, runnable from a clean checkout. `make test-report` — assembles the report from the kinds' results. `make test-kinds` *(proposed)* — prints every kind and the badges it declares, without running anything. `make test-readme-check` *(proposed)* — fails when the README lacks a badge a kind declares, or shows one no kind declares; runs no tests. `make test-and-report` — local convenience: every kind, then the report. |
| Input (ENV) | `TEST_RUN_PURPOSE` *(proposed)* = `pr-check` (the run decides whether a pull request may merge — it must be fast) or `report` (the run builds the full reports and badges for publishing; the default). `DELTA_BASE` — the ref to compare against, when there is one. They state facts about the run; each kind decides what they mean for it, and writes that decision to its log and into the report (e.g. "purpose pr-check: mutating changed files only"). |
| Directories (ENV) | `TEST_WORK_DIR` *(proposed)*, default `tmp/testing` — a kind writes only to `{work}/{kind}/`. `TEST_REPORT_DIR` *(proposed)*, default inside the work dir — where `test-report` writes. The caller chooses both; nothing writes to `public/`. |
| Output | `{report}/index.html` — entry point. `{report}/reports/{name}/` and `{report}/badges/{name}.json` — a kind may produce several; a badge always has a same-named report, a report may have no badge. Names are unique across kinds; `test-report` fails on a duplicate. |
| Skip | A kind that does not apply to the run exits `0` and leaves `{work}/{kind}/skipped` with the reason; `test-report` lists it. No badge, no report. |
| Exit code | Non-zero = this kind's checks failed. Results are written before exiting, so the report exists for a failed run. Whether a failure blocks is DevOps's call. |
| README badges | Added by whoever adds a kind. `test-readme-check` compares the README with the badges `test-kinds` declares and fails naming each missing or stale badge ("badge `{name}` is not in README"). The check is static — a `pr-check` run skips some kinds, so generated badges cannot be the reference. In a `report` run, `test-report` fails when produced badges differ from declared ones. |

CI runs `test-readme-check` and every `test-kind-*` (from `test-kinds`) in parallel, then `test-report`; a new kind is picked up with no workflow change.

## 6. Open

- How a VP attaches its own test detail to the general approach (owner: revisit when we get there).
- What each existing kind does under `pr-check` (today: PR runs unit tests without coverage; mutation runs only for the report).
- Scope and order of the DevOps skill changes; whether plateau codes keep module VPs ([[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/prefer-module-realization|prefer-module-realization]], branch `prefer-module-vp`).
- The isolation conflicts in `isolation-exceptions.tsv` — see `STATUS.md`.
