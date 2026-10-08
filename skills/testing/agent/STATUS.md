# Status

Worktree `.ai-worktree/skills-testing`, branch `skills-testing` (base `develop`). Check: `bash skills/testing/agent/check.sh`.

| Wave | Content | State |
| --- | --- | --- |
| W0 | Anchor document, decisions log | done `a4e5d442` |
| W1 | Move: 21 skills from `skills/common-workflow/test/` and `skills/{stack}/test/` into `skills/testing/core/` and `skills/testing/{stack}/` (`moves.tsv`; first grouped by topic, regrouped on the owner's decision); renames `cucmber`→`cucumber`, `no-test-theater-{stack}`→`-in-{stack}`, `dotnet-unittest`→`unittest-in-dotnet`, `testing-strategy`→`testing-strategy-in-dotnet`; every reference in the repository rewritten; 4 links out of `skills/testing/` turned into plain names; `skill-design` rule "Keep testing skills together" + ADR; `validation-config.yaml` exclude pattern; `check.sh` | done |
| W1a | Isolation conflicts closed (owner): `solution-conformance-testing-in-go` no longer depends on the Go catalog's repository-structure solution — it adds its targets to the `Makefile` and creates it when missing; the Python catalog's `solution-test` moved in as `python/solution-test-layout-in-python`. `isolation-exceptions.tsv` is empty | done |
| W1b | Living-doc report in every stack: dotnet (the unit kind copies each project's Cucumber Messages file and renders it; `tools/livingdoc/` added to the three examples) and python (classic Cucumber JSON from `pytest-bdd`) | done, run in W5 |
| W2 | Caller contract (INVARIANTS §5) in `solution-conformance-testing` — `tools/testing/testing.mk` + `testing.sh`, ADR `caller-contract` — and in the go, dotnet, python, typescript stack skills; six Go and three dotnet plateau examples migrated | done, run in W5 |
| W3 | DevOps: `pull-request` and `release-test-report` discover kinds through `make test-kinds` and name none; publish workflows gate on `make test-and-report TEST_RUN_PURPOSE=check` | written; shell steps run locally in W5, the YAML never ran on GitHub |
| W6 | Code delivery per `skill-code-delivery`: every script, tool, `Makefile` and config of the testing skills is a real file under `assets/` or `templates/`, referenced by one line; no fenced code left in the descriptions | done |
| W7 | One Makefile, one report builder, one script per kind (owner): the whole Makefile side is `tools/testing/testing.mk`, the runner `testing.sh`, the report builder `test-report.sh` — shared by every stack, Go included; a stack contributes only `tools/testing/kinds/{kind}.sh`. Go's own report tool and every per-project testing recipe removed | done, run in W5 |
| W8 | Obsolete skills removed (owner, 2026-10-08): `workflow-unittest-testplan`, `unittest-in-dotnet`; `test-driven-development` and `solid-decomposition` work from `.feature` scenarios; `cucumber-testing` states when a plain test is allowed | done |
| W9 | Python: features and tests beside the code (owner, 2026-10-08) — `solution-test-layout-in-python` removed, the layout is a rule of `cucumber-testing-in-python` with its ADR; example, `plateau-python-cli`, `solution-cli-packaging` and the Python check-changes action updated | done; the check-changes YAML not run on GitHub |
| W10 | Kinds own their output (owner, 2026-10-08): each kind script writes `badges/{name}.json` (through `kind.sh`) and its reports, the scenario page included; `test-report.sh` only gathers | done, all eleven examples re-run |
| W11 | Python living doc shows the type tag of every `Examples:` row (owner). A runnable example inside `solution-conformance-testing-in-go` (owner) — which showed the Go mutation kind measuring nothing; fixed with `gremlins --integration`, an uncached first run and a longer timeout | done, the seven Go examples and the Python example re-run |
| W12 | Report page and examples (owner, 2026-10-08): `tests` opens the living doc and its separate line is gone; the scenarios page is one table with a tags column (`scenarios.json` gained `tags`); the Go, Python and TypeScript examples carry three features each | done, all twelve examples re-run |
| W13 | Feature category tags (owner, 2026-10-08): one tag on every `Feature:` line saying what it tests (renamed `@type/…` in W14) — rule in `cucumber-testing`, `category` in `scenarios.json`, a column and a summary on the scenarios page; the unit kind fails on a scenario without a type tag or a feature without a category tag; 31 example features tagged | done, all twelve examples re-run |
| W14 | Namespaced tags (owner, 2026-10-09): `@type/…` on a feature, `@category/…` on a scenario, optional `@status/todo` / `@status/broken` / `@status/validated`; the report says `none` for a missing tag and `not-run` for a scenario no runner executed; a status legend on the scenarios page and in the living doc; 31 features retagged, the filters of the four runners switched | done, all twelve examples re-run |
| W15 | Living doc (owner, 2026-10-09): the `[object Object]` header fixed — the status legend is the page footer now; the scenarios a classic-JSON runner did not execute (`todo`, `broken`, `not-run`) are added to the living doc; the Python example shows every type, category and status tag | done, all twelve examples re-run |
| W5 | Run every example, fix what breaks; Python on `pytest-bdd`; runnable Python and TypeScript examples | done — see below |
| W4 | Layout by convention and tests of pluggable modules as testing skills; plateau `*.Tests` structure skills removed; base plateaus name the testing skills | not started — needs the owner, see below |

## W5 — what ran (2026-10-07)

Container: Go 1.26.8, .NET SDK 10.0.401, Node 24.21, Python 3.13.16; no Docker. Each example went through `bash skills/testing/agent/run-example.sh {example}` — a `report` run, a `check` run with caller-chosen directories, delta mutation — plus one deliberately broken scenario per stack. `check.sh` passes.

| Example | `make test-and-report` | tests | coverage | mutation score |
| --- | --- | --- | --- | --- |
| go `solution-conformance-testing-in-go` (new, W11; three features since W12) | exit 0 | 10/10 | 96.3% | 100% |
| go `plateau-http-service` | exit 0 | 5/5 | 10.4% | 25.9% |
| go `plateau-cached-service` | exit 0 | 11/11 | 12.0% | 24.4% |
| go `plateau-dual-api-service` | exit 0 | 5/5 | 8.5% | 23.3% |
| go `plateau-integrated-service` | exit 0 | 9/9 | 9.4% | 22.9% |
| go `plateau-persistent-service` | exit 0 | 13/13 | 10.5% | 20% |
| go `gw009-001` (PostgreSQL 18 from unpacked binaries, `TEST_DATABASE_DSN`) | exit 0 | 48/48 | 42.7% | 56% |
| dotnet `plateau-core` | exit 0 | 7/7 | 72.4% | 55.0% |
| dotnet `plateau-domain-service` | exit 0 | 10/10 | 41.7% | 31.1% |
| dotnet `plateau-offline-sync-service` | exit 0 | 14/14 | 40.1% | 29.3% |
| python `solution-conformance-testing-in-python` (new; eight features showing every tag since W15) | exit 0 | 26/26 | 99.2% | 87.4% |
| typescript `solution-conformance-testing-in-typescript` (new; three features since W12) | exit 0 | 10/10 | 100% | 89.4% |

In every one: the report holds `index.html`, `run.json`, three badges, `reports/{tests,coverage,mutation,scenarios}/` and the living doc, and every link of the landing page resolves; the `check` run writes nothing to `tmp/` or `public/`, skips mutation, and produces the `tests` badge only. The Go mutation scores are those of W11 (2026-10-08), after the mutation kind got `gremlins --integration`: no example has a surviving mutant; what keeps the plateau examples low is `noCoverage` — `main`, config and server code no scenario reaches. Before W11 the scores were 2–11% and meant nothing: gremlins ran only the mutated package's own tests.

Broken scenario: `make test-kind-unit` exits non-zero, `result/scenarios.json` is written, `make test-report` builds the report with exit 0 and shows the kind as `failed` — after the fix below.

The landing page was not opened in a browser (the container has none): its links were checked file by file as a static host resolves them, and its script was run under Node against `run.json`.

`TASK.md` items:

| # | Result |
| --- | --- |
| 1 | The three Go normalizers build and pass `go vet`; the shared `test-report.sh` works on Go results. |
| 2 | Go `unit.sh` works as written. |
| 3 | Delta mutation also works in a module below the repository root (git's `diff.relative`, 2026-10-08). `gremlins` v0.6.0 exits `0` when mutants survive and `10` under a configured threshold — the kind now forces both thresholds to `0` in a `report` run. `mutmut` 3.8 exits `0` on survivors and has no threshold. |
| 4 | dotnet `unit.sh` died before its first line of output (`ls *.slnx *.sln` under `pipefail`) — fixed. The Cucumber Messages living doc renders. |
| 5 | dotnet `mutation.sh`: the report link is right. `--since` rejected `HEAD~1` and, once given a commit id, ignored every mutant of a changed file — replaced by `--mutate` patterns. |
| 6 | Both stacks now have a runnable example inside their skill. TypeScript: three faults fixed in the kind scripts; Vitest, which the unit kind never ran, removed from the skill on 2026-10-08. |
| 7 | Read; the shell of every step run locally — the kind list, each kind on its own clean clone, the artifact hand-over, `make test-report` on a third clone with no Go on `PATH`. The YAML itself has not run on GitHub. |
| 8 | Run with PostgreSQL: 48/48, as the plateau skill records. Without `TEST_DATABASE_DSN` it is red by design. |
| 9 | Done: `pytest-bdd`, one `pytest` run, JUnit counts, `mutmut` 3.8, ADR, example. |

What was broken and is fixed — the commit messages carry the detail:
- A red test meant no published report: the mutation kind stops without a result, and `test-report` exited 1 over its missing badge. `run.json` now has the state `failed`.
- `reports/tests/` had no entry page in Go and .NET (404 on a static host).
- The dotnet unit kind did not start; dotnet and TypeScript delta mutation tested nothing; Python mutation was a placeholder.

## Waiting on the owner

- ⚠️ **Mutation testing in a pull request.** The mutation kind runs in a `check` run only when `DELTA_BASE` is given, over the changed code. The pull-request workflow passes none, so it skips itself there — today's policy ("never gate a PR on mutation testing") is unchanged. Adding `DELTA_BASE: origin/${{ github.base_ref }}` to that workflow turns delta mutation into part of the merge gate.
- **Consumers' `ai-skills.yaml`** that list `skills/common-workflow/test` or `skills/{stack}/test` must switch to `skills/testing/core` and `skills/testing/{stack}`.
- **TypeScript keeps features in a root `features/` tree** with `features/step-definitions/`, while Go and Python keep them beside the code. `solution-conformance-testing-in-typescript` and `cucumber-testing-in-typescript` were not moved to the co-located layout; nobody asked yet.
- **Feature templates of the architecture catalogs** (e.g. the dotnet catalog's test solutions) were not retagged: a project generated from them gets features without `@type/…`, and its unit kind fails until they are tagged. Belongs to W4.
- **The TypeScript living doc has no status legend**: it is cucumber-js's own page. The legend is on the scenarios page there.
- **Go scenario report is per outline, not per `Examples:` block.** `normalize_scenarios` matches results by test name, and godog names every row of an outline alike — one failed row marks every block of that outline `failed`. The other three stacks match by row line. godog's Cucumber JSON carries each row's own line (checked), so the fix is to join on it and keep the name match for a runner that writes no Cucumber JSON. Shown to the owner; not started.
- **Angular on the `make` contract** (owner, 2026-10-08): component tests and pixel tests as test kinds of their own beside the scenario-based `unit` kind — a `solution-conformance-testing-in-angular` with one kind script and one badge name per kind. Not started; the Angular catalogs do not use the contract yet. Since W10 a new kind needs no change to the shared report builder.
- **`taskbox-conformance.feature` is retagged in the `gw009-001` copy only** (`@type/infrastructure`, `@category/…`, `@status/todo`): the file is a verbatim copy from the `taskbox-go` library. The same tags have to be applied at the source — the next verbatim sync would drop it, and the unit kind would then fail.
- **Whether the scenarios page stays** now that the living doc lists every scenario (W15): it still holds the two summary tables and the `validated` column, is the same in every stack and needs no Node. The owner asked why it is still there.
- **`gw009-001`'s TaskBox runner** (the pre-release library copy) writes no Cucumber JSON, so its 30 scenarios are in the scenario report but not in the living doc.
- **Stryker.NET** logs `test coverage capture failed` twice in a `plateau-core` run and disables its coverage-based test selection for those projects. The score still matches the xUnit v2 ADR (55%), so nothing was changed.
- **A pull request for this branch** is not opened: W4 is not started and the points above are open.
- **The Python check-changes filter** names no path of `tools/testing/`, so a change to a kind script alone starts no test job. It was so before; noted while rewriting the filter.
- **W4** (done for Python in W9) needs decisions before it can start: what happens to the dotnet catalog's `solution-dotnet-conformance-testing` (test-project layout) and the plateau `*.Tests` structure skills once the layout rule moves into `skills/testing/dotnet/`; and how a VP attaches its own test detail (deferred by the owner).

## Not changed

- `.validation/*-log.yaml`: `validation_queue.py queue --dry-run` registered the moved skills under their new paths and pruned the old ones. `no-test-theater-in-angular` lost its date (20260909) and is due again; the rest were never validated.
- ADRs and catalog `agent/DECISIONS.md` journals keep the old directory names in prose where they describe a past decision.
- Catalog testing solutions stay where they are until W4: dotnet `solution-dotnet-conformance-testing`, `solution-cecil-architecture-tests`; angular `solution-app-testing`, `solution-ui-testing`, `solution-design-system-ui-testing`.
- 140 broken links elsewhere in the repository existed before this branch and are untouched.
