---
name: devops-ci-changes
description: The five categories of changed files a CI workflow gates its jobs on — code, test, ci, docker, docs — what each category starts, and the check-changes composite action that reports them, shipped ready-made per stack
whenToUse: when a CI workflow must run or skip jobs by what a pull request or a push changed, or when you add or review `.github/actions/check-changes/action.yml`
updated: 20261010
tags:
  - stack
  - concern/ci
  - github-actions
---

# Goal
- `.github/actions/check-changes/action.yml` in the project, an unchanged copy of the stack extension's asset.
- Every workflow reading changed files only through that action's outputs `code`, `test`, `ci`, `docker`, `docs`.
- Every job gated by the categories in [[#Gate each job by its categories]].

# Core Principle
- **Unknown means code** - `code` is every file no other category claims, so a file nobody classified starts the tests and a release instead of being skipped.
- A file belongs to at most one category; repository housekeeping (`.gitignore`, editor settings) belongs to none.
- **Stack extensions** - Only the `test` patterns differ between stacks; the ready action for a stack is in `devops-ci-changes-in-go`, `devops-ci-changes-in-python`, `devops-ci-changes-in-typescript`, `devops-ci-changes-in-angular`, `devops-ci-changes-in-dotnet`; ask the user which one to load.

# Rule

## MUST

### Copy the stack's action verbatim
Copy `action.yml` from the extension for the project's stack to `.github/actions/check-changes/action.yml`, and do not modify it.
- Violation: path patterns typed into a workflow, or an action written from memory of the categories.
- Risk: patterns that were never run against real paths skip the tests for a real change — a `**/name` inside braces does not match `name` at the repository root.
- Fix: copy the asset; when the project's layout differs from the stack's, tell the user instead of editing the patterns.

### Gate each job by its categories
Start each job only for the categories in this table.

| Category | Holds | In a pull request starts | On a push starts |
| --- | --- | --- | --- |
| `code` | what goes into the artifact: sources, manifests, the version source, any unclassified file | tests, the version check into `master`, the image build | tests, delivery |
| `test` | tests, features, `tools/` except `tools/version/`, the report template | tests | tests |
| `ci` | `.github/`, `.devcontainer/`, `tools/version/`, `Makefile` | tests | tests |
| `docker` | `Dockerfile`, `.dockerignore` | the version check into `master`, the image build | delivery |
| `docs` | `docs/`, `*.md`, `LICENSE` | nothing | nothing |

- Violation: the version check also demanded for `test` or `ci`, or delivery started by `ci`.
- Risk: a change that alters nothing shipped forces a version bump and publishes a release identical to the previous one.
- Fix: copy the conditions of the workflow templates; they follow this table.

### Read the raw outputs in each job
Expose the five outputs of the action unchanged from the `changes` job, and combine them in the `if:` of the job that needs them.
- Violation: a `relevant` output computed inside the `changes` job.
- Risk: the next job with a different combination needs a second `changes` job or a second combined output.
- Fix: `if: needs.changes.outputs.code == 'true' || needs.changes.outputs.docker == 'true'` on the consuming job.

# Check list
- [ ] `.github/actions/check-changes/action.yml` is byte-identical to the stack extension's asset.
- [ ] No workflow contains a path pattern of its own.
- [ ] Each job's `if:` uses the categories of [[#Gate each job by its categories]].
- [ ] The `changes` job exposes `code`, `test`, `ci`, `docker`, `docs` unchanged.
