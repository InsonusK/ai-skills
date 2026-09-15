---
name: devops-github-action-check-changes-in-go
description: Go-specific implementation of the check-changes reusable composite action — dorny/paths-filter patterns for a Go module's source/test/features/Dockerfile/docs layout
whenToUse: when creating or updating `.github/actions/check-changes/action.yml` in a Go project
updated: 20260915
tags:
  - stack/go
  - concern/ci
  - github-actions

---

# Scope
This skill adds Go-specific filter patterns on top of the `check-changes` composite action consumed by [[skills/devops/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]] and [[skills/devops/devops-github-wf-master-release-report.skill/devops-github-wf-master-release-report.skill.md|devops-github-wf-master-release-report]]. It does not cover those workflows' job graphs — only the `action.yml` this skill creates.

# Core Principle
- Go's own convention splits a package's `*_test.go` files from its production `.go` files in the same directory — "code" vs. "test" is a filename-suffix distinction, not a separate folder tree.

# Rule

## MUST

### Implement action.yml splitting *_test.go from production .go files
Create `.github/actions/check-changes/action.yml` as a composite action wrapping `dorny/paths-filter@v3`.
```yaml
name: check-changes
description: Detect which categories of files changed
outputs:
  code:
    value: ${{ steps.filter.outputs.code }}
  test:
    value: ${{ steps.filter.outputs.test }}
  workflow:
    value: ${{ steps.filter.outputs.workflow }}
  docker:
    value: ${{ steps.filter.outputs.docker }}
  docs:
    value: ${{ steps.filter.outputs.docs }}
runs:
  using: composite
  steps:
    - uses: dorny/paths-filter@v3
      id: filter
      with:
        filters: |
          code:
            - '**/*.go'
            - '!**/*_test.go'
            - 'go.mod'
            - 'go.sum'
          test:
            - '**/*_test.go'
            - 'features/**'
          workflow:
            - '.github/workflows/**'
            - '.github/actions/**'
          docker:
            - 'Dockerfile'
          docs:
            - 'docs/**'
            - '*.md'
```
- Violation: a single `**/*.go` filter for `code` with no `!**/*_test.go` exclusion.
- Risk: every test-only edit is misclassified as a production-code change, forcing `version-check` to fire on PRs that only touched tests.
- Fix: exclude `**/*_test.go` from `code` and list it (plus `features/**`, when the project uses `godog` for Gherkin) under `test`.

### Composite outputs match the consumer's contract
Expose exactly `code`, `test`, `workflow`, `docker`, `docs` as this action's outputs.
- Risk: a renamed or missing output silently breaks every consumer workflow's `if:` conditions (an unset output is falsy, so the job just never runs).
- Fix: keep the output names exactly as listed above.

# Check list
- [ ] `.github/actions/check-changes/action.yml` exists and wraps `dorny/paths-filter@v3`.
- [ ] `code` excludes `**/*_test.go`; `test` matches `**/*_test.go` (and `features/**` if the project uses Gherkin).
- [ ] `workflow`/`docker`/`docs` filters match `.github/workflows/`+`.github/actions/`, `Dockerfile`, `docs/`+`*.md` respectively.
- [ ] The action's outputs are named `code`, `test`, `workflow`, `docker`, `docs`.
