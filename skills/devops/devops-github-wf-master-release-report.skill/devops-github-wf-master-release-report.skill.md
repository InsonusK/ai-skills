---
name: devops-github-wf-master-release-report
description: Stack-agnostic master-push GitHub Actions workflow that runs the full (unscoped) unit-test-with-coverage and mutation-test suite for a project following solution-conformance-testing, assembles the report, and publishes coverage/mutation reports and README badges to GitHub Pages
whenToUse: when a project that follows [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] needs its `make unit-test`/`make mutation-test`/`make test-report` targets published as a full report on every relevant push to master
updated: 20260915
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
- Give every project following [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] the same CI wiring for its `make` contract on `master`: full coverage-instrumented unit tests, full (unscoped) mutation testing, and a published report + README badges on GitHub Pages.
- Keep that wiring identical across stacks — this workflow only ever calls `make` targets, never a stack's native test/coverage/mutation CLI directly.
- Never publish a report for a commit that `master` has already moved past.

# Core Principle
- This is a report-only workflow: it never blocks anything. The merge gate — `make unit-test` and the delta-scoped `make mutation-test` — lives in [[skills/devops/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]]; this workflow only runs after a PR already merged.
- Coverage is collected here, never on the PR-gate workflow — coverage is a trend/floor concern, not per-PR-critical feedback, and skipping its instrumentation overhead on PRs keeps that gate fast.
- Change detection reuses the same `./.github/actions/check-changes` composite action as `devops-github-wf-pull-request` — never a second, divergent path-filter implementation.
- A push that lands while a previous run of this workflow is still going means that run's eventual report would describe code no longer on `master` — cancel it, don't let it finish and publish stale numbers.
- Report publishing is not free CI time — it must not run on a push that touched nothing the report could reflect (docs-only, unrelated files).

# Workflow
1. `changes` job calls `./.github/actions/check-changes`; every job below is gated on `code`, `test`, or `workflow` having changed.
2. `unit-test` job runs `make unit-test WITH_CODE_COVERAGE=true`, uploading `tmp/result` plus `tmp/report/tests`/`tmp/report/coverage` as a build artifact.
3. `mutation-test` job runs `make mutation-test` (full, unscoped — no `ONLY_DELTA`), uploading `tmp/result` plus `tmp/report/mutation`. Its score never gates anything; the PR-gate workflow already enforced the threshold before this code reached `master`.
4. `test-report` job downloads both artifacts back into `tmp/`, runs `make test-report` to assemble `public/`, and uploads it as a Pages artifact. It cascade-skips when `unit-test`/`mutation-test` were skipped.
5. `deploy` job deploys `public/` to GitHub Pages; it cascade-skips the same way.

# Rule

## MUST

### Implement from the linked example, not from prose memory
Open and copy [Master-release-report workflow example](./templates/master-release-report.example.md) before writing the workflow file — never reconstruct the YAML from this skill's prose alone. If a real improvement is needed beyond what the example shows, propose it to the user and get it confirmed before shipping it; once confirmed, fold the fix back into the example file.
- Violation: an agent writes the workflow from memory of `# Goal`/`# Core Principle`/`# Rule` without opening the example, and silently drops a mechanical detail the prose only implies, or silently adds its own fix without flagging it.
- Risk: prose is a summary, not a spec — it cannot carry every quoting/escaping/gating detail the working example encodes; an unflagged improvisation might be correct or might be a workaround for a misunderstanding, and nobody reviewing the PR can tell which without asking.
- Fix: read the example first, copy it as the starting point, and treat any deviation as a proposal to confirm with the user — not a silent decision.

### Reuse check-changes, never a second path-filter implementation
Gate this workflow's jobs on `./.github/actions/check-changes`'s output — the same composite action [[skills/devops/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]] calls.
- Risk: a second, hand-written filter drifts from the first over time, so the same push is treated as "relevant" by one workflow and "irrelevant" by the other.
- Fix: call `uses: ./.github/actions/check-changes` here exactly as the PR workflow does.

### Call only the project's make targets
Run `make unit-test WITH_CODE_COVERAGE=true`, `make mutation-test`, and `make test-report` — never a stack's native test/coverage/mutation CLI directly.
- Risk: the workflow now needs stack-specific knowledge, and switching or reconfiguring the underlying tool later becomes a breaking change for every workflow file that calls it directly.
- Fix: route every CI invocation through the `make` targets defined by [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]].

### Always collect coverage here, never on the PR gate
Run `unit-test` with `WITH_CODE_COVERAGE=true` unconditionally in this workflow.
- Risk: without coverage collected somewhere, mutation testing has nothing to scope against and "was this even executed" is never answered.
- Fix: collect it here; keep it off the PR-gate workflow for speed.

### Keep the mutation-test job report-only
Set `continue-on-error: true` on the `Run mutation tests` step (`make mutation-test`) — never let its exit code fail the job or block this workflow.
- Violation: running `make mutation-test` with no `continue-on-error`, relying on the surrounding prose/intent alone to keep it "report-only."
- Risk: `make mutation-test` exits with the underlying mutation tool's own exit code — non-zero the moment one mutant survives, per [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#propagate-the-mutation-tools-exit-code|solution-conformance-testing's contract]]. Without `continue-on-error`, that failure fails the `mutation-test` job itself — and since `test-report`'s `needs` has no `if: always()`, GitHub cascade-skips `test-report`/`deploy` entirely instead of merely reporting a low score, so one surviving mutant silently stops the whole report/Pages pipeline from publishing anything, coverage included.
- Fix: add `continue-on-error: true` to the mutation-test step itself, as shown in [example](./templates/master-release-report.example.md); the job still uploads its report and normalized score artifact regardless of the tool's exit code, and the PR-gate workflow already enforced the threshold pre-merge.

### Cancel a superseded run
Set `concurrency: { group: ${{ github.workflow }}-${{ github.ref }}, cancel-in-progress: true }` at the workflow level.
- Violation: no concurrency control, or `cancel-in-progress: false`.
- Risk: an outdated run keeps burning CI minutes computing a report for a commit `master` has already moved past, and can even overwrite a newer, correct deployment if its `deploy` step finishes last.
- Fix: set the concurrency group above so a newer push cancels the outdated run immediately.

### Gate on relevant changes, skip the rest
Gate `unit-test`/`mutation-test` on `check-changes` finding `code`, `test`, or `workflow` changed; let `test-report`/`deploy` cascade-skip via `needs` when they are.
- Violation: a push that only edits `README.md`'s prose still runs the full suite and redeploys Pages.
- Risk: CI minutes and mutation-testing time are spent producing a report byte-for-byte identical to the one already published.
- Fix: gate on the path filter; do not add a separate `if:` to `test-report`/`deploy` — let cascade-skip handle it.

### Source badges only from this workflow's output
Publish README badges as shields.io endpoint badges reading the `*-badge.json` files `make test-report` writes into `public/` here — never hand-authored, never sourced from a PR run.
- Violation: a coverage percentage typed directly into the README as a static badge URL.
- Risk: the badge silently drifts from reality — nothing regenerates it when the number changes.
- Fix: point the badge at `https://img.shields.io/endpoint?url=<pages-url>/<label>-badge.json`.

## SHOULD
- Add a cheap pre-check (e.g. confirming there is anything new since the last successful run) before the mutation job, since GitHub can already require an up-to-date branch pre-merge.

# Example
See [Master-release-report workflow example](./templates/master-release-report.example.md).

# Check list
- [ ] The workflow was implemented by copying [Master-release-report workflow example](./templates/master-release-report.example.md), not reconstructed from prose; any deviation was confirmed with the user and folded back into the example.
- [ ] `changes` calls `./.github/actions/check-changes` — the same composite action the PR workflow uses.
- [ ] Every CI job calls the project's `make unit-test`/`make mutation-test`/`make test-report` — never a stack's native CLI directly.
- [ ] `unit-test` always runs with `WITH_CODE_COVERAGE=true`.
- [ ] `mutation-test` runs the full, unscoped run and never fails the workflow on score — its `Run mutation tests` step has `continue-on-error: true`.
- [ ] `concurrency: { group: ${{ github.workflow }}-${{ github.ref }}, cancel-in-progress: true }` is set.
- [ ] `unit-test`/`mutation-test` are gated on the path filter; `test-report`/`deploy` cascade-skip rather than repeat the condition.
- [ ] README badges are shields.io endpoint badges sourced only from this workflow's published `public/`.
