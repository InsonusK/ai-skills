---
name: devops-github-wf-pull-request
description: Stack-agnostic GitHub Actions workflow for validating a pull request into develop/master — change detection, a version-bump check on master, and the project's test kinds in a `check` run, all wired through reusable composite actions instead of inline stack logic
whenToUse: when you need to create or update `.github/workflows/pull-request.yml` (or a similarly named file) to validate pull requests into develop/master
updated: 20261006
tags:
  - concern/ci
  - github-actions
  - pr-validation
  - concern/testing
  - stack

---

# Goal
- Create a single `pull_request` workflow that validates every PR into `develop`/`master`: what changed, whether the version was bumped (when required), and unit tests.
- Route every stack-specific decision (which paths count as "code", where the version lives, how it is compared) through reusable composite GitHub Actions, so this skill's job graph is identical regardless of stack.
- Give branch protection one required status check that behaves correctly even when some jobs are skipped by a path filter.

# Core Principle
- This workflow calls two reusable composite actions — `./.github/actions/check-changes` and `./.github/actions/check-version` — never inline `dorny/paths-filter`/version-parsing logic. Their stack-specific mechanics live in a companion skill named `devops-github-action-check-changes-in-{stack}` / `devops-github-action-check-version-in-{stack}`; ask the user which stack to load rather than loading all of them.
- Every PR to `develop` or `master` runs every test kind the project declares, as a `check` run; a PR to `master` that touches code, the workflow, or the Dockerfile must also strictly increase the project's version.
- This workflow never names a test kind: it lists them with `make test-kinds`, runs each as `make test-kind-{kind}` with `TEST_RUN_PURPOSE=check`, and each kind decides what a `check` run means for it. It only ever runs blocking checks — mutation testing is a report-only signal, not a per-PR gate, so this workflow passes no `DELTA_BASE` and the mutation kind skips itself; it runs on the master-push report workflow (see [[skills/devops/workflows/devops-github-wf-release-test-report.skill/devops-github-wf-release-test-report.skill.md|devops-github-wf-release-test-report]]) for a project following [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]].
- The AI agent never pushes directly to `master` or `develop`; it always opens a PR from a separate branch, or does not push at all.

# Workflow
1. `changes` job calls `./.github/actions/check-changes`, producing `code`/`test`/`workflow`/`docker`/`docs` booleans.
2. `version-check` job calls `./.github/actions/check-version`, but only when `github.base_ref == 'master'` and `changes` found `code`, `workflow`, or `docker` — then a follow-up step fails the job when the action's `bumped` output is not `true`, i.e. unless the PR's version is strictly greater than `master`'s.
3. `test-kinds` job — when `code` or `test` changed — reads the kinds from `make test-kinds` and runs `make test-readme-check`; `test-kind` then runs one matrix leg per kind: it sets up the stack's toolchain (`Set up {stack}` — the only step that changes between stacks) and runs `make test-kind-{kind}` with `TEST_RUN_PURPOSE=check`. A cross-platform console app adds an OS dimension to the matrix. A project not yet on solution-conformance-testing runs its native test runner in one plain job instead.
4. `report` aggregate job — `needs: [changes, version-check, test-kinds, test-kind]`, `if: always()` — prints the changed-file summary and fails only if any needed job's `result` is `failure`; `skipped` counts as passing. This is the job branch protection requires, never the individual jobs.

# Rule

## MUST

### Implement from the linked example, not from prose memory
Open and copy [Pull-request workflow example](./templates/pull-request.example.md) before writing `.github/workflows/pull-request.yml` — never reconstruct the YAML from this skill's prose alone. If a real improvement is needed beyond what the example shows, propose it to the user and get it confirmed before shipping it; once confirmed, fold the fix back into the example file.
- Violation: an agent writes the workflow from memory of `# Goal`/`# Core Principle`/`# Rule` without opening the example, and silently drops a mechanical detail the prose only implies, or silently adds its own fix without flagging it.
- Risk: prose is a summary, not a spec — it cannot carry every quoting/escaping/gating detail the working example encodes; an unflagged improvisation might be correct or might be a workaround for a misunderstanding, and nobody reviewing the PR can tell which without asking.
- Fix: read the example first, copy it as the starting point, and treat any deviation as a proposal to confirm with the user — not a silent decision.

### Call the reusable check-changes/check-version actions, never inline logic
Implement change detection and version comparison as `uses: ./.github/actions/check-changes` and `uses: ./.github/actions/check-version`, not as inline `dorny/paths-filter`/parsing steps in this workflow.
- Violation: a `paths-filter` step or a hand-rolled version-parsing script pasted directly into `pull-request.yml`.
- Risk: the same logic is duplicated (and drifts) across every workflow that needs change detection or version comparison (this one, `docker-release-publish`, `stack-lib-release-publish`, `release-info-publish`, `release-test-report`).
- Fix: create `.github/actions/check-changes/action.yml` and `.github/actions/check-version/action.yml` per the matching `devops-github-action-check-changes-in-{stack}`/`devops-github-action-check-version-in-{stack}` skill, and call them from every workflow that needs them.

### Aggregate job wraps every conditional job
Add a final aggregate `report:` job with `needs: [...]` listing every job above (including conditionally-skipped ones) and `if: always()`, that fails only when a listed job's `result` is `failure` — treat `skipped` and `success` as passing. Require this job, not the underlying jobs, in branch protection.
- Violation: branch protection requires a `Test ({kind})` matrix leg directly.
- Risk: when a job is skipped by the path filter (no relevant changes), GitHub never reports success for that job's required check — it stays pending, blocking merge even though nothing needed to run.
- Fix: add the aggregate `report:` job (see [example](./templates/pull-request.example.md)) and require it instead.

### version-check scoped to master, code/workflow/docker changes only
Run `version-check` only when `github.base_ref == 'master'` (or `main`) and `check-changes` reports `code`, `workflow`, or `docker` changed — no broader, no narrower.
- Violation: the job also runs for PRs into `develop`, or is skipped when only the workflow file changed.
- Risk: skipping it lets a master release ship with a stale/duplicate version; widening it forces version bumps for docs-only or test-only PRs.
- Fix: `if: github.base_ref == 'master' && (needs.changes.outputs.code == 'true' || needs.changes.outputs.workflow == 'true' || needs.changes.outputs.docker == 'true')`.

### Fail version-check when the version was not bumped
Follow `./.github/actions/check-version` with a step that exits non-zero when its `bumped` output is not `true`.
- Violation: `version-check` only calls the action, as if the action itself failed on a missing bump.
- Risk: `check-version` only reports `bumped` and always succeeds, so the job is green, `report` passes, and a PR into `master` merges without a version bump.
- Fix: add the `Require a version bump` step from the [example](./templates/pull-request.example.md).

### Set up the stack's toolchain before running tests
Install the project's toolchain version with `actions/setup-{stack}` (the `Set up {stack}` step in the [example](./templates/pull-request.example.md)) before `make test-kind-{kind}`.
- Violation: `test-kind` runs `make test-kind-{kind}` straight after checkout.
- Risk: tests run on whatever version `ubuntu-latest` preinstalls (for Go, not the `go.mod` version) and without a dependency cache.
- Fix: add `actions/setup-{stack}` pinned to the project's version file, with its built-in cache enabled.

### Never gate a PR on mutation testing
Never pass `DELTA_BASE` in this workflow, never name a mutation-testing job in it, and never require one in branch protection.
- Violation: `DELTA_BASE: origin/${{ github.base_ref }}` added to the `test-kind` job, which makes the mutation kind run over the changed code and its exit code part of the aggregate `report:` job.
- Risk: mutation testing is a quality *signal*, not a correctness gate the way unit tests are — blocking merge on it trains the team to treat surviving mutants as a merge obstacle to route around (loosen assertions, mark scenarios `@todo`) rather than a report to act on deliberately; it also slows down every PR with a run whose only consumer is a report nobody reads synchronously.
- Fix: let mutation testing run exclusively on [[skills/devops/workflows/devops-github-wf-release-test-report.skill/devops-github-wf-release-test-report.skill.md|devops-github-wf-release-test-report]]'s unscoped, report-only job after merge.

### Never push directly to a protected branch
Always work in a separate branch and open a PR; if a separate branch cannot be created, do not push the change at all.
- Risk: unvalidated changes enter `develop`/`master` directly, bypassing review and this whole workflow.
- Fix: create a branch, push there, open the PR.

## SHOULD
- Cache dependencies (`actions/setup-{python,node,dotnet}` built-in cache, or `actions/cache`).
- Pin every action to a tag or SHA; never leave an action unpinned to a floating default.
- Use `fail-fast: false` on any OS/version matrix so every combination runs to completion.
- Enable Git long paths on Windows runners that clone repositories or install from Git URLs (`git config --global core.longpaths true`).
- Give every job and step a clear, descriptive name.

## MAY
- Add extra jobs such as linting, type checking, or formatting.
- Publish a coverage summary as a PR comment when the project already collects coverage.

# Example
See [Pull-request workflow example](./templates/pull-request.example.md).

# Check list
- [ ] The workflow was implemented by copying [Pull-request workflow example](./templates/pull-request.example.md), not reconstructed from prose; any deviation was confirmed with the user and folded back into the example.
- [ ] The workflow triggers on `pull_request` to `develop` and `master` (or `main`).
- [ ] `changes` and `version-check` call `./.github/actions/check-changes`/`./.github/actions/check-version` — no inline path-filter or version-parsing logic.
- [ ] `version-check` runs only for PRs to `master` when code, workflow, or Dockerfile changed.
- [ ] `version-check` fails when `bumped != 'true'`.
- [ ] `test-kinds` lists the kinds from `make test-kinds` and runs `make test-readme-check`; `test-kind` sets up the stack's toolchain via `actions/setup-{stack}` and runs one `make test-kind-{kind}` per kind with `TEST_RUN_PURPOSE=check`; no kind is named in the workflow.
- [ ] The workflow passes no `DELTA_BASE`, names no mutation-testing job, and branch protection does not require one.
- [ ] A final aggregate `report:` job is what branch protection requires, not the individual conditional jobs.
- [ ] No direct push to `develop`/`master` — every change went through a branch and a PR.
