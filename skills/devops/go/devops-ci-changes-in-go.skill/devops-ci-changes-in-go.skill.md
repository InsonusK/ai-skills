---
name: devops-ci-changes-in-go
description: Go implementation of devops-ci-changes — the ready check-changes composite action whose test category follows the Go test layout: `*_test.go` files, `{package}/test/`, `{package}/features/`, `testdata/`
whenToUse: when a Go project needs `.github/actions/check-changes/action.yml`, or when you review that file in a Go project
updated: 20261010
tags:
  - stack/go
  - concern/ci
  - github-actions
---

# Goal
- `.github/actions/check-changes/action.yml`, an unchanged copy of this skill's asset.
- Every test file of the project inside the layout the action's `test` category matches: `*_test.go` files, `{package}/test/`, `{package}/features/`, `testdata/`.

# Core Principle
- This skill details [[skills/devops/core/devops-ci-changes.skill.md|devops-ci-changes]] for Go; apply both.
- The `test` patterns are the layout the Go testing skills produce, never a guess at where tests may live.

# Rule

## MUST

### Copy action.yml verbatim
Copy [[./assets/.github/actions/check-changes/action.yml|action.yml]] verbatim to `.github/actions/check-changes/action.yml`; do not modify it.
- Risk: an edited pattern is not covered by the path cases the asset was run against.
- Fix: restore the file from the asset.

### Keep tests inside the matched layout
Place every test file where the `test` category matches it: `*_test.go` files, `{package}/test/`, `{package}/features/`, `testdata/`.
- Violation: a `helpers.go` used only by tests, kept beside the production code instead of in `{package}/test/`.
- Risk: the file counts as `code`, so a test-only pull request into `master` demands a version bump and a push publishes a release.
- Fix: move the file into the layout; when the project keeps a different layout on purpose, tell the user.

# Check list
- [ ] `.github/actions/check-changes/action.yml` is byte-identical to this skill's asset.
- [ ] A change of one test file sets `test` and leaves `code` unset.
- [ ] A change of one source file sets `code` and leaves `test` unset.
