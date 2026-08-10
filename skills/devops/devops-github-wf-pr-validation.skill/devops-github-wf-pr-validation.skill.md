---
name: devops-github-wf-pr-validation
description: Basic GitHub Actions workflow for PR validation — running unit tests and checking that the application version is bumped when code changes — plus, for projects that follow bdd-coverage-mutation-testing, the PR-gate Cucumber/mutation jobs and the master-push workflow that publishes coverage/mutation reports and badges to GitHub Pages.
whenToUse: when you need to create or update `.github/workflows/pr.yml` (or a similar file) to validate pull requests into develop/master, or when a project following bdd-coverage-mutation-testing needs its `make` targets wired into a PR-gate workflow and a master-push report-publishing/GitHub-Pages/badges workflow.
tags:
  - devops
  - github-actions
  - ci
  - pr-validation
  - testing
  - bdd
  - cucumber
  - mutation-testing
  - github-pages
  - badges
---

# Goal
- Create a minimal, clear workflow that runs unit tests on every pull request to `develop` or `master`.
- Verify that the application version is increased when a PR targets `master` and changes source code or the workflow itself.
- Protect the `develop` and `master` branches from unvalidated changes.
- When the project follows [[skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md|bdd-coverage-mutation-testing]], run its Cucumber conformance suite and delta-scoped mutation testing on every PR to `master`, and publish full coverage/mutation reports plus README badges via GitHub Pages on every push to `master`.

# Core Principle
- Every PR to `develop` or `master` must run unit tests.
- If a PR targets `master` and touches application code or CI, the project metadata version must be strictly greater than the version in the target branch.
- The AI agent **never** pushes directly to `master` or `develop`. Always create a separate branch and open a PR.
- If it is impossible to create a separate branch, the changes must not be pushed at all.
- If the project is a console application that can run on different operating systems, the workflow must test it on multiple OSs.
- When the project follows [[skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md|bdd-coverage-mutation-testing]], this workflow only ever calls that project's `make cucumber-test`/`make mutation-test`/`make result-page` targets — it never invokes a stack's native test/coverage/mutation CLI directly, and it never re-decides what those targets do.
- Two separate workflows, not one, own the BDD gate: a **PR-gate** workflow (`pull_request` → `master`) that blocks merge, and a **master-push** workflow (`push` → `master`) that only publishes reports/badges and never blocks anything. See [# Report publishing](#report-publishing-running-tests-coverage-and-mutation-testing-in-ci).
- Coverage is collected only on the master-push workflow, never on the PR-gate workflow — coverage is a trend/floor check, not per-PR-critical feedback, and skipping its instrumentation overhead on PRs keeps that gate fast for what does need per-PR feedback: mutation testing scoped to the exact lines the PR changed.
- A master-push run reports on one specific commit. If a second push to `master` lands before the first run finishes, the first run's eventual report would describe code that is no longer on `master` — so the superseded run must be cancelled, not left to finish and publish stale numbers.
- Report publishing is not free CI time, so it must not run on every push to `master` — only when the push actually changed something the reports could reflect (source code, tests/specs, or the CI/report tooling itself).

# Rule

## MUST
- Trigger the workflow on `pull_request` to `develop` and `master` (or `main` if that is the project's default branch).
- Provide a `test` job that installs dependencies and runs unit tests.
- Provide a `version-check` job that runs only when:
  - `github.base_ref == 'master'` (or `main`), and
  - source code (`src/**`, `*.py`, `*.cs`, etc.) or the workflow file (`.github/workflows/pr.yml`) has changed.
- Use a path filter, e.g. `dorny/paths-filter@v3`, to detect changed files.
- Compare versions semantically: `PR version > base version`. Plain string comparison is not enough.
- Use the correct version file for the stack:
  - Python — `pyproject.toml` (`project.version`),
  - .NET — `*.csproj` / `Directory.Build.props` / `version.json`,
  - Node.js — `package.json` (`version`),
  - for other stacks, ask the user where the version is stored.
- If the application is a cross-platform console app, use an OS matrix. At minimum include `ubuntu-latest` and `windows-latest`; add `macos-latest` if relevant.
- The AI agent must work in a separate branch and create a PR. Direct push to `develop`/`master` is forbidden. If a separate branch cannot be created, do not push the changes.

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
  This prevents checkout failures for paths longer than 260 characters or paths containing special characters such as curly braces.

## MAY
- Add extra jobs such as linting, type checking, or formatting checks.
- Publish a coverage report if the project already collects coverage.

## SHOULD NOT
- Hardcode secrets, tokens, or credentials in the workflow.
- Overload the workflow with build/publish artifact steps — those belong in other workflows.

## MUST NOT
- Push workflow or code directly to `develop` or `master` without a PR.
- Skip the version check for a `master` PR that changes code or the workflow.
- Require a version bump for PRs that only change documentation, tests, or non-publishable configuration.

# Report publishing (running tests, coverage, and mutation testing in CI)
Every project running tests, coverage, and mutation testing in CI needs the job structure below — the PR-gate/master-push split, the concurrency cancellation, the path filtering, the Pages/badge publishing. What differs is only *how* each job invokes the tooling:
- If the project has adopted [[skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md|bdd-coverage-mutation-testing]], every job below calls its standardized `make cucumber-test`/`make mutation-test`/`make result-page` contract — chosen specifically so a developer can run the exact same command locally to reproduce a CI failure, instead of having to reverse-engineer what the workflow file does.
- If it has not, implement the equivalent invocation directly in the workflow (call the stack's test runner, coverage tool, and mutation-testing tool natively) — the job boundaries, triggers, concurrency, and path-filter rules below still apply; only the `run:` step's command changes.

## PR-gate workflow (`pull_request` → `master`)
- A `cucumber-test` job runs `make cucumber-test` (no coverage).
- A `mutation-test` job runs `make mutation-test ONLY_DELTA=true DELTA_BASE=<PR base ref/SHA>`, scoping mutation testing to code the PR actually changed. Give it full git history (`fetch-depth: 0`) so the delta diff has a base to compare against.
- Both jobs feed the same aggregate-job pattern already required in [# Anti-patterns](#anti-patterns) for the plain `test` job — the aggregate job is what branch protection requires, not `cucumber-test`/`mutation-test` directly.
- Coverage is not collected here — see [# Core Principle](#core-principle) for why.

## Master-push workflow (`push` → `master`, plus `workflow_dispatch`)
A second, separate workflow file (e.g. `.github/workflows/report-publish.yml`) that only reports and publishes — it never fails the push itself:
- `permissions: contents: read, pages: write, id-token: write`.
- `concurrency: { group: <workflow-name>-<ref>, cancel-in-progress: true }` at the workflow level (e.g. `group: ${{ github.workflow }}-${{ github.ref }}`) — a push that lands while the previous master-push run is still going cancels that run outright instead of letting it finish and publish a report for a commit `master` has already moved past. This trades a small chance of interrupting an in-flight `deploy` step (GitHub's official Pages template avoids that by never cancelling) for never publishing stale numbers; the deploy step is short, and the next successful run republishes cleanly, so the trade-off favors cancellation here.
- A `changes` job (path filter, e.g. `dorny/paths-filter@v3`) runs first and gates every job below on whether the push touched source code, tests/specs (`.feature`/step-definitions/`test/`), or CI/report tooling (`.github/workflows/**`, `Makefile`, `scripts/**`) — a push that only touches docs or unrelated files must not trigger a report run at all.
- A `cucumber-test` job (`if:` the `changes` job found a relevant change) runs `make cucumber-test WITH_CODE_COVERAGE=true` and uploads `tmp/result` plus the native report directories it produced (e.g. `tmp/report/tests`, `tmp/report/coverage`) as a build artifact.
- A `mutation-test` job (same `if:`) runs `make mutation-test` (no `ONLY_DELTA` — the full project) and uploads `tmp/result` plus `tmp/report/mutation` as a build artifact. This job's mutation score does not gate anything — the PR-gate workflow already enforced the threshold before this code reached `master`.
- A `result-page` job depends on both, downloads their artifacts back into `tmp/`, runs `make result-page` to assemble `public/`, and uploads `public/` as a Pages artifact (`actions/upload-pages-artifact@v3`). It cascade-skips automatically when `cucumber-test`/`mutation-test` were skipped by the path filter.
- A `deploy` job deploys `public/` to GitHub Pages (`actions/deploy-pages@v4`); it cascade-skips the same way.

## Badges
- `make result-page` writes one `<label>-badge.json` file per metric into `public/` (shields.io's [endpoint badge](https://shields.io/badges/endpoint-badge) schema: `{"schemaVersion":1,"label":"...","message":"...","color":"..."}`), alongside the native reports it copies in.
- README badges reference those files as shields.io endpoint badges: `https://img.shields.io/endpoint?url=<pages-url>/<label>-badge.json` — never a hand-authored static badge.
- Each badge links to its full native report on the same Pages site, not just to the raw JSON.
- README also keeps the native GitHub Actions status badge for the PR-gate workflow itself (`.../actions/workflows/<pr-workflow-file>/badge.svg`), alongside the shields.io endpoint badges.

## Rule

### MUST
- Run `make cucumber-test` and `make mutation-test ONLY_DELTA=true DELTA_BASE=<PR base>` as required jobs of the PR-gate workflow, feeding the same aggregate-job pattern used for plain unit tests.
- Add a separate workflow triggered on `push` to `master` (plus `workflow_dispatch`) that runs `make cucumber-test WITH_CODE_COVERAGE=true` and `make mutation-test` (full, unscoped), assembles `public/` via `make result-page`, and deploys it to GitHub Pages.
- Set the master-push workflow's `concurrency.group` so it is scoped to that workflow (e.g. `${{ github.workflow }}-${{ github.ref }}`) and set `cancel-in-progress: true`, so a push that supersedes a still-running master-push run cancels it instead of letting it publish a report for a commit `master` has already moved past.
- Gate the master-push workflow's `cucumber-test`/`mutation-test` jobs on a path filter that checks for changes to source code, tests/specs, or CI/report tooling (workflow files, `Makefile`, `scripts/**`) — skip the whole run when the push touched none of those.
- Publish README badges as shields.io endpoint badges sourced from the master-push workflow's published `*-badge.json` files — never from a PR run, never hand-authored.

### SHOULD
- Add a cheap pre-check job (e.g. verifying the PR branch is already up to date with `master`) before the cucumber/mutation jobs, and skip them when it fails — GitHub can already require an up-to-date branch before merge, so paying for a full test/mutation run on a stale branch is wasted CI time.

### MUST NOT
- Collect coverage (`WITH_CODE_COVERAGE=true`) on the PR-gate workflow.
- Let the master-push workflow's `mutation-test` job fail/block the workflow on a low score — it only reports; the PR-gate workflow is where that threshold is enforced.
- Call a stack's native test/coverage/mutation CLI directly from either workflow instead of the project's `make` targets.
- Run the master-push workflow's report jobs on a push that only changed files outside source/tests/CI (e.g. docs) — the path filter must skip them.
- Let two master-push runs for the same branch execute concurrently, or let an outdated one keep running (and eventually publish) after a newer push has already superseded it.

# Anti-patterns
- **Pushing directly to `master` or `develop`**
  - Consequence: unvalidated changes enter the protected branch, breaking the stable version and bypassing review history.
  - Instead: always create a separate branch, let the workflow run, and open a PR.

- **Skipping the version check for a code-changing PR to `master`**
  - Consequence: releases are built with duplicate or stale versions, confusing the release process and artifacts.
  - Instead: add a `version-check` job that compares the PR version with the version in `origin/master`.

- **Not bumping the version when the CI workflow changes**
  - Consequence: a new validation logic, build environment, or test matrix is released under an existing version, making it impossible to trace which version introduced the CI behavior. Reproducing an old release may produce different results because the workflow changed silently.
  - Instead: include workflow file changes (`.github/workflows/pr.yml`) in the `version-check` job conditions so that any CI change targeting `master` also requires a version bump.

- **Testing cross-platform console apps on a single OS only**
  - Consequence: OS-specific bugs are only discovered by end users.
  - Instead: use an OS matrix for console applications.

- **Failing to enable long paths on Windows when cloning Git dependencies**
  - Consequence: `git clone` or `pip install` from a Git URL fails with errors like `cannot create directory ... Filename too long`, especially for paths containing curly braces or other special characters.
  - Instead: add `git config --global core.longpaths true` before the clone or install step on Windows runners.

- **Comparing versions as strings**
  - Consequence: `"10.0.0"` is considered less than `"2.0.0"` with string comparison, leading to false positives or missed errors.
  - Instead: use semantic version comparison (`packaging.Version` in Python, `System.Version` in .NET, `semver` in Node.js, etc.).

- **Requiring a version bump for every PR, including `develop` PRs and documentation changes**
  - Consequence: unnecessary version bumps, noisy history, and slower development.
  - Instead: check the version only for PRs to `master`/`main` and only when code or the workflow has changed.

- **Not adding a final aggregate `test:` job that wraps the (possibly conditional) test job(s)**
  - Consequence: if branch protection requires the matrix/test job directly (e.g. `test-matrix`) and that job is skipped by a path filter (no `src`/`tests` changes on this PR), GitHub's required status check for it never reports success — the check stays pending or is reported as failing, blocking the PR merge even though nothing needed to be tested.
  - Instead: add a final `test:` job with `needs: [..., test-matrix]` and `if: always()` that inspects `needs.test-matrix.result`, fails only on an actual `failure`, and treats `skipped`/`success` as passing (see [Python PR workflow example](./templates/python-pr.example.md)). Set this aggregate job — not the underlying matrix job — as the required status check in branch protection.

- **Collecting coverage on the PR-gate workflow**
  - Consequence: every PR pays coverage instrumentation overhead for a metric that is a trend/floor check, not something that needs per-PR feedback — the gate gets slower for no proportional benefit.
  - Instead: run `make cucumber-test` without `WITH_CODE_COVERAGE` on the PR-gate workflow; collect coverage only on the master-push workflow via `make cucumber-test WITH_CODE_COVERAGE=true`.

- **Running `make mutation-test` without `ONLY_DELTA=true` on the PR-gate workflow**
  - Consequence: PR feedback takes as long as a full-project mutation run, and the team eventually disables or ignores the check under deadline pressure.
  - Instead: pass `ONLY_DELTA=true DELTA_BASE=<PR base ref/SHA>` on the PR-gate workflow; the master-push workflow runs the unscoped, full command.

- **Letting the master-push workflow's `mutation-test` job fail the workflow**
  - Consequence: a threshold already enforced pre-merge now blocks the trunk itself, with no PR left to fix in response — a broken `master` and no clear path to green.
  - Instead: keep this job report-only; it uploads its report and mutation score without gating anything.

- **Hand-authoring README badges instead of shields.io endpoint badges sourced from the Pages site**
  - Example: a coverage percentage typed directly into the README as a static shields.io badge URL.
  - Consequence: the badge silently drifts from reality — nothing regenerates it when the number changes.
  - Instead: point the badge at `https://img.shields.io/endpoint?url=<pages-url>/<label>-badge.json`, a file only the master-push workflow's `make result-page` step ever writes.

- **No concurrency control on the master-push workflow, or `cancel-in-progress: false`**
  - Example: two pushes to `master` five minutes apart both run the full report pipeline to completion.
  - Consequence: the first run keeps burning CI minutes computing a report for a commit that is no longer on `master`, and — since both runs eventually reach `deploy` — the second, correct deployment can be overwritten by the first, stale one finishing later, depending on which `deploy` job completes last.
  - Instead: set `concurrency: { group: ${{ github.workflow }}-${{ github.ref }}, cancel-in-progress: true }` at the workflow level, so a newer push cancels the outdated run immediately.

- **Running the report pipeline on every push to `master`, including docs-only changes**
  - Example: a push that only edits `README.md`'s prose (not a badge) still runs `cucumber-test`, `mutation-test`, and redeploys Pages.
  - Consequence: CI minutes and mutation-testing time are spent producing a report that is byte-for-byte identical to the one already published.
  - Instead: gate `cucumber-test`/`mutation-test` on a path filter (source, tests/specs, or CI/report tooling); let `result-page`/`deploy` cascade-skip when there is nothing to report.

# Example

See [Python PR workflow example](./templates/python-pr.example.md) for a Python project using `pyproject.toml`.

See [Report-publishing workflow example](./templates/report-publish.example.md) for the master-push workflow (Cucumber + coverage + mutation testing + GitHub Pages + badges) of a project following [[skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md|bdd-coverage-mutation-testing]].

# Check list
- [ ] Workflow triggers on PR to `develop` and `master` (or `main`).
- [ ] The `test` job runs unit tests and uses an OS matrix for cross-platform console apps.
- [ ] The `version-check` job runs only for PRs to `master`/`main` when code or the workflow changed.
- [ ] Versions are compared semantically and must strictly increase.
- [ ] The AI agent created a separate branch and PR; no direct push to `develop`/`master`.
- [ ] The workflow contains no secrets, tokens, or deployment steps.
- [ ] For non-Python stacks, the version file and reading method are adapted accordingly.
- [ ] Windows jobs that clone Git repositories or install packages from Git URLs enable `core.longpaths`.
- [ ] For projects following bdd-coverage-mutation-testing: the PR-gate workflow runs `make cucumber-test` and `make mutation-test ONLY_DELTA=true DELTA_BASE=<base>`, without coverage.
- [ ] For projects following bdd-coverage-mutation-testing: a master-push workflow runs `make cucumber-test WITH_CODE_COVERAGE=true` and `make mutation-test` (full), publishes `public/` (via `make result-page`) to GitHub Pages, and never fails on mutation score.
- [ ] The master-push workflow sets `concurrency: { group: ${{ github.workflow }}-${{ github.ref }}, cancel-in-progress: true }` so a newer push cancels an outdated, still-running one.
- [ ] The master-push workflow's report jobs are gated on a path filter (source, tests/specs, CI/report tooling) and are skipped for unrelated pushes (e.g. docs-only).
- [ ] README badges are shields.io endpoint badges sourced from the published Pages site, regenerated only by the master-push workflow.
