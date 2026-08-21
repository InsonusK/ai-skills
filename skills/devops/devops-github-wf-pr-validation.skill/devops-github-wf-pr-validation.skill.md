---
name: devops-github-wf-pr-validation
description: Basic GitHub Actions workflow for PR validation — running unit tests and checking that the application version is bumped when code changes.
whenToUse: when you need to create or update `.github/workflows/pr.yml` (or a similar file) to validate pull requests into develop/master with unit tests and a version-bump check.
tags:
  - concern/ci
  - github-actions
  - pr-validation
  - concern/testing
  - stack

---

# Goal
- Create a minimal, clear workflow that runs unit tests on every pull request to `develop` or `master`.
- Verify that the application version is increased when a PR targets `master` and changes source code or the workflow itself.
- Protect the `develop` and `master` branches from unvalidated changes.

# Core Principle
- Every PR to `develop` or `master` must run unit tests.
- If a PR targets `master` and touches application code or CI, the project metadata version must be strictly greater than the version in the target branch.
- The AI agent **never** pushes directly to `master` or `develop`. Always create a separate branch and open a PR.
- If it is impossible to create a separate branch, the changes must not be pushed at all.
- If the project is a console application that can run on different operating systems, the workflow must test it on multiple OSs.
- A project following [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] needs its Cucumber/mutation/report jobs added to this workflow set — see [[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]] for that CI wiring; this skill only defines the base `test`/`version-check` jobs.

# Rule

## MUST
- Trigger the workflow on `pull_request` to `develop` and `master` (or `main` if that is the project's default branch).
  - Risk: without this trigger, PRs into the protected branch merge without running any validation.
  - Fix: add `on: pull_request: branches: [develop, master]` (or `main`) to the workflow file.
- Provide a `test` job that installs dependencies and runs unit tests.
  - Risk: a PR with a broken test suite can merge undetected.
  - Fix: add a `test` job that installs the project's dependencies and runs its unit test suite.
- Add a final aggregate `test:` job that wraps the (possibly conditional) test job(s), with `needs: [..., test-matrix]` and `if: always()` that inspects `needs.test-matrix.result`, failing only on an actual `failure` and treating `skipped`/`success` as passing. Set this aggregate job — not the underlying matrix job — as the required status check in branch protection.
  - Violation: branch protection requires the matrix/test job directly (e.g. `test-matrix`) instead of the aggregate.
  - Risk: when that job is skipped by a path filter (no `src`/`tests` changes on this PR), GitHub's required status check for it never reports success — the check stays pending or is reported as failing, blocking the PR merge even though nothing needed to be tested.
  - Fix: add the aggregate `test:` job (see [Python PR workflow example](./templates/python-pr.example.md)) and require that job in branch protection. [[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]]'s PR-gate `unit-test`/`mutation-test` jobs feed this same aggregate pattern.
- Provide a `version-check` job that runs only when `github.base_ref == 'master'` (or `main`) and source code (`src/**`, `*.py`, `*.cs`, etc.) or the workflow file (`.github/workflows/pr.yml`) has changed — no broader, no narrower.
  - Violation: either the job is missing or skipped for a qualifying PR (e.g. a CI change lands without `.github/workflows/pr.yml` in its changed-file conditions), or it is widened to also require a bump for PRs that only touch documentation, tests, or non-publishable configuration.
  - Risk: skipping it lets releases ship with duplicate or stale versions, or lets new CI behavior ship untraceable to a version, since reproducing an old release may behave differently once the workflow changed silently; widening it produces unnecessary version bumps, noisy history, and slower development.
  - Fix: scope the job's `if:` condition exactly to "targets `master`/`main`" and "code or workflow changed" — include workflow file changes in the path filter, and nothing broader.
- Use a path filter, e.g. `dorny/paths-filter@v3`, to detect changed files.
  - Risk: without a path filter, every job runs on every PR regardless of what changed, wasting CI time and making conditional jobs (`version-check`, OS matrix) impossible to gate correctly.
  - Fix: add a `changes` job using `dorny/paths-filter@v3` (or equivalent) and gate conditional jobs on its outputs.
- Compare versions semantically: `PR version > base version`. Plain string comparison is not enough.
  - Violation: `"10.0.0"` compared as a plain string is considered less than `"2.0.0"`.
  - Risk: false positives or missed errors in the version-bump check.
  - Fix: use semantic version comparison (`packaging.Version` in Python, `System.Version` in .NET, `semver` in Node.js, etc.).
- Use the correct version file for the stack:
  - Python — `pyproject.toml` (`project.version`),
  - .NET — `*.csproj` / `Directory.Build.props` / `version.json`,
  - Node.js — `package.json` (`version`),
  - for other stacks, ask the user where the version is stored.
  - Risk: reading the wrong file silently compares an unrelated or stale version number, defeating the version-check job entirely.
  - Fix: read the version from the file the project's package manager actually publishes from, confirming with the user for unlisted stacks.
- If the application is a cross-platform console app, use an OS matrix. At minimum include `ubuntu-latest` and `windows-latest`; add `macos-latest` if relevant.
  - Risk: OS-specific bugs are only discovered by end users.
  - Fix: add an OS matrix covering at least `ubuntu-latest` and `windows-latest`.
- Always work in a separate branch and open a PR; never push code or workflow changes directly to `develop`/`master`. If a separate branch cannot be created, do not push the changes at all.
  - Risk: unvalidated changes enter the protected branch, breaking the stable version and bypassing review history.
  - Fix: create a separate branch, let the workflow run, and open a PR — or, if that is not possible, skip pushing entirely.

## SHOULD
- Cache dependencies (`actions/setup-python` with `cache: pip`, `actions/cache` for other ecosystems).
- Pin action versions to tags or SHAs; avoid unpinned `latest` references.
- Give jobs and steps clear, descriptive names.
- Use `fail-fast: false` in the matrix so every OS/version combination runs to completion.
- Enable Git long paths on Windows runners when the workflow clones repositories or installs packages from Git URLs:
  ```yaml
  - name: Enable Git long paths
    run: git config --global core.longpaths true
  ```
  - Risk if skipped: `git clone`/`pip install` from a Git URL fails with errors like `cannot create directory ... Filename too long`, especially for paths containing curly braces or other special characters.
- Do not hardcode secrets, tokens, or credentials in the workflow.
- Do not overload the workflow with build/publish artifact steps — those belong in other workflows.

## MAY
- Add extra jobs such as linting, type checking, or formatting checks.
- Publish a coverage report if the project already collects coverage.

# Example

See [Python PR workflow example](./templates/python-pr.example.md) for a Python project using `pyproject.toml`.

For a project following [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]], see [[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]] for the PR-gate Cucumber/mutation jobs and the master-push report/Pages/badges workflow that build on top of this one.

# Check list
- [ ] Workflow triggers on PR to `develop` and `master` (or `main`).
- [ ] The `test` job runs unit tests and uses an OS matrix for cross-platform console apps.
- [ ] A final aggregate `test:` job wraps the (possibly conditional) test job(s) and is the one required by branch protection, not the underlying matrix job.
- [ ] The `version-check` job runs only for PRs to `master`/`main` when code or the workflow changed — no broader, no narrower.
- [ ] Versions are compared semantically and must strictly increase.
- [ ] The AI agent created a separate branch and PR; no direct push to `develop`/`master`.
- [ ] The workflow contains no secrets, tokens, or deployment steps.
- [ ] For non-Python stacks, the version file and reading method are adapted accordingly.
- [ ] Windows jobs that clone Git repositories or install packages from Git URLs enable `core.longpaths`.
- [ ] For projects following solution-conformance-testing, see [[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]]'s check list for the PR-gate/master-push CI wiring.
