---
name: devops-ci-changes-in-angular
description: Angular implementation of devops-ci-changes — the ready check-changes composite action whose test category follows the Angular test layout: `test/`, `features/`, and `spec/` folders beside the code, `*.spec.ts` files, and `-e2e` projects
whenToUse: when a Angular project needs `.github/actions/check-changes/action.yml`, or when you review that file in a Angular project
updated: 20261010
tags:
  - stack/typescript
  - framework/angular
  - concern/ci
  - github-actions
---

# Goal
- `.github/actions/check-changes/action.yml`, an unchanged copy of this skill's asset.
- **Tests in the layout** - Every test file of the project inside the layout the action's `test` category matches: `test/`, `features/`, and `spec/` folders beside the code, `*.spec.ts` files, and `-e2e` projects.

# Core Principle
- This skill details [[skills/devops/core/devops-ci-changes.skill.md|devops-ci-changes]] for Angular; apply both.
- The `test` patterns are the layout the Angular testing skills produce, never a guess at where tests may live.

# Rule

## MUST

### Copy action.yml verbatim
Copy [[./assets/.github/actions/check-changes/action.yml|action.yml]] verbatim to `.github/actions/check-changes/action.yml`; do not modify it.
- Risk: an edited pattern is not covered by the path cases the asset was run against.
- Fix: restore the file from the asset.

### Keep tests inside the matched layout
Place every test file where the `test` category matches it: `test/`, `features/`, and `spec/` folders beside the code, `*.spec.ts` files, and `-e2e` projects.
- Violation: a test harness `cart.harness.ts` beside the component instead of in `spec/`.
- Risk: the file counts as `code`, so a test-only pull request into `master` demands a version bump and a push publishes a release.
- Fix: move the file into the layout; when the project keeps a different layout on purpose, tell the user.

# Check list
- [ ] `.github/actions/check-changes/action.yml` is byte-identical to this skill's asset.
- [ ] A change of one test file sets `test` and leaves `code` unset.
- [ ] A change of one source file sets `code` and leaves `test` unset.
