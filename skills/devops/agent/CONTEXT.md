# skills/devops — context for the agent who starts work here

Written on 2026-10-09 by the agent who finished `skills/testing`. It holds what that work changed in the DevOps skills, what it left for you, and what was decided. It does not hold your task: the owner sets that.

## Where the work is

Branch `skills-testing` (worktree `.ai-worktree/skills-testing`), 50-odd commits on top of `develop`, local, not pushed, no pull request. **It already changed eight files of `skills/devops/workflows/` and the Python change filter.** Start from that branch, or after it is merged into `develop` — a branch cut from today's `develop` edits the previous versions of those files.

Project rules are in `AGENTS.md` at the repository root: a worktree of your own, `skills/` as the only source, the bulk-authoring harness for a wide change, and say so when a decision looks wrong.

## The contract between testing and DevOps

`skills/testing/agent/INVARIANTS.md` §5 is the reviewed text; read it before anything else. In short, a workflow knows only this:

| | |
| --- | --- |
| Targets | `make test-kinds` (lists the kinds, runs nothing), `make test-kind-{kind}`, `make test-report`, `make test-readme-check`, `make test-and-report` |
| Input | `TEST_RUN_PURPOSE` = `check` or `report`; `DELTA_BASE` when there is a ref to compare with; `TEST_WORK_DIR`, `TEST_REPORT_DIR` |
| Output | `{report}/index.html`, `{report}/run.json`, `{report}/badges/{name}.json`, `{report}/reports/{name}/` |
| Exit code | non-zero = this kind's checks failed; its results are written first, so the report exists for a failed run |

A workflow names no test kind and no tool. A new kind — Angular added `components` and `ui` — is picked up with no workflow change. The contract was fixed with the owner; adding a target or a caller-facing variable is a change to raise with the owner, not to make.

Fourteen runnable examples implement it: `skills/testing/{go,python,typescript,dotnet,angular}/solution-conformance-testing-in-*.skill/example` and the nine plateau examples of the Go and .NET catalogs. `bash skills/testing/agent/run-example.sh {example-dir}` runs one end to end; `bash skills/testing/agent/check.sh` checks the copies.

## What `skills-testing` already changed here

- `devops-github-wf-pull-request` and `devops-github-wf-release-test-report`: the kinds are discovered through `make test-kinds` and run as a matrix; no kind is named.
- `devops-github-wf-docker-release-publish` and `devops-github-wf-stack-lib-release-publish`: publishing is gated on `make test-and-report TEST_RUN_PURPOSE=check`.
- `skills/python/devops/devops-github-action-check-changes-in-python`: the filter follows the Python layout — features and tests beside the code.

## Known gaps — found, not fixed

1. **The workflows never ran on GitHub.** The shell of every step was run locally, on clean clones; the YAML itself was not. Treat every workflow template as unproved until one run is green.
2. **The Go and TypeScript change filters describe a layout that no longer exists.** `devops-github-action-check-changes-in-go` and `-in-typescript` still list a root `features/**`; the TypeScript one also speaks of co-located Vitest specs, and the TypeScript testing skill has no Vitest. Today: `{package}/features/*.feature` beside the code in both; Go tests in `{package}/test/*_test.go`; TypeScript steps in `src/{package}/test/*.steps.ts`. The Python filter shows the accepted shape.
3. **No change filter lists `tools/testing/**`.** A change to a kind script alone starts no test job. True for all four stacks.
4. **Angular has no DevOps skill.** No change filter, no version check. Its example's `make init` installs Chromium with its system libraries (`playwright install --with-deps chromium`), which needs root or a prepared image; native specs are `src/**/spec/*.spec.ts`, Cucumber steps `src/**/test/*.steps.ts`.
5. **`make init` is not part of the contract.** Every example that needs preparing has the target, and the workflows prepare the toolchain with `actions/setup-{stack}` instead. Whether a workflow should call `make init` — which would keep the knowledge of a stack's preparation out of the workflow — is not decided.
6. **Services a test needs.** The `gw009-001` example needs a PostgreSQL through `TEST_DATABASE_DSN`. No workflow provides a service to a test kind.
7. **Run time.** A `report` run of `gw009-001` takes about eleven minutes, most of it mutation testing; a kind runs in its own matrix leg.

## Decisions that stand

- **A pull request is never gated on mutation testing.** The pull-request workflow passes no `DELTA_BASE`, so the mutation kind skips itself in a `check` run. Passing `DELTA_BASE: origin/${{ github.base_ref }}` would make delta mutation part of the merge gate — the kinds support it and it was measured; the owner has not asked for it.
- **Whether a failed kind blocks is the workflow's call**, not the kind's: a kind only reports through its exit code.
- **The report is published as it is built**: a workflow copies `TEST_REPORT_DIR`, it computes nothing and renders nothing.
- **Consumers** of these skills are the owner alone for now; no versioning of the skill paths yet.

## Still open on the testing side

- Angular in an Nx workspace: the testing skill is delivered for one Angular CLI application, the Angular catalog builds on Nx. Under discussion; expect a second set of kinds or an adapter.
- The details are in `skills/testing/agent/STATUS.md` ("Waiting on the owner") and `DECISIONS.md`.

## The environment the testing work ran in

Go 1.26, .NET SDK 10, Node 24, Python 3.13; no Docker, no browser besides the Chromium Playwright installs. PostgreSQL came from unpacked binaries — the recipe is in the earlier revisions of `skills/testing/agent/TASK.md`. A full run of all fourteen examples needs several gigabytes of free disk: the Go build cache alone grew past eight.
