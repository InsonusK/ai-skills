---
name: devops-github-wf-bdd-report-publish
description: PR-gate Cucumber/mutation-testing jobs and the master-push workflow that publishes coverage/mutation reports and README badges to GitHub Pages, for projects following solution-conformance-testing.
whenToUse: when a project that follows [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] needs its `make unit-test`/`make mutation-test`/`make test-report` targets wired into CI — a PR-gate workflow that blocks merge, and a master-push workflow that publishes coverage/mutation reports and badges to GitHub Pages.
tags:
  - concern/ci
  - github-actions
  - concern/testing/bdd
  - cucumber
  - concern/testing/mutation
  - github-pages
  - badges
  - concern/testing
  - stack

---

# Goal
- Give every project following [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] the same CI wiring for its `make` contract: a PR-gate workflow that blocks merge on Cucumber/mutation results, and a master-push workflow that publishes full coverage/mutation reports and README badges via GitHub Pages.
- Keep that wiring identical across stacks — only the underlying `make` targets differ per project, never the job structure.

# Core Principle
- This skill assumes [[skills/devops/devops-github-wf-pr-validation.skill/devops-github-wf-pr-validation.skill.md|devops-github-wf-pr-validation]] already owns the base `pull_request`/`push` triggers, the `test` job, and the aggregate-job pattern for branch protection. The jobs below are added on top of that — this skill does not redefine triggers or the base `test` job.
- This workflow only ever calls the project's `make unit-test`/`make mutation-test`/`make test-report` targets — it never invokes a stack's native test/coverage/mutation CLI directly, and it never re-decides what those targets do.
- Two separate workflows, not one, own the BDD gate: a **PR-gate** workflow (`pull_request` → `master`) that blocks merge, and a **master-push** workflow (`push` → `master`) that only publishes reports/badges and never blocks anything.
- Coverage is collected only on the master-push workflow, never on the PR-gate workflow — coverage is a trend/floor check, not per-PR-critical feedback, and skipping its instrumentation overhead on PRs keeps that gate fast for what does need per-PR feedback: mutation testing scoped to the exact lines the PR changed.
- A master-push run reports on one specific commit. If a second push to `master` lands before the first run finishes, the first run's eventual report would describe code that is no longer on `master` — so the superseded run must be cancelled, not left to finish and publish stale numbers.
- Report publishing is not free CI time, so it must not run on every push to `master` — only when the push actually changed something the reports could reflect (source code, tests/specs, or the CI/report tooling itself).

# Workflow structure

## PR-gate workflow (`pull_request` → `master`)
- A `unit-test` job runs `make unit-test` (no coverage).
- A `mutation-test` job runs `make mutation-test ONLY_DELTA=true DELTA_BASE=<PR base ref/SHA>`, scoping mutation testing to code the PR actually changed. Give it full git history (`fetch-depth: 0`) so the delta diff has a base to compare against.
- Both jobs feed the same aggregate-job pattern required by [[skills/devops/devops-github-wf-pr-validation.skill/devops-github-wf-pr-validation.skill.md|devops-github-wf-pr-validation]]'s `## MUST` rule to add a final aggregate `test:` job — the aggregate job is what branch protection requires, not `unit-test`/`mutation-test` directly.
- Coverage is not collected here — see [# Core Principle](#core-principle) for why.

## Master-push workflow (`push` → `master`, plus `workflow_dispatch`)
A second, separate workflow file (e.g. `.github/workflows/report-publish.yml`) that only reports and publishes — it never fails the push itself:
- `permissions: contents: read, pages: write, id-token: write`.
- `concurrency: { group: <workflow-name>-<ref>, cancel-in-progress: true }` at the workflow level (e.g. `group: ${{ github.workflow }}-${{ github.ref }}`) — a push that lands while the previous master-push run is still going cancels that run outright instead of letting it finish and publish a report for a commit `master` has already moved past. This trades a small chance of interrupting an in-flight `deploy` step (GitHub's official Pages template avoids that by never cancelling) for never publishing stale numbers; the deploy step is short, and the next successful run republishes cleanly, so the trade-off favors cancellation here.
- A `changes` job (path filter, e.g. `dorny/paths-filter@v3`) runs first and gates every job below on whether the push touched source code, tests/specs (`.feature`/step-definitions/`test/`), or CI/report tooling (`.github/workflows/**`, `Makefile`, `scripts/**`) — a push that only touches docs or unrelated files must not trigger a report run at all.
- A `unit-test` job (`if:` the `changes` job found a relevant change) runs `make unit-test WITH_CODE_COVERAGE=true` and uploads `tmp/result` plus the native report directories it produced (e.g. `tmp/report/tests`, `tmp/report/coverage`) as a build artifact.
- A `mutation-test` job (same `if:`) runs `make mutation-test` (no `ONLY_DELTA` — the full project) and uploads `tmp/result` plus `tmp/report/mutation` as a build artifact. This job's mutation score does not gate anything — the PR-gate workflow already enforced the threshold before this code reached `master`.
- A `test-report` job depends on both, downloads their artifacts back into `tmp/`, runs `make test-report` to assemble `public/`, and uploads `public/` as a Pages artifact (`actions/upload-pages-artifact@v3`). It cascade-skips automatically when `unit-test`/`mutation-test` were skipped by the path filter.
- A `deploy` job deploys `public/` to GitHub Pages (`actions/deploy-pages@v4`); it cascade-skips the same way.

## Badges
- `make test-report` writes one `<label>-badge.json` file per metric into `public/` (shields.io's [endpoint badge](https://shields.io/badges/endpoint-badge) schema: `{"schemaVersion":1,"label":"...","message":"...","color":"..."}`), alongside the native reports it copies in.
- README badges reference those files as shields.io endpoint badges: `https://img.shields.io/endpoint?url=<pages-url>/<label>-badge.json` — never a hand-authored static badge.
- Each badge links to its full native report on the same Pages site, not just to the raw JSON.
- README also keeps the native GitHub Actions status badge for the PR-gate workflow itself (`.../actions/workflows/<pr-workflow-file>/badge.svg`), alongside the shields.io endpoint badges.

# Rule

## MUST
- Run `make unit-test` and `make mutation-test ONLY_DELTA=true DELTA_BASE=<PR base>` as required jobs of the PR-gate workflow, feeding the same aggregate-job pattern used for plain unit tests.
  - Violation: the PR-gate workflow runs `make mutation-test` without `ONLY_DELTA=true`.
  - Risk: PR feedback takes as long as a full-project mutation run, and the team eventually disables or ignores the check under deadline pressure.
  - Fix: pass `ONLY_DELTA=true DELTA_BASE=<PR base ref/SHA>` on the PR-gate workflow; the master-push workflow runs the unscoped, full command.
- Add a separate workflow triggered on `push` to `master` (plus `workflow_dispatch`) that runs `make unit-test WITH_CODE_COVERAGE=true` and `make mutation-test` (full, unscoped), assembles `public/` via `make test-report`, and deploys it to GitHub Pages.
  - Risk: without a dedicated master-push workflow, either coverage/mutation reports never get generated and published, or the PR-gate workflow gets burdened with the full, unscoped runs, slowing down every PR.
  - Fix: add a second workflow file, triggered on `push` to `master` and `workflow_dispatch`, that runs the full `make` targets and publishes the result to GitHub Pages.
- Set the master-push workflow's `concurrency.group` so it is scoped to that workflow (e.g. `${{ github.workflow }}-${{ github.ref }}`) and set `cancel-in-progress: true` — never let two master-push runs for the same branch execute concurrently, or let an outdated one keep running (and eventually publish) after a newer push has already superseded it.
  - Violation: no concurrency control, or `cancel-in-progress: false` — two pushes to `master` five minutes apart both run the full report pipeline to completion.
  - Risk: the first run keeps burning CI minutes computing a report for a commit that is no longer on `master`, and — since both runs eventually reach `deploy` — the second, correct deployment can be overwritten by the first, stale one finishing later, depending on which `deploy` job completes last.
  - Fix: set `concurrency: { group: ${{ github.workflow }}-${{ github.ref }}, cancel-in-progress: true }` at the workflow level, so a newer push cancels the outdated run immediately.
- Gate the master-push workflow's `unit-test`/`mutation-test` jobs on a path filter that checks for changes to source code, tests/specs, or CI/report tooling (workflow files, `Makefile`, `scripts/**`) — skip the whole run when the push touched none of those.
  - Violation: a push that only edits `README.md`'s prose (not a badge) still runs `unit-test`, `mutation-test`, and redeploys Pages.
  - Risk: CI minutes and mutation-testing time are spent producing a report that is byte-for-byte identical to the one already published.
  - Fix: gate `unit-test`/`mutation-test` on the path filter; let `test-report`/`deploy` cascade-skip when there is nothing to report.
- Publish README badges as shields.io endpoint badges sourced from the master-push workflow's published `*-badge.json` files — never from a PR run, never hand-authored.
  - Violation: a coverage percentage typed directly into the README as a static shields.io badge URL.
  - Risk: the badge silently drifts from reality — nothing regenerates it when the number changes.
  - Fix: point the badge at `https://img.shields.io/endpoint?url=<pages-url>/<label>-badge.json`, a file only the master-push workflow's `make test-report` step ever writes.
- Call the project's `make unit-test`/`make mutation-test`/`make test-report` targets — never a stack's native test/coverage/mutation CLI directly from either workflow.
  - Risk: the workflow now needs stack-specific knowledge, and switching or reconfiguring the underlying tool later becomes a breaking change for every workflow file that calls it directly.
  - Fix: route every CI invocation through the `make` targets defined by [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]].
- Keep the master-push workflow's `mutation-test` job report-only — never let it fail or block the workflow on a low score; the PR-gate workflow is where that threshold is enforced.
  - Risk: a threshold already enforced pre-merge now blocks the trunk itself, with no PR left to fix in response — a broken `master` and no clear path to green.
  - Fix: let this job upload its report and mutation score without gating anything.

## SHOULD
- Add a cheap pre-check job (e.g. verifying the PR branch is already up to date with `master`) before the cucumber/mutation jobs, and skip them when it fails — GitHub can already require an up-to-date branch before merge, so paying for a full test/mutation run on a stale branch is wasted CI time.

# Example
See [Report-publishing workflow example](./templates/report-publish.example.md) for the master-push workflow (Cucumber + coverage + mutation testing + GitHub Pages + badges) of a project following [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]].

# Check list
- [ ] The PR-gate workflow runs `make unit-test` and `make mutation-test ONLY_DELTA=true DELTA_BASE=<base>`, without coverage.
- [ ] A master-push workflow runs `make unit-test WITH_CODE_COVERAGE=true` and `make mutation-test` (full), publishes `public/` (via `make test-report`) to GitHub Pages, and never fails on mutation score.
- [ ] The master-push workflow sets `concurrency: { group: ${{ github.workflow }}-${{ github.ref }}, cancel-in-progress: true }` so a newer push cancels an outdated, still-running one.
- [ ] The master-push workflow's report jobs are gated on a path filter (source, tests/specs, CI/report tooling) and are skipped for unrelated pushes (e.g. docs-only).
- [ ] README badges are shields.io endpoint badges sourced from the published Pages site, regenerated only by the master-push workflow.
- [ ] Every CI job calls the project's `make unit-test`/`make mutation-test`/`make test-report` targets, never a stack's native CLI directly.
