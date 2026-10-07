# Status

Worktree `.ai-worktree/skills-testing`, branch `skills-testing` (base `develop`). Check: `bash skills/testing/agent/check.sh`.

| Wave | Content | State |
| --- | --- | --- |
| W0 | Anchor document, decisions log | done `a4e5d442` |
| W1 | Move: 21 skills from `skills/common-workflow/test/` and `skills/{stack}/test/` into `skills/testing/core/` and `skills/testing/{stack}/` (`moves.tsv`; first grouped by topic, regrouped on the owner's decision); renames `cucmber`→`cucumber`, `no-test-theater-{stack}`→`-in-{stack}`, `dotnet-unittest`→`unittest-in-dotnet`, `testing-strategy`→`testing-strategy-in-dotnet`; every reference in the repository rewritten; 4 links out of `skills/testing/` turned into plain names; `skill-design` rule "Keep testing skills together" + ADR; `validation-config.yaml` exclude pattern; `check.sh` | done |
| W1a | Isolation conflicts closed (owner): `solution-conformance-testing-in-go` no longer depends on the Go catalog's repository-structure solution — it adds its targets to the `Makefile` and creates it when missing; the Python catalog's `solution-test` moved in as `python/solution-test-layout-in-python`. `isolation-exceptions.tsv` is empty | done |
| W1b | Living-doc report in every stack: dotnet (`unit-test.sh` copies each project's Cucumber Messages file and renders it; `tools/livingdoc/` added to the three examples) and python (behave stays, classic Cucumber JSON through `behave-cucumber-formatter`) | written, not run |
| W2 | Caller contract (INVARIANTS §5) in `solution-conformance-testing` — `tools/testing/testing.mk` + `testing.sh`, ADR `caller-contract` — and in the go, dotnet, python, typescript stack skills; six Go and three dotnet plateau examples migrated | written; shared half tested here, toolchain-dependent half not run |
| W3 | DevOps: `pull-request` and `release-test-report` discover kinds through `make test-kinds` and name none; publish workflows gate on `make test-and-report TEST_RUN_PURPOSE=check` | written, not run |
| W6 | Code delivery per `skill-code-delivery`: every script, tool, `Makefile` and config of the testing skills is a real file under `assets/` or `templates/`, referenced by one line; no fenced code left in the descriptions | done |
| W5 | Run every example and the workflows — `TASK.md` | **next, needs the rebuilt container** |
| W4 | Layout by convention and tests of pluggable modules as testing skills; plateau `*.Tests` structure skills removed; base plateaus name the testing skills | not started — needs the owner, see below |

## Living-doc report: not in every stack

The base requires every stack's `unit-test` to write the runner's standard Cucumber report to `tmp/report/tests/cucumber/` and render `tmp/report/tests/livingdoc/` with the shared `tools/livingdoc/`. It covers the test (scenario) report only — coverage and mutation keep each tool's own report.

| Stack | Protocol named in `cucumber-testing-in-{stack}` | Render step in `solution-conformance-testing-in-{stack}` | Examples |
| --- | --- | --- | --- |
| Go | classic Cucumber JSON | yes | all 6 Go plateau examples |
| TypeScript | Cucumber Messages | yes | none exist |
| dotnet | Cucumber Messages | **no** | 3 dotnet plateau examples lack it |
| Python | classic Cucumber JSON | **no** — and the skill runs `behave`, whose JSON `cucumber-testing-in-python` says is not classic; an HTML formatter is still an open `TODO` there | none exist |

## Not run

The container had no Go, .NET, Node, Python or Docker. `.devcontainer/devcontainer.json` on this branch adds them; after a rebuild `TASK.md` lists exactly what to run and where the risk is.

## Waiting on the owner

- ⚠️ **Mutation testing in a pull request.** The mutation kind runs in a `check` run only when `DELTA_BASE` is given, over the changed code. The pull-request workflow passes none, so it skips itself there — today's policy ("never gate a PR on mutation testing") is unchanged. Adding `DELTA_BASE: origin/${{ github.base_ref }}` to that workflow turns delta mutation into part of the merge gate.
- **Consumers' `ai-skills.yaml`** that list `skills/common-workflow/test` or `skills/{stack}/test` must switch to `skills/testing/core` and `skills/testing/{stack}`.
- **W4** needs decisions before it can start: what happens to the dotnet catalog's `solution-dotnet-conformance-testing` (test-project layout) and the plateau `*.Tests` structure skills once the layout rule moves into `skills/testing/dotnet/`; and how a VP attaches its own test detail (deferred by the owner).

## Not changed

- `.validation/*-log.yaml` still key the moved skills by their old paths — the files are written only by `validation_queue.py`, which cannot run here. Re-run it; the one real date lost is `no-test-theater-in-angular` (20260909), the rest were never validated.
- ADRs and catalog `agent/DECISIONS.md` journals keep the old directory names in prose where they describe a past decision.
- Catalog testing solutions stay where they are until W4: dotnet `solution-dotnet-conformance-testing`, `solution-cecil-architecture-tests`; angular `solution-app-testing`, `solution-ui-testing`, `solution-design-system-ui-testing`.
- 140 broken links elsewhere in the repository existed before this branch and are untouched.
