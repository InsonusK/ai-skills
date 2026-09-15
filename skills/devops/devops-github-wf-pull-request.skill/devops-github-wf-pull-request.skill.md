---
name: devops-github-wf-pull-request
description: Stack-agnostic GitHub Actions workflow for validating a pull request into develop/master — change detection, a version-bump check on master, and unit tests, all wired through reusable composite actions instead of inline stack logic
whenToUse: when you need to create or update `.github/workflows/pull-request.yml` (or a similarly named file) to validate pull requests into develop/master
updated: 20260915
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
- Every PR to `develop` or `master` runs unit tests; a PR to `master` that touches code, the workflow, or the Dockerfile must also strictly increase the project's version.
- This workflow only ever runs blocking checks — it never includes mutation testing. Mutation testing is a report-only signal, not a per-PR gate; it only ever runs on the master-push report workflow (see [[skills/devops/devops-github-wf-master-release-report.skill/devops-github-wf-master-release-report.skill.md|devops-github-wf-master-release-report]]) for a project following [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]].
- The AI agent never pushes directly to `master` or `develop`; it always opens a PR from a separate branch, or does not push at all.

# Workflow
1. `changes` job calls `./.github/actions/check-changes`, producing `code`/`test`/`workflow`/`docker`/`docs` booleans.
2. `version-check` job calls `./.github/actions/check-version`, but only when `github.base_ref == 'master'` and `changes` found `code`, `workflow`, or `docker` — it fails unless the PR's version is strictly greater than `master`'s.
3. `unit-test` job runs `make unit-test` (or the stack's native test runner for a project not yet on solution-conformance-testing) when `code` or `test` changed; a cross-platform console app runs it across an OS matrix.
4. `report` aggregate job — `needs: [changes, version-check, unit-test]`, `if: always()` — prints the changed-file summary and fails only if any needed job's `result` is `failure`; `skipped` counts as passing. This is the job branch protection requires, never the individual jobs.

# Rule

## MUST

### Call the reusable check-changes/check-version actions, never inline logic
Implement change detection and version comparison as `uses: ./.github/actions/check-changes` and `uses: ./.github/actions/check-version`, not as inline `dorny/paths-filter`/parsing steps in this workflow.
- Violation: a `paths-filter` step or a hand-rolled version-parsing script pasted directly into `pull-request.yml`.
- Risk: the same logic is duplicated (and drifts) across every workflow that needs change detection or version comparison (this one, `docker-release-publish`, `stack-lib-release-publish`, `release-info-publish`, `master-release-report`).
- Fix: create `.github/actions/check-changes/action.yml` and `.github/actions/check-version/action.yml` per the matching `devops-github-action-check-changes-in-{stack}`/`devops-github-action-check-version-in-{stack}` skill, and call them from every workflow that needs them.

### Aggregate job wraps every conditional job
Add a final aggregate `report:` job with `needs: [...]` listing every job above (including conditionally-skipped ones) and `if: always()`, that fails only when a listed job's `result` is `failure` — treat `skipped` and `success` as passing. Require this job, not the underlying jobs, in branch protection.
- Violation: branch protection requires `unit-test` directly.
- Risk: when a job is skipped by the path filter (no relevant changes), GitHub never reports success for that job's required check — it stays pending, blocking merge even though nothing needed to run.
- Fix: add the aggregate `report:` job (see [example](./templates/pull-request.example.md)) and require it instead.

### version-check scoped to master, code/workflow/docker changes only
Run `version-check` only when `github.base_ref == 'master'` (or `main`) and `check-changes` reports `code`, `workflow`, or `docker` changed — no broader, no narrower.
- Violation: the job also runs for PRs into `develop`, or is skipped when only the workflow file changed.
- Risk: skipping it lets a master release ship with a stale/duplicate version; widening it forces version bumps for docs-only or test-only PRs.
- Fix: `if: github.base_ref == 'master' && (needs.changes.outputs.code == 'true' || needs.changes.outputs.workflow == 'true' || needs.changes.outputs.docker == 'true')`.

### Never gate a PR on mutation testing
Never add a mutation-testing job to this workflow, and never require one in branch protection.
- Violation: a `mutation-test` job with `ONLY_DELTA=true` wired into this workflow's aggregate `report:` job, or required directly in branch protection.
- Risk: mutation testing is a quality *signal*, not a correctness gate the way unit tests are — blocking merge on it trains the team to treat surviving mutants as a merge obstacle to route around (loosen assertions, mark scenarios `@todo`) rather than a report to act on deliberately; it also slows down every PR with a run whose only consumer is a report nobody reads synchronously.
- Fix: let mutation testing run exclusively on [[skills/devops/devops-github-wf-master-release-report.skill/devops-github-wf-master-release-report.skill.md|devops-github-wf-master-release-report]]'s unscoped, report-only job after merge.

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
- [ ] The workflow triggers on `pull_request` to `develop` and `master` (or `main`).
- [ ] `changes` and `version-check` call `./.github/actions/check-changes`/`./.github/actions/check-version` — no inline path-filter or version-parsing logic.
- [ ] `version-check` runs only for PRs to `master` when code, workflow, or Dockerfile changed.
- [ ] `unit-test` runs on code/test changes, with an OS matrix for cross-platform console apps.
- [ ] No mutation-testing job exists in this workflow, and branch protection does not require one.
- [ ] A final aggregate `report:` job is what branch protection requires, not the individual conditional jobs.
- [ ] No direct push to `develop`/`master` — every change went through a branch and a PR.
