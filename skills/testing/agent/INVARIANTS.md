# skills/testing — invariants

The anchor document for gathering every testing skill into `skills/testing/` and making testing a self-contained concern (per [[skills/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). Draft for the owner's review — nothing is migrated yet. Choices are logged in `DECISIONS.md`; names marked *(proposed)* are the agent's and open to change.

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
  {topic}/
    {topic}.skill/              stack-agnostic
    {topic}-in-{stack}.skill/   one per stack that has it
```

A topic with only one stack has only its `-in-{stack}` skill; a topic with no stack variants has only the agnostic skill. Tags are unchanged — skillsets still resolve by `stack/{x} & concern/testing`.

## 3. Isolation

No link from `skills/testing/` leads into an architecture catalog — a plateau, a catalog solution, a Variability Map, a Feature Model. Links to design-method skills (`skill-design`, `solution-create`, `adr-create`) are allowed. `check.sh` enforces this, plus link resolution.

## 4. Applying the skills to an existing program

The agent fixes small deviations itself (targets, report, layout, tool wrappers). A large change — replacing the test framework, rewriting existing tests — is done only as a separate task or on the user's direct instruction; until then the deviation is reported. Each skill states its deviations as a checkable list.

## 5. Contract with DevOps

DevOps skills use only this contract and never know which test kinds exist or how they run.

| Part | Contract |
| --- | --- |
| Targets | `make test-kind-{kind}` — one test kind, independent of every other kind, runnable from a clean checkout. `make test-report` — assembles the report from the kinds' results. `make test-kinds` *(proposed)* — prints every kind and the badges it declares, without running anything. `make test-setup-check` *(proposed)* — static checks of the testing setup. `make test-and-report` — local convenience: every kind, then the report. |
| Input (ENV) | `TEST_RUN_PURPOSE` *(proposed)* = `gate` (check before merge) or `report` (full report; the default). `DELTA_BASE` — the ref to compare against, when there is one. They state facts about the run; each kind decides what they mean for it, and writes that decision to its log and into the report (e.g. "purpose gate: mutating changed files only"). |
| Directories (ENV) | `TEST_WORK_DIR` *(proposed)*, default `tmp/testing` — a kind writes only to `{work}/{kind}/`. `TEST_REPORT_DIR` *(proposed)*, default inside the work dir — where `test-report` writes. The caller chooses both; nothing writes to `public/`. |
| Output | `{report}/index.html` — entry point. `{report}/reports/{name}/` and `{report}/badges/{name}.json` — a kind may produce several; a badge always has a same-named report, a report may have no badge. Names are unique across kinds; `test-report` fails on a duplicate. |
| Skip | A kind that does not apply to the run exits `0` and leaves `{work}/{kind}/skipped` with the reason; `test-report` lists it. No badge, no report. |
| Exit code | Non-zero = this kind's checks failed. Results are written before exiting, so the report exists for a failed run. Whether a failure blocks is DevOps's call. |
| README badges | Added by whoever adds a kind. `test-setup-check` compares the README with the badges `test-kinds` declares and fails naming each missing or stale badge ("badge `{name}` is not in README"). The check is static — a `gate` run skips some kinds, so generated badges cannot be the reference. In a `report` run, `test-report` fails when produced badges differ from declared ones. |

CI runs `test-setup-check` and every `test-kind-*` (from `test-kinds`) in parallel, then `test-report`; a new kind is picked up with no workflow change.

## 6. Open

- How a VP attaches its own test detail to the general approach (owner: revisit when we get there).
- What each existing kind does under `gate` (today: PR runs unit tests without coverage; mutation runs only for the report).
- Scope and order of the DevOps skill changes; whether plateau codes keep module VPs ([[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/prefer-module-realization|prefer-module-realization]], branch `prefer-module-vp`).
- Inventory of skills to move, wave plan, `check.sh` — written after this document is reviewed.
