---
name: devops-github-action-check-changes-in-dotnet
description: .NET-specific implementation of the check-changes reusable composite action — dorny/paths-filter patterns for a .NET solution's production-project/test-project/Dockerfile/docs layout
whenToUse: when creating or updating `.github/actions/check-changes/action.yml` in a .NET project
updated: 20260928
tags:
  - stack/dotnet
  - concern/ci
  - github-actions

---

# Scope
This skill adds .NET-specific filter patterns on top of the `check-changes` composite action consumed by [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]] and [[skills/devops/workflows/devops-github-wf-release-test-report.skill/devops-github-wf-release-test-report.skill.md|devops-github-wf-release-test-report]]. It does not cover those workflows' job graphs — only the `action.yml` this skill creates.

# Core Principle
- Test projects follow [solution-dotnet-conformance-testing](skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md): one `*.Tests` project per production project, carrying both plain unit tests and Reqnroll `.feature` files — so "code changed" and "test changed" are split by project-name suffix, not by folder alone.

# Rule

## MUST

### Implement action.yml distinguishing production from test projects
Create `.github/actions/check-changes/action.yml` as a composite action wrapping `dorny/paths-filter@v3`, splitting `*.Tests` projects from production code.
```yaml
name: check-changes
description: Detect which categories of files changed
outputs:
  code:
    value: ${{ steps.code.outputs.code }}
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
    # paths-filter@v3 ORs rules by default, so a standalone negation would match
    # every non-test file. `code` needs AND semantics, which is a step-level
    # setting, hence its own step.
    - uses: dorny/paths-filter@v3
      id: code
      with:
        predicate-quantifier: 'every'
        filters: |
          code:
            - '**/*.{cs,csproj,sln}'
            - '!**/*.Tests/**'
            - '!**/*.Tests.csproj'
    - uses: dorny/paths-filter@v3
      id: filter
      with:
        filters: |
          test:
            - '**/*.Tests/**'
            - '**/*.Tests.csproj'
          workflow:
            - '.github/workflows/**'
            - '.github/actions/**'
          docker:
            - 'Dockerfile'
          docs:
            - 'docs/**'
            - '*.md'
```
- Violation: a single `**/*.cs` filter for `code` with no exclusion for `*.Tests` projects.
- Risk: every scenario/step-definition edit inside a `*.Tests` project is misclassified as a production-code change, forcing `version-check` to fire on PRs that only touched tests.
- Fix: exclude `**/*.Tests/**` and `**/*.Tests.csproj` from `code`, and list them under `test` instead — matching [solution-dotnet-conformance-testing](skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md)'s one-test-project-per-production-project layout.

### Express the `code` exclusion under `predicate-quantifier: 'every'`
Put the `code` filter in its own `paths-filter` step with `predicate-quantifier: 'every'`, one positive pattern, and the `!` exclusions.
- Violation: `'!**/*.Tests/**'` listed next to `'**/*.cs'` in a step with the default `predicate-quantifier: 'some'`.
- Risk: paths-filter ORs the rules, so the negation alone matches every file outside a `*.Tests` project (`README.md`, workflow files); `code` reports `true` on almost every PR and `version-check` fires on docs-only changes.
- Fix: keep the separate `code` step shown above; do not replace it with a negated extglob like `!(**/*.Tests/**)…`, because picomatch's multi-segment negated extglobs misclassify paths such as `src/App.TestsHelpers/X.cs`.

### Composite outputs match the consumer's contract
Expose exactly `code`, `test`, `workflow`, `docker`, `docs` as this action's outputs.
- Risk: a renamed or missing output silently breaks every consumer workflow's `if:` conditions (an unset output is falsy, so the job just never runs).
- Fix: keep the output names exactly as listed above.

# Check list
- [ ] `.github/actions/check-changes/action.yml` exists and wraps `dorny/paths-filter@v3`.
- [ ] `code` excludes `*.Tests` projects via its own step with `predicate-quantifier: 'every'` (no `!` rule under the default `some`); `test` matches only `*.Tests` projects.
- [ ] `workflow`/`docker`/`docs` filters match `.github/workflows/`+`.github/actions/`, `Dockerfile`, `docs/`+`*.md` respectively.
- [ ] The action's outputs are named `code`, `test`, `workflow`, `docker`, `docs`.
