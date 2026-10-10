---
name: devops-github-wf-pull-request
description: Stack-agnostic GitHub Actions workflow that validates a pull request into develop or master — what changed, a raised version into master, the README badges, every test kind as a parallel check run, a build of what the project delivers — and reports one required status
whenToUse: when you create or update `.github/workflows/pull-request.yml`, or set up branch protection for `develop`/`master`
updated: 20261010
tags:
  - stack
  - concern/ci
  - concern/testing
  - github-actions
  - pr-validation
---

# Goal
- `.github/workflows/pull-request.yml`, a copy of this skill's asset that differs from it only in the test environment of the `test-kind` job.
- The actions and targets the workflow calls present in the project: `check-changes`, `setup-toolchain`, `release`, `make init`, `make version-check`, the testing targets.
- Branch protection of `develop` and `master` requiring the status `Pull request report` and no other job.

# Core Principle
- The workflow follows [[skills/devops/core/devops-ci-orchestration.skill/devops-ci-orchestration.skill.md|devops-ci-orchestration]]: it calls `make` for tests and the version, and holds no stack's command.
- Jobs start by the categories of [[skills/devops/core/devops-ci-changes.skill.md|devops-ci-changes]]; a change of `docs` alone runs only `changes`, `readme-check`, and `report`.
- A pull request is checked, never measured: every test kind runs with `TEST_RUN_PURPOSE=check`.

# Workflow
1. `changes` — `./.github/actions/check-changes`: the files the pull request changes against the commit it branched from.
2. `version-check` — into `master`, when `code` or `docker` changed: `make version-check` against the tip of `master`.
3. `readme-check` — `make test-readme-check`.
4. `test-kinds`, `test-kind` — when `code`, `test`, or `ci` changed: one parallel job per kind `make test-kinds` lists, each `setup-toolchain`, `make init`, `make test-kind-{kind}`.
5. `delivery-check` — when `code` or `docker` changed: `./.github/actions/release` with `channel: check` builds what the project delivers and publishes nothing.
6. `report` — writes the summary and fails when a job failed or was cancelled.

# Rule

## MUST

### Change only the test environment
Copy [[./assets/.github/workflows/pull-request.yml|pull-request.yml]] to `.github/workflows/pull-request.yml` and change nothing in it but the test environment of the `test-kind` job: steps between `make init` and the test step that start what the tests need, and `env` entries of that job, per [[skills/devops/core/devops-ci-orchestration.skill/devops-ci-orchestration.skill.md#Start test services in the test job|devops-ci-orchestration]].
- Violation: a job for the project's stack or delivery typed into the file; a trigger, a job condition, or a `make` call changed.
- Risk: the file stops being the one every project has, and a fix in the skill no longer applies to it.
- Fix: restore the file and re-add the test environment; what differs between projects belongs in the `setup-toolchain` and `release` actions and behind `make`.

### Provide what the workflow calls
Before the first run, apply [[skills/devops/core/devops-ci-changes.skill.md|devops-ci-changes]], [[skills/devops/core/devops-ci-toolchain.skill.md|devops-ci-toolchain]], [[skills/devops/core/devops-project-version.skill/devops-project-version.skill.md|devops-project-version]], and [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]], take the project's release action per [[skills/devops/core/devops-github-wf-release.skill/devops-github-wf-release.skill.md|devops-github-wf-release]], and give the `Makefile` an `init` target.
- Violation: the workflow added to a project without `tools/version/`, without `.github/actions/release`, or without an `init` target.
- Risk: the first pull request fails on a missing action or target.
- Fix: run `make init`, `make -s version`, `make test-kinds`, and `make test-readme-check` locally; add an `init` target that does nothing when a checkout needs no preparation.

### Require only the report job
Set branch protection to require the status `Pull request report`, never a test or version job.
- Violation: `Test (unit)` required in branch protection.
- Risk: a job skipped for a docs-only change never reports, and the pull request waits forever.
- Fix: require `Pull request report`; it passes when the others were skipped and fails when one failed.

### Never gate a pull request on mutation testing
Pass no `DELTA_BASE` to the test kinds.
- Violation: `DELTA_BASE: origin/${{ github.base_ref }}` added to the `test-kind` job.
- Risk: the mutation kind runs over the changed code and its result blocks the merge, which teaches authors to weaken assertions instead of reading the report.
- Fix: add no `DELTA_BASE` to the job's `env`; mutation testing runs after the merge to `master`.

### Raise the version in the pull request into master
Raise the version in the same pull request that changes `code` or `docker` files, when it goes into `master`.
- Violation: the bump planned as a follow-up commit on `master`.
- Risk: `version-check` fails, and a push that gets around it is refused by the release workflow.
- Fix: change the version source and run `make version-check DELTA_BASE=origin/master`.

### Never push directly to develop or master
Open a pull request from a separate branch for every change.
- Risk: a direct push skips every check of this workflow.
- Fix: create a branch, push it, open the pull request.

# Check list
- [ ] `.github/workflows/pull-request.yml` differs from this skill's asset only in the test environment of the `test-kind` job.
- [ ] `check-changes`, `setup-toolchain`, `release`, `tools/version/`, the testing targets, and `make init` exist and run locally.
- [ ] Branch protection requires `Pull request report` only.
- [ ] The `test-kind` job sets `TEST_RUN_PURPOSE: check` and no `DELTA_BASE`.
- [ ] No test kind, tool, or version file is named in the workflow.
