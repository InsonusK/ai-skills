---
name: devops-github-wf-release-test-report
description: Stack-agnostic master-push GitHub Actions workflow that runs every test kind of a project following solution-conformance-testing as a full `report` run, builds the report, and publishes it with its badges to GitHub Pages — without naming a test kind, a tool, or a report file
whenToUse: when a project that follows [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] needs its test kinds (`make test-kinds`, `make test-kind-{kind}`, `make test-report`) published as a full report on every relevant push to master
updated: 20261006
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
- Give every project following [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] the same CI wiring for its `make` contract on `master`: every test kind in a full `report` run, and a published report + README badges on GitHub Pages.
- Keep that wiring identical across stacks — this workflow only ever calls `make` targets, never a stack's native test/coverage/mutation CLI directly.
- Never publish a report for a commit that `master` has already moved past.

# Core Principle
- This is a report workflow: it blocks no merge, and a red kind shows as a red job beside a published report. The merge gate — a `check` run of the same kinds — lives in [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]]; a kind that skips itself in a `check` run (mutation testing) runs only here, after a PR already merged, and only ever as a report.
- What a `report` run adds to a `check` run — coverage reporting, whole-project mutation testing — is decided by each kind, not here: this workflow states `TEST_RUN_PURPOSE=report` and chooses the two directories, nothing else.
- Change detection reuses the same `./.github/actions/check-changes` composite action as `devops-github-wf-pull-request` — never a second, divergent path-filter implementation.
- A push that lands while a previous run of this workflow is still going means that run's eventual report would describe code no longer on `master` — cancel it, don't let it finish and publish stale numbers.
- Report publishing is not free CI time — it must not run on a push that touched nothing the report could reflect (docs-only, unrelated files).

# Workflow
1. Set the repository's Settings → Pages → Source to "GitHub Actions".
2. `changes` job calls `./.github/actions/check-changes`; every job below is gated on `code`, `test`, or `workflow` having changed, or on the run being a manual `workflow_dispatch`.
3. `test-kinds` job reads the kinds from `make test-kinds`.
4. `test-kind` job runs one matrix leg per kind: `make test-kind-{kind}`, then — also after a failure — uploads the kind's one directory, `$TEST_WORK_DIR/kinds/{kind}`, as the artifact `test-kind-{kind}`. A kind that exits non-zero turns its leg red; the report is still built and published.
5. `test-report` job downloads every `test-kind-*` artifact back under `$TEST_WORK_DIR/kinds/`, runs `make test-report` to build `$TEST_REPORT_DIR` (`site/testing`), and uploads `site` as the Pages artifact. It cascade-skips when the kinds were skipped.
6. `deploy` job deploys the site to GitHub Pages; it cascade-skips the same way.
7. Add the workflow badge, the report link, and one badge per declared test badge from the [example](./templates/release-test-report.example.md) to the project's README.

# Rule

## MUST

### Implement from the linked example, not from prose memory
Open and copy [Release-test-report workflow example](./templates/release-test-report.example.md) before writing the workflow file — never reconstruct the YAML from this skill's prose alone. If a real improvement is needed beyond what the example shows, propose it to the user and get it confirmed before shipping it; once confirmed, fold the fix back into the example file.
- Violation: an agent writes the workflow from memory of `# Goal`/`# Core Principle`/`# Rule` without opening the example, and silently drops a mechanical detail the prose only implies, or silently adds its own fix without flagging it.
- Risk: prose is a summary, not a spec — it cannot carry every quoting/escaping/gating detail the working example encodes; an unflagged improvisation might be correct or might be a workaround for a misunderstanding, and nobody reviewing the PR can tell which without asking.
- Fix: read the example first, copy it as the starting point, and treat any deviation as a proposal to confirm with the user — not a silent decision.

### Reuse check-changes, never a second path-filter implementation
Gate this workflow's jobs on `./.github/actions/check-changes`'s output — the same composite action [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]] calls.
- Risk: a second, hand-written filter drifts from the first over time, so the same push is treated as "relevant" by one workflow and "irrelevant" by the other.
- Fix: call `uses: ./.github/actions/check-changes` here exactly as the PR workflow does.

### Call only the project's make targets
Run `make test-kinds`, `make test-kind-{kind}`, and `make test-report` — never a stack's native test/coverage/mutation CLI, and never a kind by name.
- Risk: the workflow now needs stack- or kind-specific knowledge, and adding a test kind or switching a tool later becomes a breaking change for every workflow file that calls it directly.
- Fix: route every CI invocation through the `make` targets defined by [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]].

### State the purpose, nothing else
Set `TEST_RUN_PURPOSE: report`, `TEST_WORK_DIR`, and `TEST_REPORT_DIR` once at workflow level — never a coverage, delta, or tool switch.
- Risk: a switch set here duplicates a decision the test kinds own, and drifts from it when a kind changes.
- Fix: keep the three `env` lines of the [example](./templates/release-test-report.example.md); what a `report` run includes is the kinds' business.

### Publish the report after a failed kind
Upload the kind's directory with `if: !cancelled()` and run `test-report`/`deploy` with `!cancelled()` conditions, so a kind's non-zero exit code turns its job red without stopping the report — never hide that exit code with `continue-on-error`.
- Violation: `continue-on-error: true` on the kind step, or `test-report` with a plain `needs:` and no `if:`.
- Risk: with `continue-on-error` a red test on `master` leaves the workflow green; with a plain `needs:` a failed kind cascade-skips `test-report`/`deploy`, and the report that would show the failure is never published. A kind does not exit non-zero over a score in a `report` run, per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#propagate-the-mutation-tools-exit-code|solution-conformance-testing's contract]].
- Fix: copy the `if:` conditions of the [example](./templates/release-test-report.example.md).

### Cancel a superseded run
Set `concurrency: { group: ${{ github.workflow }}-${{ github.ref }}, cancel-in-progress: true }` at the workflow level.
- Violation: no concurrency control, or `cancel-in-progress: false`.
- Risk: an outdated run keeps burning CI minutes computing a report for a commit `master` has already moved past, and can even overwrite a newer, correct deployment if its `deploy` step finishes last.
- Fix: set the concurrency group above so a newer push cancels the outdated run immediately.

### Gate on relevant changes, skip the rest
Gate `test-kinds` on `check-changes` finding `code`, `test`, or `workflow` changed, or on `github.event_name == 'workflow_dispatch'`; let `test-kind`/`test-report`/`deploy` cascade-skip via `needs` when it is skipped.
- Violation: a push that only edits `README.md`'s prose still runs the full suite and redeploys Pages; or a manual run gated on the path filter alone.
- Risk: CI minutes and mutation-testing time are spent producing a report byte-for-byte identical to the one already published; a manual run on `master` finds no changes against itself and skips every job.
- Fix: gate on the path filter OR a manual dispatch, as in the [example](./templates/release-test-report.example.md); keep the `!cancelled() && needs.….result == 'success'` conditions of the example on `test-report`/`deploy`, which still cascade-skip.

### Enable GitHub Pages from Actions
Set the repository's Settings → Pages → Source to "GitHub Actions" before the first run.
- Risk: with Pages disabled or sourced from a branch, `deploy` fails on every run and no report is published.
- Fix: switch the Pages source once, in the repository settings.

### Add the badges to the README
Add the workflow badge, the report link, and one badge per declared test badge from the [example](./templates/release-test-report.example.md)'s README section to the project's README.
- Violation: the workflow publishes the report but the README carries no badge.
- Risk: the published numbers are never seen by anyone reading the repository.
- Fix: copy the badge lines, replacing `{org}`/`{repo}`, with one `{name}` line per badge `make test-kinds` lists; `make test-readme-check` in the pull-request workflow verifies it.

### Source badges only from this workflow's output
Publish README badges as shields.io endpoint badges reading the `badges/{name}.json` files `make test-report` writes into `$TEST_REPORT_DIR` here — never hand-authored, never sourced from a PR run.
- Violation: a coverage percentage typed directly into the README as a static badge URL.
- Risk: the badge silently drifts from reality — nothing regenerates it when the number changes.
- Fix: point the badge at `https://img.shields.io/endpoint?url=<pages-url>/testing/badges/<name>.json`.

## SHOULD
- Add a cheap pre-check (e.g. confirming there is anything new since the last successful run) before the mutation job, since GitHub can already require an up-to-date branch pre-merge.

# Example
See [Release-test-report workflow example](./templates/release-test-report.example.md).

# Check list
- [ ] The workflow was implemented by copying [Release-test-report workflow example](./templates/release-test-report.example.md), not reconstructed from prose; any deviation was confirmed with the user and folded back into the example.
- [ ] `changes` calls `./.github/actions/check-changes` — the same composite action the PR workflow uses.
- [ ] Every CI job calls the project's `make test-kinds`/`make test-kind-{kind}`/`make test-report` — never a stack's native CLI, never a kind by name.
- [ ] The workflow sets only `TEST_RUN_PURPOSE: report`, `TEST_WORK_DIR`, `TEST_REPORT_DIR`.
- [ ] The kind step has no `continue-on-error`; `test-report`/`deploy` run after a failed kind; each kind's directory is uploaded as `test-kind-{kind}` and restored under `$TEST_WORK_DIR/kinds/` before `make test-report`.
- [ ] `concurrency: { group: ${{ github.workflow }}-${{ github.ref }}, cancel-in-progress: true }` is set.
- [ ] `test-kinds` is gated on the path filter or a manual `workflow_dispatch`; the jobs after it cascade-skip.
- [ ] GitHub Pages source is set to "GitHub Actions".
- [ ] The README carries the workflow badge, the report link, and one badge per declared test badge.
- [ ] README badges are shields.io endpoint badges sourced only from this workflow's published report.
